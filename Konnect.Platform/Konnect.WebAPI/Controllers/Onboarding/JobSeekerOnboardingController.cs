using System.Globalization;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Infrastructure.Services.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Konnect.WebAPI.Controllers.Onboarding;

/// <summary>
/// Post-Auth0 handshake for job seekers. The Auth0 SPA hits this endpoint
/// once after sign-up to provision the <c>JobSeekerUser</c> row; subsequent
/// calls are idempotent and return the existing seeker without writes.
/// Identity comes from the JWT, never from the request body.
/// </summary>
[ApiController]
[Route("api/seeker/onboard")]
[Authorize(Roles = JwtRoles.JobSeeker)]
public sealed class JobSeekerOnboardingController(IJobSeekerOnboardingService onboardingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Onboard(
        [FromBody] OnboardJobSeekerInput input,
        CancellationToken cancellationToken)
    {
        var externalId = ReadExternalId();
        var email = ReadEmail();

        var result = await onboardingService.OnboardAsync(externalId, email, input, cancellationToken);

        return result switch
        {
            JobSeekerOnboardingResult.Created created => StatusCode(
                StatusCodes.Status201Created,
                ToResponse(created.JobSeeker)),
            JobSeekerOnboardingResult.Existing existing => Ok(
                ToResponse(existing.JobSeeker)),
            _ => throw new InvalidOperationException($"Unhandled onboarding result: {result.GetType().Name}"),
        };
    }

    private Guid ReadExternalId()
    {
        var raw = User.FindFirst(KonnectClaimTypes.ExternalId)?.Value;
        return Guid.TryParse(raw, out var externalId)
            ? externalId
            : throw new InvalidOperationException(
                $"Authenticated request reached seeker onboarding without a valid {KonnectClaimTypes.ExternalId} claim.");
    }

    private string ReadEmail()
        => User.FindFirst("email")?.Value
            ?? throw new InvalidOperationException(
                "Authenticated request reached seeker onboarding without an email claim.");

    private static JobSeekerOnboardingResponse ToResponse(JobSeekerUser seeker)
        => new(
            seeker.Id,
            seeker.Email,
            seeker.Headline,
            seeker.Location,
            seeker.OpenToWork,
            seeker.CreatedAt.ToString("O", CultureInfo.InvariantCulture));
}

public sealed record JobSeekerOnboardingResponse(
    Guid JobSeekerId,
    string Email,
    string Headline,
    string Location,
    bool OpenToWork,
    string CreatedAt);
