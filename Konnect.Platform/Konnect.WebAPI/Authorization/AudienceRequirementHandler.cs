using Konnect.Infrastructure.Contracts.Enums;
using Konnect.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Konnect.WebAPI.Authorization;

/// <summary>
/// Evaluates <see cref="AudienceRequirement"/> against the current principal.
/// The token must carry the <c>aud</c> claim matching the configured audience
/// URL for the requirement's side AND the role expected for that side; both
/// checks are independent so a token re-issued under the wrong audience (or
/// with the wrong role claim) still fails. Audience URLs are read from
/// <see cref="Auth0Settings"/> at evaluation time via
/// <see cref="IOptions{TOptions}"/> so the integration-test fixture's
/// in-memory override is picked up after <c>ConfigureTestServices</c> runs.
/// </summary>
public sealed class AudienceRequirementHandler(IOptions<Auth0Settings> auth0Options)
    : AuthorizationHandler<AudienceRequirement>
{
    private readonly IOptions<Auth0Settings> _auth0Options = auth0Options;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AudienceRequirement requirement)
    {
        var settings = _auth0Options.Value;

        var (expectedAudience, expectedRole) = requirement.Audience switch
        {
            AudienceType.JobSeeker => (settings.SeekerAudience, JwtRoles.JobSeeker),
            AudienceType.Recruiter => (settings.RecruiterAudience, JwtRoles.Recruiter),
            _ => (string.Empty, string.Empty),
        };

        if (string.IsNullOrEmpty(expectedAudience) || string.IsNullOrEmpty(expectedRole))
        {
            return Task.CompletedTask;
        }

        if (!context.User.HasClaim("aud", expectedAudience))
        {
            return Task.CompletedTask;
        }

        if (!context.User.IsInRole(expectedRole))
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
