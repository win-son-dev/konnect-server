using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Konnect.Repositories.Configurations;

/// <summary>
/// TPH configuration for the User hierarchy: one <c>users</c> table with an
/// <c>audience</c> discriminator column. EF Core picks the right concrete
/// type (<see cref="JobSeekerUser"/> or <see cref="RecruiterUser"/>) at
/// materialization time based on the discriminator. The primary key
/// <see cref="User.Id"/> is the Auth0-generated <c>external_id</c> Guid —
/// no separate IdP-identifier column is needed.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.HasDiscriminator<string>("audience")
            .HasValue<JobSeekerUser>("JobSeeker")
            .HasValue<RecruiterUser>("Recruiter");

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(user => user.Email);

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .IsRequired();
    }
}
