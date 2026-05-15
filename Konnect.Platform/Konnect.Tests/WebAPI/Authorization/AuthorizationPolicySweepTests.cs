using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Konnect.Infrastructure.Contracts.Companies;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Infrastructure.Services.Onboarding;
using Konnect.Tests.Infrastructure;
using Konnect.Tests.WebAPI.Authentication.Fixtures;

namespace Konnect.Tests.WebAPI.Authorization;

/// <summary>
/// One-stop authorization sweep across every policy-protected endpoint
/// currently shipped: <c>/api/seeker/onboard</c>, <c>/api/recruiter/onboard</c>,
/// <c>/api/recruiter/company</c>, and the GraphQL <c>recruiter</c> subtree.
/// Each protected surface is exercised with the same matrix —
/// no token (401), token from the opposite audience (403), and a token with
/// the right audience claim but the wrong role (403). Happy-path 200s live
/// in the per-feature test classes; this class is the cross-cutting boundary
/// check that guarantees a regression in <c>AddKonnectAuthorization</c> can
/// never silently weaken the policy on any single endpoint.
/// </summary>
[Collection(DatabaseTestSuite.Name)]
public sealed class AuthorizationPolicySweepTests : IDisposable
{
    private readonly KonnectWebApplicationFactory _factory;

    public AuthorizationPolicySweepTests(PostgresFixture postgresFixture)
    {
        _factory = new KonnectWebApplicationFactory
        {
            PostgresConnectionString = postgresFixture.ConnectionString,
        };
    }

    public void Dispose() => _factory.Dispose();

    public static TheoryData<string> RecruiterRestEndpoints => new()
    {
        "/api/recruiter/onboard",
        "/api/recruiter/company",
    };

    public static TheoryData<string> SeekerRestEndpoints => new()
    {
        "/api/seeker/onboard",
    };

    [Theory]
    [MemberData(nameof(RecruiterRestEndpoints))]
    public async Task Should_Return401_When_RecruiterEndpointReceivesNoToken(string endpoint)
    {
        var client = _factory.CreateClient();

        var response = await SendDefaultPayload(client, endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(SeekerRestEndpoints))]
    public async Task Should_Return401_When_SeekerEndpointReceivesNoToken(string endpoint)
    {
        var client = _factory.CreateClient();

        var response = await SendDefaultPayload(client, endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(RecruiterRestEndpoints))]
    public async Task Should_Return403_When_RecruiterEndpointReceivesSeekerToken(string endpoint)
    {
        var token = CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            role: JwtRoles.JobSeeker);
        var client = CreateAuthorizedClient(token);

        var response = await SendDefaultPayload(client, endpoint);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(SeekerRestEndpoints))]
    public async Task Should_Return403_When_SeekerEndpointReceivesRecruiterToken(string endpoint)
    {
        var token = CreateToken(
            audience: KonnectWebApplicationFactory.RecruiterAudience,
            role: JwtRoles.Recruiter);
        var client = CreateAuthorizedClient(token);

        var response = await SendDefaultPayload(client, endpoint);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(RecruiterRestEndpoints))]
    public async Task Should_Return403_When_RecruiterEndpointReceivesRightAudienceButWrongRole(string endpoint)
    {
        // Defence-in-depth: token was issued for the recruiter API but the
        // role claim is JobSeeker. Without the independent role check on the
        // policy this would slip through on aud alone.
        var token = CreateToken(
            audience: KonnectWebApplicationFactory.RecruiterAudience,
            role: JwtRoles.JobSeeker);
        var client = CreateAuthorizedClient(token);

        var response = await SendDefaultPayload(client, endpoint);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(SeekerRestEndpoints))]
    public async Task Should_Return403_When_SeekerEndpointReceivesRightAudienceButWrongRole(string endpoint)
    {
        var token = CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            role: JwtRoles.Recruiter);
        var client = CreateAuthorizedClient(token);

        var response = await SendDefaultPayload(client, endpoint);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnGraphQLError_When_RecruiterScopedQueryHasNoToken()
    {
        var client = _factory.CreateClient();

        var json = await PostRecruiterScopedQuery(client);

        AssertGraphQLHasErrors(json);
    }

    [Fact]
    public async Task Should_ReturnGraphQLError_When_RecruiterScopedQueryReceivesSeekerToken()
    {
        var token = CreateToken(
            audience: KonnectWebApplicationFactory.SeekerAudience,
            role: JwtRoles.JobSeeker);
        var client = CreateAuthorizedClient(token);

        var json = await PostRecruiterScopedQuery(client);

        AssertGraphQLHasErrors(json);
    }

    [Fact]
    public async Task Should_ReturnGraphQLError_When_RecruiterScopedQueryHasRightAudienceButWrongRole()
    {
        var token = CreateToken(
            audience: KonnectWebApplicationFactory.RecruiterAudience,
            role: JwtRoles.JobSeeker);
        var client = CreateAuthorizedClient(token);

        var json = await PostRecruiterScopedQuery(client);

        AssertGraphQLHasErrors(json);
    }

    private string CreateToken(string audience, string role)
        => _factory.TokenFactory.CreateToken(
            audience,
            additionalClaims:
            [
                new(KonnectClaimTypes.ExternalId, Guid.NewGuid().ToString()),
                new(KonnectClaimTypes.Role, role),
                new("email", "principal@example.test"),
            ]);

    private HttpClient CreateAuthorizedClient(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static Task<HttpResponseMessage> SendDefaultPayload(HttpClient client, string endpoint)
        => endpoint switch
        {
            "/api/seeker/onboard" => client.PostAsJsonAsync(
                new Uri(endpoint, UriKind.Relative),
                new OnboardJobSeekerInput(null, null, false)),
            "/api/recruiter/onboard" => client.PostAsJsonAsync(
                new Uri(endpoint, UriKind.Relative),
                new OnboardRecruiterInput(
                    CompanyName: "Acme",
                    CompanySlug: $"acme-{Guid.NewGuid():N}",
                    CompanyDescription: null,
                    CompanyWebsiteUrl: null,
                    FirstName: "Alice",
                    LastName: "Anderson",
                    JobTitle: "Recruiter")),
            "/api/recruiter/company" => client.PutAsJsonAsync(
                new Uri(endpoint, UriKind.Relative),
                new UpdateCompanyInput("Acme", null, null)),
            _ => throw new InvalidOperationException($"No default payload mapped for endpoint '{endpoint}'."),
        };

    private static async Task<string> PostRecruiterScopedQuery(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            new Uri("/graphql", UriKind.Relative),
            new { query = "{ recruiter { company { slug } } }" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    private static void AssertGraphQLHasErrors(string json)
    {
        using var document = JsonDocument.Parse(json);
        Assert.True(
            document.RootElement.TryGetProperty("errors", out _),
            "Expected GraphQL response to contain an 'errors' array for the unauthorized request.");
    }
}
