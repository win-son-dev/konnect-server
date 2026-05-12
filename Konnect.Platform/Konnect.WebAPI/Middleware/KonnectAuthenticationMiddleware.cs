using Konnect.Infrastructure.Services.Authentication;

namespace Konnect.WebAPI.Middleware;

/// <summary>
/// Runs immediately after the framework's JwtBearer middleware. By the time
/// this middleware sees a request, the bearer scheme has already validated
/// the token's signature, issuer, audience, and expiry — what we still need
/// to confirm is that the Auth0-stamped Konnect-specific claims are present
/// and well-formed:
/// <list type="bullet">
///   <item><c>https://konnect.dev/external_id</c> — must parse as a Guid</item>
///   <item><c>https://konnect.dev/role</c> — must be a known role name</item>
/// </list>
/// Either claim missing or malformed means a token that's signed by Auth0
/// but not provisioned by Konnect's Pre-User-Registration / Post-Login
/// Actions. We reject with 401 and log at <c>Warning</c> — the token
/// passed JwtBearer validation so an attacker brute-forcing this path needs
/// a real Auth0-signed token, but we still treat the rejection as
/// operational noise rather than a system error.
/// </summary>
public sealed class KonnectAuthenticationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<KonnectAuthenticationMiddleware> logger)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            // Anonymous request — let the pipeline continue. The framework's
            // [Authorize] policy will reject it later if the endpoint requires
            // authentication; if not, the request is genuinely public.
            await next(context);
            return;
        }

        var externalIdClaim = context.User.FindFirst(KonnectClaimTypes.ExternalId)?.Value;
        if (!Guid.TryParse(externalIdClaim, out _))
        {
            logger.LogWarning(
                "Authenticated request missing or unparseable {ClaimName} claim; rejecting.",
                KonnectClaimTypes.ExternalId);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var role = context.User.FindFirst(KonnectClaimTypes.Role)?.Value;
        if (role is not (JwtRoles.JobSeeker or JwtRoles.Recruiter))
        {
            logger.LogWarning(
                "Authenticated request has missing or unknown {RoleClaimName} claim ({Role}); rejecting.",
                KonnectClaimTypes.Role,
                role ?? "<none>");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
