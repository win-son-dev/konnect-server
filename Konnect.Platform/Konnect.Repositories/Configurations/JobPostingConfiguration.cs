using Konnect.Infrastructure.Contracts.Enums;
using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Konnect.Repositories.Configurations;

public sealed class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.HasKey(posting => posting.Id);

        builder.Property(posting => posting.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(posting => posting.Description)
            .IsRequired();

        builder.Property(posting => posting.Location)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(posting => posting.EmploymentType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(posting => posting.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(posting => posting.SalaryMin)
            .HasPrecision(18, 2);

        builder.Property(posting => posting.SalaryMax)
            .HasPrecision(18, 2);

        builder.Property(posting => posting.PostedAt)
            .IsRequired();

        builder.Property(posting => posting.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(posting => posting.Company)
            .WithMany(company => company.JobPostings)
            .HasForeignKey(posting => posting.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(posting => new { posting.CompanyId, posting.IsActive });
    }
}
