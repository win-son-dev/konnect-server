using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Konnect.Repositories.Configurations;

public sealed class JobSeekerUserConfiguration : IEntityTypeConfiguration<JobSeekerUser>
{
    public void Configure(EntityTypeBuilder<JobSeekerUser> builder)
    {
        builder.Property(seeker => seeker.Headline)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(seeker => seeker.Location)
            .HasMaxLength(120)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(seeker => seeker.OpenToWork)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
