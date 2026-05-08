using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Infrastructure.Services.Onboarding;
using Konnect.Tests.Infrastructure;
using Konnect.Tests.WebAPI.Authentication.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Tests.WebAPI.Onboarding;

/// <summary>
/// End-to-end coverage for <c>POST /api/recruiter/onboard</c>: the controller
/// + service + repository chain against a real Testcontainers Postgres.
/// Confirms the post-Auth0 handshake provisions a Company + RecruiterUser in
/// one transaction, is idempotent on replay, rejects audience mismatches via
/// <c>[Authorize]</c>, and surfaces slug collisions as 409.
/// </summary>
[Collection(DatabaseCollection.Name)]
public sealed class RecruiterOnboardingTests : IDisposable
{
    private readonly PostgresFixture postgresFixture;
    private readonly KonnectWebApplicationFactory factory;

    public RecruiterOnboardingTests(PostgresFixture postgresFixture)
    {
        this.postgresFixture = postgresFixture;
        factory = new KonnectWebApplicationFactory
        {
            PostgresConnectionString = postgresFixture.ConnectionString,
        };
    }

    public void Dispose() => factory.Dispose();

    [Fact]
    public async Task Should_Return201_AndPersistCompanyAndRecruiter_When_FirstOnboarding()
    {
        var externalId = Guid.NewGuid();
        var slug = NewSlug();
        var client = CreateClientWithRecruiterToken(externalId, "alice@acme.test");

        var response = await client.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(slug));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using var dbContext = postgresFixture.CreateDbContext();
        var recruiter = await dbContext.Recruiters
            .Include(record => record.Company)
            .FirstOrDefaultAsync(record => record.Id == externalId);

        Assert.NotNull(recruiter);
        Assert.Equal("alice@acme.test", recruiter.Email);
        Assert.Equal(slug, recruiter.Company.Slug);
        Assert.False(recruiter.Company.Verified);
        Assert.NotEqual(default, recruiter.CreatedAt);
        Assert.NotEqual(default, recruiter.Company.CreatedAt);
    }

    [Fact]
    public async Task Should_Return200_When_IdempotentReplay()
    {
        var externalId = Guid.NewGuid();
        var slug = NewSlug();
        var client = CreateClientWithRecruiterToken(externalId, "alice@acme.test");

        var first = await client.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(slug));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(NewSlug(), companyName: "Should Not Apply"));

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        await using var dbContext = postgresFixture.CreateDbContext();
        var company = await dbContext.Companies.FirstAsync(record => record.Slug == slug);
        Assert.Equal("Acme Corp", company.Name);

        var recruiterRowCount = await dbContext.Recruiters
            .Where(record => record.Id == externalId)
            .CountAsync();
        Assert.Equal(1, recruiterRowCount);
    }

    [Fact]
    public async Task Should_Return409_When_SlugAlreadyOwnedByDifferentRecruiter()
    {
        var slug = NewSlug();

        var firstClient = CreateClientWithRecruiterToken(Guid.NewGuid(), "alice@acme.test");
        var firstOnboard = await firstClient.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(slug));
        Assert.Equal(HttpStatusCode.Created, firstOnboard.StatusCode);

        var secondClient = CreateClientWithRecruiterToken(Guid.NewGuid(), "bob@acme-rival.test");
        var collision = await secondClient.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(slug));

        Assert.Equal(HttpStatusCode.Conflict, collision.StatusCode);
    }

    [Fact]
    public async Task Should_Return403_When_SeekerTokenSentToRecruiterEndpoint()
    {
        var externalId = Guid.NewGuid();
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, externalId.ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
                new("email", "seeker@example.test"),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(NewSlug()));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_NoBearerToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            new Uri("/api/recruiter/onboard", UriKind.Relative),
            NewRecruiterInput(NewSlug()));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateClientWithRecruiterToken(Guid externalId, string email)
    {
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.EmployerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, externalId.ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.Recruiter),
                new("email", email),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static OnboardRecruiterInput NewRecruiterInput(string slug, string companyName = "Acme Corp")
        => new(
            CompanyName: companyName,
            CompanySlug: slug,
            CompanyDescription: "We make anvils.",
            CompanyWebsiteUrl: "https://acme.test",
            FirstName: "Alice",
            LastName: "Anderson",
            JobTitle: "Senior Recruiter");

    private static string NewSlug() => $"acme-{Guid.NewGuid():N}";
}
