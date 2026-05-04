using Konnect.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Repositories;

/// <summary>
/// Konnect's EF Core session. Inherits from <see cref="IdentityDbContext{TUser, TRole, TKey}"/>
/// so the ASP.NET Identity tables (<c>asp_net_users</c>, <c>asp_net_roles</c>, etc.)
/// land in the same database as the domain tables. The User hierarchy is TPH —
/// see <c>UserConfiguration</c> for the discriminator setup.
/// </summary>
public class KonnectDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public KonnectDbContext(DbContextOptions<KonnectDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<JobPosting> JobPostings => Set<JobPosting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(KonnectDbContext).Assembly);

        // IdentityDbContext.OnModelCreating hard-codes PascalCase table names
        // (AspNetRoles, AspNetUserRoles, ...) which the snake_case convention
        // can't override. Force the names here AND drop the framework's
        // "asp_net_" prefix — Konnect has only one user concept, so the
        // shorter names read cleaner in psql and don't leak the framework.
        builder.Entity<IdentityRole<Guid>>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
    }
}
