using System.Security.Claims;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.WebAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Konnect.Tests.WebAPI.Middleware;

/// <summary>
/// Unit tests for the claim-validation middleware. These run with a
/// hand-built <see cref="DefaultHttpContext"/> — no Kestrel, no real
/// JwtBearer scheme — because every behavior under test is pure
/// claim-inspection logic. The full pipeline (JwtBearer + this middleware)
/// is exercised in <c>AuthenticationPipelineTests</c>.
/// </summary>
public class KonnectAuthenticationMiddlewareTests
{
    [Fact]
    public async Task Should_PassThrough_When_RequestIsAnonymous()
    {
        var nextCalled = false;
        var middleware = new KonnectAuthenticationMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context, NullLogger<KonnectAuthenticationMiddleware>.Instance);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_AuthenticatedButExternalIdClaimIsMissing()
    {
        var middleware = BuildMiddleware();

        var context = BuildAuthenticatedContext(
            externalIdClaimValue: null,
            roleClaimValue: JwtRoles.JobSeeker);

        await middleware.InvokeAsync(context, NullLogger<KonnectAuthenticationMiddleware>.Instance);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_AuthenticatedButExternalIdClaimIsUnparseable()
    {
        var middleware = BuildMiddleware();

        var context = BuildAuthenticatedContext(
            externalIdClaimValue: "not-a-guid",
            roleClaimValue: JwtRoles.JobSeeker);

        await middleware.InvokeAsync(context, NullLogger<KonnectAuthenticationMiddleware>.Instance);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_AuthenticatedButRoleClaimIsMissing()
    {
        var middleware = BuildMiddleware();

        var context = BuildAuthenticatedContext(
            externalIdClaimValue: Guid.NewGuid().ToString(),
            roleClaimValue: null);

        await middleware.InvokeAsync(context, NullLogger<KonnectAuthenticationMiddleware>.Instance);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task Should_Return401_When_RoleClaimValueIsUnknown()
    {
        var middleware = BuildMiddleware();

        var context = BuildAuthenticatedContext(
            externalIdClaimValue: Guid.NewGuid().ToString(),
            roleClaimValue: "Administrator");

        await middleware.InvokeAsync(context, NullLogger<KonnectAuthenticationMiddleware>.Instance);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Theory]
    [InlineData(JwtRoles.JobSeeker)]
    [InlineData(JwtRoles.Recruiter)]
    public async Task Should_PassThrough_When_BothClaimsArePresentAndValid(string roleClaimValue)
    {
        var nextCalled = false;
        var middleware = new KonnectAuthenticationMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = BuildAuthenticatedContext(
            externalIdClaimValue: Guid.NewGuid().ToString(),
            roleClaimValue: roleClaimValue);

        await middleware.InvokeAsync(context, NullLogger<KonnectAuthenticationMiddleware>.Instance);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static KonnectAuthenticationMiddleware BuildMiddleware() =>
        new(_ => throw new InvalidOperationException(
            "next() should not be invoked when middleware short-circuits with 401."));

    private static DefaultHttpContext BuildAuthenticatedContext(
        string? externalIdClaimValue,
        string? roleClaimValue)
    {
        var claims = new List<Claim>();
        if (externalIdClaimValue is not null)
        {
            claims.Add(new Claim(KonnectClaimTypes.ExternalId, externalIdClaimValue));
        }

        if (roleClaimValue is not null)
        {
            claims.Add(new Claim(KonnectClaimTypes.Role, roleClaimValue));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        var principal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext { User = principal };
    }
}
