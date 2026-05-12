using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Repositories;

/// <summary>
/// Data-plane access for <see cref="Company"/>. Phase 1 covers the slug-keyed
/// public lookup, idempotency-side existence checks, and the atomic
/// company-plus-first-recruiter insert that recruiter onboarding relies on.
/// The atomic insert lives here (rather than spanning two repositories) so
/// the transaction boundary stays inside a single leaf — services orchestrate
/// validation and call this one method instead of juggling a unit of work.
/// </summary>
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new <paramref name="company"/> together with its first
    /// <paramref name="firstRecruiter"/> in a single Postgres transaction.
    /// Either both rows commit or neither does — there is no partial state
    /// where a recruiter exists without their company or vice versa.
    /// </summary>
    Task AddWithFirstRecruiterAsync(
        Company company,
        RecruiterUser firstRecruiter,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists modifications to an existing <paramref name="company"/>.
    /// EF's change tracker drives the SQL: a tracked entity emits an UPDATE
    /// only for the dirty columns; a detached entity is treated as a full
    /// replace. <c>updated_at</c> is refreshed by the Postgres
    /// <c>set_updated_at</c> trigger — the application never assigns it.
    /// </summary>
    Task UpdateAsync(Company company, CancellationToken cancellationToken);
}
