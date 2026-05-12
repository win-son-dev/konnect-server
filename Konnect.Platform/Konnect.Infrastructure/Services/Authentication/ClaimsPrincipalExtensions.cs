using System.Security.Claims;

namespace Konnect.Infrastructure.Services.Authentication;

/// <summary>
/// Read accessors for Konnect's namespaced JWT claims. Lives next to
/// <see cref="KonnectClaimTypes"/> and <see cref="JwtRoles"/> so the
/// Authentication folder is the single source of truth for any code that
/// reads the access token. Authenticated requests always pass through
/// <c>KonnectAuthenticationMiddleware</c> first, which validates the claim
/// shape and 401s on anything malformed — so callers here trust that an
/// authenticated principal carries a parseable claim and these methods
/// never throw on missing/bad input. Anything unexpected falls through as
/// <see cref="Guid.Empty"/>, surfacing as a normal "not found" downstream
/// rather than a hot exception an attacker could try to spam.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid GetExternalId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirst(KonnectClaimTypes.ExternalId)?.Value;
        return Guid.TryParse(raw, out var externalId) ? externalId : Guid.Empty;
    }
}
