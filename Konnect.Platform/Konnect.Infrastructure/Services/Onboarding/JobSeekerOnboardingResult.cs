using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Services.Onboarding;

/// <summary>
/// Tagged-union outcome of a job-seeker onboarding attempt. The controller
/// pattern-matches on the concrete case to pick the HTTP status code:
/// <list type="bullet">
///   <item><see cref="Created"/> — first onboarding, 201</item>
///   <item><see cref="Existing"/> — idempotent replay, 200</item>
/// </list>
/// </summary>
public abstract record JobSeekerOnboardingResult
{
    private JobSeekerOnboardingResult()
    {
    }

    public sealed record Created(JobSeekerUser JobSeeker) : JobSeekerOnboardingResult;

    public sealed record Existing(JobSeekerUser JobSeeker) : JobSeekerOnboardingResult;
}
