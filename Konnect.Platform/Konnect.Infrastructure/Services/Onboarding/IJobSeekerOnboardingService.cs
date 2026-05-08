namespace Konnect.Infrastructure.Services.Onboarding;

/// <summary>
/// Provisions the <c>JobSeekerUser</c> row keyed off the Auth0
/// <c>external_id</c> claim of the authenticated caller. Idempotent on
/// <paramref name="externalId"/> — re-invocations return the existing seeker
/// without mutating state.
/// </summary>
public interface IJobSeekerOnboardingService
{
    Task<JobSeekerOnboardingResult> OnboardAsync(
        Guid externalId,
        string email,
        OnboardJobSeekerInput input,
        CancellationToken cancellationToken);
}
