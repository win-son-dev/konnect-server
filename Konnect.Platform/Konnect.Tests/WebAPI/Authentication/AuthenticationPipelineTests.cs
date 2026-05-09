using System.Net;
using System.Net.Http.Headers;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Tests.WebAPI.Authentication.Fixtures;

namespace Konnect.Tests.WebAPI.Authentication;

/// <summary>
/// End-to-end tests for the auth pipeline: framework JwtBearer →
/// <c>KonnectAuthenticationMiddleware</c> → <c>[Authorize]</c> endpoint.
/// Hits the real <c>/api/me</c> controller through
/// <see cref="KonnectWebApplicationFactory"/>, which overrides JwtBearer to
/// trust tokens minted by <see cref="TestJwtTokenFactory"/>.
/// </summary>
public class AuthenticationPipelineTests(KonnectWebApplicationFactory factory)
    : IClassFixture<KonnectWebApplicationFactory>
{
    [Fact]
    public async Task Should_Return401_When_NoBearerTokenPresent()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return200_When_TokenIsFullyValid()
    {
        var externalId = Guid.NewGuid();
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, externalId.ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
                new("email", "seeker@example.com"),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(externalId.ToString(), body);
        Assert.Contains(JwtRoles.JobSeeker, body);
    }

    [Fact]
    public async Task Should_Return200_When_TokenAudienceIsEmployerSide()
    {
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.EmployerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, Guid.NewGuid().ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.Recruiter),
                new("email", "recruiter@example.com"),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_TokenAudienceIsUnknown()
    {
        var token = factory.TokenFactory.CreateToken(
            audience: "https://api.konnect.test/unknown-audience",
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, Guid.NewGuid().ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_TokenIsExpired()
    {
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, Guid.NewGuid().ToString()),
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
            ],
            notBefore: DateTimeOffset.UtcNow.AddMinutes(-30),
            expires: DateTimeOffset.UtcNow.AddMinutes(-15));

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_TokenIsMissingExternalIdClaim()
    {
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.Role, JwtRoles.JobSeeker),
                new("email", "seeker@example.com"),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_TokenIsMissingRoleClaim()
    {
        var token = factory.TokenFactory.CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, Guid.NewGuid().ToString()),
                new("email", "seeker@example.com"),
            ]);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
