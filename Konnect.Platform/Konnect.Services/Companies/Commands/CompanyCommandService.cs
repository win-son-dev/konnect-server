using Konnect.Infrastructure.Contracts.Companies;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Companies.Commands;

namespace Konnect.Services.Companies.Commands;

public sealed class CompanyCommandService(
    IUserRepository userRepository,
    ICompanyRepository companyRepository) : ICompanyCommandService
{
    public async Task<Company> UpdateByRecruiterIdAsync(
        Guid recruiterExternalId,
        UpdateCompanyInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Name);

        // Belt-and-braces: the company being updated is derived from the
        // recruiter's domain row, not from any client input. A recruiter
        // simply has no path to update a different company's profile.
        var recruiter = await userRepository.GetByIdAsync(recruiterExternalId, cancellationToken)
            as RecruiterUser
            ?? throw new InvalidOperationException(
                $"No RecruiterUser exists for external_id {recruiterExternalId}. " +
                "The SPA must call POST /api/recruiter/onboard before any recruiter-scoped mutation.");

        var company = await companyRepository.GetByIdAsync(recruiter.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Recruiter {recruiterExternalId} references missing company {recruiter.CompanyId}.");

        company.Name = input.Name;
        company.Description = input.Description;
        company.WebsiteUrl = input.WebsiteUrl;

        // updated_at is owned by the Postgres set_updated_at trigger.
        await companyRepository.UpdateAsync(company, cancellationToken);

        return company;
    }
}
