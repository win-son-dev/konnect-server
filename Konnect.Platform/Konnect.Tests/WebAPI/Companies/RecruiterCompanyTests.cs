using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Konnect.Infrastructure.Contracts.Companies;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Tests.Infrastructure;
using Konnect.Tests.WebAPI.Authentication.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Tests.WebAPI.Companies;

/// <summary>
/// End-to-end coverage for <c>PUT /api/recruiter/company</c>: controller +
/// service + repository chain against a real Testcontainers Postgres.
/// Confirms a recruiter updates their own company in place, the slug stays
/// untouched (only Name / Description / WebsiteUrl are editable), and the
/// auth gates reject anonymous + cross-role tokens.
/// </summary>
[Collection(DatabaseTestSuite.Name)]
public sealed class RecruiterCompanyTests : IDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly KonnectWebApplicationFactory _factory;

    public RecruiterCompanyTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new KonnectWebApplicationFactory
        {
            PostgresConnectionString = postgresFixture.ConnectionString,
        };
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_Return200_AndPersistChanges_When_RecruiterUpdatesOwnCompany()
    {
        var (recruiterId, companyId, originalSlug) = await SeedRecruiterWithCompany();
        var client = CreateRecruiterClient(recruiterId);

        var response = await client.PutAsJsonAsync(
            new Uri("/api/recruiter/company", UriKind.Relative),
            new UpdateCompanyInput(
                Name: "Acme Renamed",
                Description: "Renamed description",
                WebsiteUrl: "https://renamed.test"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var dbContext = _postgresFixture.CreateDbContext();
        var refreshed = await dbContext.Companies
            .AsNoTracking()
            .FirstAsync(record => record.Id == companyId);

        Assert.Equal("Acme Renamed", refreshed.Name);
        Assert.Equal("Renamed description", refreshed.Description);
        Assert.Equal("https://renamed.test", refreshed.WebsiteUrl);
        Assert.Equal(originalSlug, refreshed.Slug);
    }

    [Fact]
    public async Task Should_Return403_When_SeekerTokenSentToRecruiterEndpoint()
    {
        var token = _factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, Guid.NewGuid().ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
                new("email", "seeker@example.test"),
            ]);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync(
            new Uri("/api/recruiter/company", UriKind.Relative),
            new UpdateCompanyInput("Whatever", null, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_NoBearerToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            new Uri("/api/recruiter/company", UriKind.Relative),
            new UpdateCompanyInput("Whatever", null, null));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<(Guid RecruiterId, Guid CompanyId, string Slug)> SeedRecruiterWithCompany()
    {
        var recruiterId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var slug = $"acme-{Guid.NewGuid():N}";

        await using var dbContext = _postgresFixture.CreateDbContext();
        dbContext.Companies.Add(new Company
        {
            Id = companyId,
            Name = "Acme Original",
            Slug = slug,
            Description = "Original description",
            WebsiteUrl = "https://original.test",
            Verified = false,
        });
        dbContext.Recruiters.Add(new RecruiterUser
        {
            Id = recruiterId,
            Email = "alice@acme.test",
            CompanyId = companyId,
            FirstName = "Alice",
            LastName = "Anderson",
            JobTitle = "Recruiter",
        });
        await dbContext.SaveChangesAsync();

        return (recruiterId, companyId, slug);
    }

    private HttpClient CreateRecruiterClient(Guid externalId)
    {
        var token = _factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.EmployerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, externalId.ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.Recruiter),
                new("email", "alice@acme.test"),
            ]);

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
