using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Repositories;

/// <summary>
/// Data-plane access for <see cref="JobPosting"/>. Phase 1 ships the four
/// CRUD primitives so the Repository pattern is established end-to-end. The
/// query / search surface (list-by-company, filtering, pagination) expands
/// when sub-issue #25 (JobPosting GraphQL type) lands.
/// </summary>
public interface IJobRepository
{
    Task<JobPosting?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(JobPosting jobPosting, CancellationToken cancellationToken);

    Task UpdateAsync(JobPosting jobPosting, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
