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
/// End-to-end coverage for <c>POST /api/seeker/onboard</c>: the controller
/// + service + repository chain against a real Testcontainers Postgres.
/// </summary>
[Collection(DatabaseTestSuite.Name)]
public sealed class JobSeekerOnboardingTests : IDisposable
{
    private readonly PostgresFixture postgresFixture;
    private readonly KonnectWebApplicationFactory factory;

    public JobSeekerOnboardingTests(PostgresFixture postgresFixture)
    {
        this.postgresFixture = postgresFixture;
        factory = new KonnectWebApplicationFactory
        {
            PostgresConnectionString = postgresFixture.ConnectionString,
        };
    }

    public void Dispose() => factory.Dispose();

    [Fact]
    public async Task Should_Return201_AndPersistJobSeeker_When_FirstOnboarding()
    {
        var externalId = Guid.NewGuid();
        var client = CreateClientWithSeekerToken(externalId, "seeker@example.test");

        var response = await client.PostAsJsonAsync(
            new Uri("/api/seeker/onboard", UriKind.Relative),
            new OnboardJobSeekerInput("Senior Engineer", "Sydney", true));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await using var dbContext = postgresFixture.CreateDbContext();
        var seeker = await dbContext.JobSeekers.FirstOrDefaultAsync(record => record.Id == externalId);
        Assert.NotNull(seeker);
        Assert.Equal("seeker@example.test", seeker.Email);
        Assert.Equal("Senior Engineer", seeker.Headline);
        Assert.Equal("Sydney", seeker.Location);
        Assert.True(seeker.OpenToWork);
        Assert.NotEqual(default, seeker.CreatedAt);
    }

    [Fact]
    public async Task Should_Return200_When_IdempotentReplay()
    {
        var externalId = Guid.NewGuid();
        var client = CreateClientWithSeekerToken(externalId, "seeker@example.test");

        var first = await client.PostAsJsonAsync(
            new Uri("/api/seeker/onboard", UriKind.Relative),
            new OnboardJobSeekerInput("Senior Engineer", "Sydney", true));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync(
            new Uri("/api/seeker/onboard", UriKind.Relative),
            new OnboardJobSeekerInput("Should Not Apply", "Should Not Apply", false));

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        await using var dbContext = postgresFixture.CreateDbContext();
        var seeker = await dbContext.JobSeekers.FirstAsync(record => record.Id == externalId);
        Assert.Equal("Senior Engineer", seeker.Headline);
        Assert.True(seeker.OpenToWork);
    }

    [Fact]
    public async Task Should_Return403_When_RecruiterTokenSentToSeekerEndpoint()
    {
        var externalId = Guid.NewGuid();
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.EmployerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, externalId.ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.Recruiter),
                new("email", "recruiter@acme.test"),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            new Uri("/api/seeker/onboard", UriKind.Relative),
            new OnboardJobSeekerInput(null, null, false));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_NoBearerToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            new Uri("/api/seeker/onboard", UriKind.Relative),
            new OnboardJobSeekerInput(null, null, false));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private HttpClient CreateClientWithSeekerToken(Guid externalId, string email)
    {
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, externalId.ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
                new("email", email),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
