using Konnect.Infrastructure.Contracts.Companies;
using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Services.Companies.Commands;

/// <summary>
/// Write-side service for the <see cref="Company"/> aggregate. Phase 1
/// exposes a single "edit your own company profile" command; later phases
/// will add company verification, slug rename, archival, etc. Pairs with
/// <see cref="Queries.ICompanyQueryService"/> for the read side.
/// </summary>
public interface ICompanyCommandService
{
    /// <summary>
    /// Updates the company the recruiter (identified by their Auth0
    /// <paramref name="recruiterExternalId"/>) belongs to. The recruiter
    /// can only edit *their own* company — the service derives the target
    /// company from the recruiter's <c>CompanyId</c> rather than trusting
    /// any client-supplied identifier. Returns the updated <see cref="Company"/>.
    /// </summary>
    Task<Company> UpdateByRecruiterIdAsync(
        Guid recruiterExternalId,
        UpdateCompanyInput input,
        CancellationToken cancellationToken);
}
