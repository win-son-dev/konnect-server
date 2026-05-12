using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Companies.Queries;

namespace Konnect.Services.Companies.Queries;

public sealed class CompanyQueryService(
    IUserRepository userRepository,
    ICompanyRepository companyRepository) : ICompanyQueryService
{
    public Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        return companyRepository.GetBySlugAsync(slug, cancellationToken);
    }

    public async Task<Company> GetByRecruiterIdAsync(
        Guid recruiterExternalId,
        CancellationToken cancellationToken)
    {
        var recruiter = await userRepository.GetByIdAsync(recruiterExternalId, cancellationToken)
            as RecruiterUser
            ?? throw new InvalidOperationException(
                $"No RecruiterUser exists for external_id {recruiterExternalId}. " +
                "The SPA must call POST /api/recruiter/onboard before any recruiter-scoped query.");

        return await companyRepository.GetByIdAsync(recruiter.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Recruiter {recruiterExternalId} references missing company {recruiter.CompanyId}.");
    }
}
