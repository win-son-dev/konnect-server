using Konnect.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Konnect.WebAPI.Controllers;

/// <summary>
/// Returns the authenticated caller's identity, distilled from the JWT
/// claims into a cheap who-am-I shape. Useful as a smoke test for the auth
/// pipeline (curl against a SPA-issued token), and as a "did the SPA's
/// stored access token still work" probe before rendering an authenticated
/// view. No DB lookup — the claims are stamped by Auth0 and never leave the
/// JWT, so this endpoint is O(1).
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var externalId = User.FindFirst(KonnectClaimTypes.ExternalId)?.Value;
        var role = User.FindFirst(KonnectClaimTypes.Role)?.Value;
        var email = User.FindFirst("email")?.Value;

        return Ok(new MeResponse(externalId, role, email));
    }
}

public sealed record MeResponse(string? ExternalId, string? Role, string? Email);
