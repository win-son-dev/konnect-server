using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Repositories;

public sealed class CompanyRepository(KonnectDbContext dbContext) : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Companies
            .FirstOrDefaultAsync(company => company.Id == id, cancellationToken);

    public Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => dbContext.Companies
            .FirstOrDefaultAsync(company => company.Slug == slug, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
        => dbContext.Companies
            .AnyAsync(company => company.Slug == slug, cancellationToken);

    /// <summary>
    /// The transaction is owned here, not by the calling service, so that
    /// the repository remains the single boundary touching the data store —
    /// matches the rule that services orchestrate while repositories are
    /// leaves. SaveChanges runs once with both entities staged; if it fails
    /// (slug unique-index violation, FK violation) the transaction rolls
    /// back and neither row is persisted.
    /// </summary>
    public async Task AddWithFirstRecruiterAsync(
        Company company,
        RecruiterUser firstRecruiter,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        dbContext.Companies.Add(company);
        dbContext.Recruiters.Add(firstRecruiter);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
