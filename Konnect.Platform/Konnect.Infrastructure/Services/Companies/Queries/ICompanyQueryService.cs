using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Services.Companies.Queries;

/// <summary>
/// Read-side service for the <see cref="Company"/> aggregate. Mirrors the
/// GraphQL query surface: public slug-based lookup + recruiter-scoped
/// "the company that this recruiter belongs to" lookup. Pairs with
/// <see cref="Commands.ICompanyCommandService"/> for the write side.
/// Lookup methods follow the repo-style <c>GetByXxxAsync</c> convention —
/// the subject (<see cref="Company"/>) is implicit from the interface name.
/// </summary>
public interface ICompanyQueryService
{
    /// <summary>
    /// Public lookup by URL-safe slug. Returns <c>null</c> if no company
    /// matches — the GraphQL resolver bubbles that up as a nullable field.
    /// </summary>
    Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the company the recruiter (identified by their Auth0
    /// <paramref name="recruiterExternalId"/>) belongs to. Throws
    /// <see cref="InvalidOperationException"/> if the recruiter has no
    /// domain row (the JWT was valid but #51 onboarding never ran for this
    /// user — surface loudly rather than silently 404-ing).
    /// </summary>
    Task<Company> GetByRecruiterIdAsync(
        Guid recruiterExternalId,
        CancellationToken cancellationToken);
}
