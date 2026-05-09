using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Onboarding;

namespace Konnect.Services.Onboarding;

public sealed class JobSeekerOnboardingService(IUserRepository userRepository) : IJobSeekerOnboardingService
{
    public async Task<JobSeekerOnboardingResult> OnboardAsync(
        Guid externalId,
        string email,
        OnboardJobSeekerInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var existing = await userRepository.GetByIdAsync(externalId, cancellationToken);
        if (existing is JobSeekerUser existingSeeker)
        {
            return new JobSeekerOnboardingResult.Existing(existingSeeker);
        }

        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"User {externalId} is already provisioned as {existing.GetType().Name}, cannot onboard as JobSeeker.");
        }

        var seeker = new JobSeekerUser
        {
            Id = externalId,
            Email = email,
            Headline = input.Headline ?? string.Empty,
            Location = input.Location ?? string.Empty,
            OpenToWork = input.OpenToWork,
        };

        await userRepository.AddJobSeekerAsync(seeker, cancellationToken);

        return new JobSeekerOnboardingResult.Created(seeker);
    }
}
