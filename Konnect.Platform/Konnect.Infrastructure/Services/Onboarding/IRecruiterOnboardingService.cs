namespace Konnect.Infrastructure.Services.Onboarding;

/// <summary>
/// Provisions the <c>RecruiterUser</c> + owning <c>Company</c> rows that
/// Phase-1 recruiter-scoped features depend on, keyed off the Auth0
/// <c>external_id</c> claim of the authenticated caller. Idempotent on
/// <paramref name="externalId"/> — re-invocations return the existing pair
/// without mutating state.
/// </summary>
public interface IRecruiterOnboardingService
{
    Task<RecruiterOnboardingResult> OnboardAsync(
        Guid externalId,
        string email,
        OnboardRecruiterInput input,
        CancellationToken cancellationToken);
}
