using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Repositories;

public sealed class JobRepository : IJobRepository
{
    private readonly KonnectDbContext dbContext;

    public JobRepository(KonnectDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<JobPosting?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.JobPostings
            .FirstOrDefaultAsync(jobPosting => jobPosting.Id == id, cancellationToken);

    public async Task AddAsync(JobPosting jobPosting, CancellationToken cancellationToken)
    {
        dbContext.JobPostings.Add(jobPosting);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Persists changes on a <see cref="JobPosting"/>. EF's change tracker
    /// drives the SQL: a tracked entity that the caller modified emits an
    /// UPDATE only for the dirty columns (PATCH-style); a detached entity
    /// passed in fresh is treated as a full replace and emits UPDATE for
    /// every column.
    /// </summary>
    public async Task UpdateAsync(JobPosting jobPosting, CancellationToken cancellationToken)
    {
        var entry = dbContext.Entry(jobPosting);
        if (entry.State == EntityState.Detached)
        {
            dbContext.JobPostings.Update(jobPosting);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var jobPosting = await dbContext.JobPostings
            .FirstOrDefaultAsync(posting => posting.Id == id, cancellationToken);

        if (jobPosting is null)
        {
            return false;
        }

        dbContext.JobPostings.Remove(jobPosting);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
