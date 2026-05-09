using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Services.Onboarding;

/// <summary>
/// Tagged-union outcome of a recruiter onboarding attempt. The controller
/// pattern-matches on the concrete case to pick the HTTP status code:
/// <list type="bullet">
///   <item><see cref="Created"/> — first onboarding, 201</item>
///   <item><see cref="Existing"/> — idempotent replay, 200</item>
///   <item><see cref="SlugConflict"/> — slug already owned by a different recruiter, 409</item>
/// </list>
/// </summary>
public abstract record RecruiterOnboardingResult
{
    private RecruiterOnboardingResult()
    {
    }

    public sealed record Created(Guid RecruiterId, Company Company) : RecruiterOnboardingResult;

    public sealed record Existing(Guid RecruiterId, Company Company) : RecruiterOnboardingResult;

    public sealed record SlugConflict(string Slug) : RecruiterOnboardingResult;
}
