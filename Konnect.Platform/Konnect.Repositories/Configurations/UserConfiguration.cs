using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Konnect.Repositories.Configurations;

/// <summary>
/// TPH configuration for the User hierarchy: one underlying
/// <c>asp_net_users</c> table with an <c>audience</c> discriminator column.
/// EF Core picks the right concrete type (<see cref="JobSeekerUser"/> or
/// <see cref="RecruiterUser"/>) at materialization time based on the column.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // IdentityDbContext hard-codes "AspNetUsers" for the base table, which
        // breaks TPH because the subclass tables get the snake_case convention
        // applied. Force the table name here — both to fix TPH and to drop
        // the framework's "asp_net_" prefix in favour of the cleaner "users".
        builder.ToTable("users");

        builder.HasDiscriminator<string>("audience")
            .HasValue<JobSeekerUser>("JobSeeker")
            .HasValue<RecruiterUser>("Recruiter");

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .IsRequired();
    }
}
