using System.Globalization;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Infrastructure.Services.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Konnect.WebAPI.Controllers.Onboarding;

/// <summary>
/// Post-Auth0 handshake for recruiters. The Auth0 SPA hits this endpoint
/// once after sign-up to provision the <c>RecruiterUser</c> + owning
/// <c>Company</c> rows; subsequent calls (e.g. SPA replays after a network
/// hiccup) are idempotent and return the existing pair without writes.
/// Identity always comes from the JWT — never from the request body — so a
/// recruiter cannot onboard another user.
/// </summary>
[ApiController]
[Route("api/recruiter/onboard")]
[Authorize(Roles = JwtRoles.Recruiter)]
public sealed class RecruiterOnboardingController(IRecruiterOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Onboard(
        [FromBody] OnboardRecruiterInput input,
        CancellationToken cancellationToken)
    {
        var externalId = ReadExternalId();
        var email = ReadEmail();

        var result = await onboardingService.OnboardAsync(externalId, email, input, cancellationToken);

        return result switch
        {
            RecruiterOnboardingResult.Created created => StatusCode(
                StatusCodes.Status201Created,
                ToResponse(created.RecruiterId, created.Company)),
            RecruiterOnboardingResult.Existing existing => Ok(
                ToResponse(existing.RecruiterId, existing.Company)),
            RecruiterOnboardingResult.SlugConflict conflict => Conflict(new
            {
                error = "slug_conflict",
                slug = conflict.Slug,
                message = $"Company slug '{conflict.Slug}' is already taken.",
            }),
            _ => throw new InvalidOperationException($"Unhandled onboarding result: {result.GetType().Name}"),
        };
    }

    private Guid ReadExternalId()
    {
        var raw = User.FindFirst(KonnectClaimTypes.ExternalId)?.Value;
        return Guid.TryParse(raw, out var externalId)
            ? externalId
            : throw new InvalidOperationException(
                $"Authenticated request reached recruiter onboarding without a valid {KonnectClaimTypes.ExternalId} claim.");
    }

    private string ReadEmail()
        => User.FindFirst("email")?.Value
            ?? throw new InvalidOperationException(
                "Authenticated request reached recruiter onboarding without an email claim.");

    private static RecruiterOnboardingResponse ToResponse(Guid recruiterId, Company company)
        => new(
            recruiterId,
            new OnboardingCompanyResponse(
                company.Id,
                company.Name,
                company.Slug,
                company.Description,
                company.WebsiteUrl,
                company.Verified,
                company.CreatedAt.ToString("O", CultureInfo.InvariantCulture)));
}

public sealed record RecruiterOnboardingResponse(Guid RecruiterId, OnboardingCompanyResponse Company);

public sealed record OnboardingCompanyResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    bool Verified,
    string CreatedAt);
