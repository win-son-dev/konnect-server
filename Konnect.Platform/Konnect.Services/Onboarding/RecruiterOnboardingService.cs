using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Onboarding;

namespace Konnect.Services.Onboarding;

public sealed class RecruiterOnboardingService : IRecruiterOnboardingService
{
    private readonly IUserRepository userRepository;
    private readonly ICompanyRepository companyRepository;

    public RecruiterOnboardingService(
        IUserRepository userRepository,
        ICompanyRepository companyRepository)
    {
        this.userRepository = userRepository;
        this.companyRepository = companyRepository;
    }

    public async Task<RecruiterOnboardingResult> OnboardAsync(
        Guid externalId,
        string email,
        OnboardRecruiterInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ValidateInput(input);

        var existing = await userRepository.GetByIdAsync(externalId, cancellationToken);
        if (existing is RecruiterUser existingRecruiter)
        {
            // Idempotent replay — already onboarded. Return the existing pair
            // without touching the DB; the SPA's retry-after-network-hiccup
            // path lands here.
            var existingCompany = await companyRepository.GetByIdAsync(
                existingRecruiter.CompanyId,
                cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Recruiter {existingRecruiter.Id} references missing company {existingRecruiter.CompanyId}.");

            return new RecruiterOnboardingResult.Existing(existingRecruiter.Id, existingCompany);
        }

        if (existing is not null)
        {
            // The external_id is already provisioned for a different audience
            // (a JobSeekerUser). Auth0's pre-registration action enforces a
            // single audience per identity, so reaching here means a bug
            // upstream — surface loudly rather than silently shadowing.
            throw new InvalidOperationException(
                $"User {externalId} is already provisioned as {existing.GetType().Name}, cannot onboard as Recruiter.");
        }

        if (await companyRepository.SlugExistsAsync(input.CompanySlug, cancellationToken))
        {
            return new RecruiterOnboardingResult.SlugConflict(input.CompanySlug);
        }

        var companyId = Guid.NewGuid();

        // CreatedAt / UpdatedAt are owned by Postgres (DEFAULT CURRENT_TIMESTAMP
        // + set_updated_at trigger). Leaving them at the .NET default tells
        // EF to skip them on INSERT and read back what the DB stamped.
        var company = new Company
        {
            Id = companyId,
            Name = input.CompanyName,
            Slug = input.CompanySlug,
            Description = input.CompanyDescription,
            WebsiteUrl = input.CompanyWebsiteUrl,
            Verified = false,
        };

        var recruiter = new RecruiterUser
        {
            Id = externalId,
            Email = email,
            CompanyId = companyId,
            FirstName = input.FirstName,
            LastName = input.LastName,
            JobTitle = input.JobTitle,
        };

        await companyRepository.AddWithFirstRecruiterAsync(company, recruiter, cancellationToken);

        return new RecruiterOnboardingResult.Created(recruiter.Id, company);
    }

    private static void ValidateInput(OnboardRecruiterInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.CompanyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.CompanySlug);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.FirstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.LastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.JobTitle);
    }
}
