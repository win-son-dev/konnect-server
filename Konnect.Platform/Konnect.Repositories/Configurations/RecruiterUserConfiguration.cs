using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Konnect.Repositories.Configurations;

public sealed class RecruiterUserConfiguration : IEntityTypeConfiguration<RecruiterUser>
{
    public void Configure(EntityTypeBuilder<RecruiterUser> builder)
    {
        builder.Property(recruiter => recruiter.FirstName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(recruiter => recruiter.LastName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(recruiter => recruiter.JobTitle)
            .HasMaxLength(120)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.HasOne(recruiter => recruiter.Company)
            .WithMany(company => company.Recruiters)
            .HasForeignKey(recruiter => recruiter.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(recruiter => recruiter.CompanyId);
    }
}
