using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Tests.Infrastructure;
using Konnect.Tests.WebAPI.Authentication.Fixtures;

namespace Konnect.Tests.GraphQL.Companies;

/// <summary>
/// End-to-end coverage for the Company GraphQL surface served from
/// <c>/graphql</c>: the public <c>company(slug)</c> field, the recruiter-
/// scoped <c>recruiter.company</c> field, and the auth gates on the
/// recruiter wrapper. All requests go through the real ASP.NET pipeline
/// against a Testcontainers Postgres so the HotChocolate authorization
/// middleware + JwtBearer scheme are actually exercised.
/// </summary>
[Collection(DatabaseTestSuite.Name)]
public sealed class CompanyGraphQLTests : IDisposable
{
    private readonly PostgresFixture _postgresFixture;
    private readonly KonnectWebApplicationFactory _factory;

    public CompanyGraphQLTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
        _factory = new KonnectWebApplicationFactory
        {
            PostgresConnectionString = postgresFixture.ConnectionString,
        };
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Should_ReturnCompany_When_PublicSlugQuery()
    {
        var (_, _, slug) = await SeedRecruiterWithCompany("Acme Public");
        var client = _factory.CreateClient();

        var json = await PostGraphQL(client, $"{{ company(slug: \"{slug}\") {{ slug name }} }}");

        using var document = JsonDocument.Parse(json);
        var company = document.RootElement.GetProperty("data").GetProperty("company");
        Assert.Equal(slug, company.GetProperty("slug").GetString());
        Assert.Equal("Acme Public", company.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Should_ReturnRecruitersOwnCompany_When_RecruiterScopedQuery()
    {
        var (recruiterId, _, slug) = await SeedRecruiterWithCompany("Acme Owned");
        var client = CreateRecruiterClient(recruiterId);

        var json = await PostGraphQL(client, "{ recruiter { company { slug name } } }");

        using var document = JsonDocument.Parse(json);
        Assert.False(document.RootElement.TryGetProperty("errors", out _));
        var company = document.RootElement
            .GetProperty("data")
            .GetProperty("recruiter")
            .GetProperty("company");
        Assert.Equal(slug, company.GetProperty("slug").GetString());
        Assert.Equal("Acme Owned", company.GetProperty("name").GetString());
    }

    [Fact]
    public async Task Should_ReturnError_When_RecruiterScopedQueryHasNoToken()
    {
        var client = _factory.CreateClient();

        var json = await PostGraphQL(client, "{ recruiter { company { slug } } }");

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Should_ReturnError_When_SeekerTokenAccessesRecruiterScope()
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

        var json = await PostGraphQL(client, "{ recruiter { company { slug } } }");

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("errors", out _));
    }

    private static async Task<string> PostGraphQL(HttpClient client, string query)
    {
        var response = await client.PostAsJsonAsync(
            new Uri("/graphql", UriKind.Relative),
            new { query });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<(Guid RecruiterId, Guid CompanyId, string Slug)> SeedRecruiterWithCompany(string companyName)
    {
        var recruiterId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var slug = $"acme-{Guid.NewGuid():N}";

        await using var dbContext = _postgresFixture.CreateDbContext();
        dbContext.Companies.Add(new Company
        {
            Id = companyId,
            Name = companyName,
            Slug = slug,
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
            audience: KonnectWebApplicationFactory.RecruiterAudience,
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
