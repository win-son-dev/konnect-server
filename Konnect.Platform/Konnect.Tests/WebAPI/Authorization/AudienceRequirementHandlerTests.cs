using System.Security.Claims;
using Konnect.Infrastructure.Contracts.Enums;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.WebAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Konnect.Tests.WebAPI.Authorization;

/// <summary>
/// Unit tests for <see cref="AudienceRequirementHandler"/>. The full pipeline
/// (JwtBearer → policy → endpoint) is exercised in
/// <see cref="AuthorizationPolicySweepTests"/>; this class isolates the
/// branch-by-branch logic of the requirement handler so a regression in
/// either of the two independent checks (audience claim, role) is caught
/// without spinning up the host.
/// </summary>
public sealed class AudienceRequirementHandlerTests
{
    private const string SeekerAudience = "https://api.konnect.test/seeker";
    private const string RecruiterAudience = "https://api.konnect.test/recruiter";

    [Fact]
    public async Task Should_Succeed_When_SeekerAudienceAndSeekerRoleMatch()
    {
        var context = BuildContext(
            audience: AudienceType.JobSeeker,
            audienceClaim: SeekerAudience,
            roleClaim: JwtRoles.JobSeeker);

        await BuildHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Should_Succeed_When_RecruiterAudienceAndRecruiterRoleMatch()
    {
        var context = BuildContext(
            audience: AudienceType.Recruiter,
            audienceClaim: RecruiterAudience,
            roleClaim: JwtRoles.Recruiter);

        await BuildHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Should_Fail_When_AudienceClaimMismatchesRequirement()
    {
        // Token has the seeker audience but the endpoint requires the recruiter
        // audience — even if the role claim says "Recruiter", the token was
        // never issued for the recruiter API and must be rejected.
        var context = BuildContext(
            audience: AudienceType.Recruiter,
            audienceClaim: SeekerAudience,
            roleClaim: JwtRoles.Recruiter);

        await BuildHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Should_Fail_When_RoleClaimMismatchesRequirement()
    {
        // Audience says recruiter side, but the role claim is JobSeeker — the
        // independent role check is what catches it.
        var context = BuildContext(
            audience: AudienceType.Recruiter,
            audienceClaim: RecruiterAudience,
            roleClaim: JwtRoles.JobSeeker);

        await BuildHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Should_Fail_When_AudienceClaimIsMissing()
    {
        var context = BuildContext(
            audience: AudienceType.Recruiter,
            audienceClaim: null,
            roleClaim: JwtRoles.Recruiter);

        await BuildHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Should_Fail_When_RoleClaimIsMissing()
    {
        var context = BuildContext(
            audience: AudienceType.Recruiter,
            audienceClaim: RecruiterAudience,
            roleClaim: null);

        await BuildHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Should_Fail_When_ConfiguredAudienceUrlIsEmpty()
    {
        // Defensive: if Auth0Settings was misconfigured at startup so the URL
        // is empty, the handler must not succeed on a token whose aud claim is
        // also empty. Without this guard, an attacker who minted a token with
        // aud="" would silently pass.
        var settings = new Auth0Settings
        {
            Domain = "konnect-test.auth0.local",
            SeekerAudience = string.Empty,
            RecruiterAudience = string.Empty,
        };
        var handler = new AudienceRequirementHandler(Options.Create(settings));

        var context = BuildContext(
            audience: AudienceType.Recruiter,
            audienceClaim: string.Empty,
            roleClaim: JwtRoles.Recruiter);

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static AudienceRequirementHandler BuildHandler()
    {
        var settings = new Auth0Settings
        {
            Domain = "konnect-test.auth0.local",
            SeekerAudience = SeekerAudience,
            RecruiterAudience = RecruiterAudience,
        };
        return new AudienceRequirementHandler(Options.Create(settings));
    }

    private static AuthorizationHandlerContext BuildContext(
        AudienceType audience,
        string? audienceClaim,
        string? roleClaim)
    {
        var claims = new List<Claim>();
        if (audienceClaim is not null)
        {
            claims.Add(new Claim("aud", audienceClaim));
        }

        if (roleClaim is not null)
        {
            claims.Add(new Claim(KonnectClaimTypes.Role, roleClaim));
        }

        // RoleClaimType has to match what JwtBearer wires up in Program.cs —
        // otherwise ClaimsPrincipal.IsInRole(...) looks at the wrong claim and
        // the role check always returns false.
        var identity = new ClaimsIdentity(
            claims,
            authenticationType: "Test",
            nameType: KonnectClaimTypes.ExternalId,
            roleType: KonnectClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);

        var requirement = new AudienceRequirement(audience);
        return new AuthorizationHandlerContext(
            [requirement],
            principal,
            resource: null);
    }
}
