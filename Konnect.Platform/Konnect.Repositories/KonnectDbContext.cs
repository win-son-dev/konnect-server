using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Repositories;

/// <summary>
/// Konnect's EF Core session. Plain <see cref="DbContext"/> — Konnect does
/// not store passwords or Identity tokens, so there is no
/// <c>IdentityDbContext</c> base and no <c>asp_net_*</c> tables. Authentication
/// is owned by Auth0; the <c>users</c> table holds profile rows keyed by the
/// Auth0-generated <c>external_id</c> Guid (see <see cref="User.Id"/>).
/// </summary>
public class KonnectDbContext(DbContextOptions<KonnectDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<JobSeekerUser> JobSeekers => Set<JobSeekerUser>();

    public DbSet<RecruiterUser> Recruiters => Set<RecruiterUser>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<JobPosting> JobPostings => Set<JobPosting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KonnectDbContext).Assembly);
    }
}
