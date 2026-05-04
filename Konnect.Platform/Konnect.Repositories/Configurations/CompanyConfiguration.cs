using Konnect.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Konnect.Repositories.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(company => company.Id);

        builder.Property(company => company.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(company => company.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(company => company.Slug)
            .IsUnique();

        builder.Property(company => company.Description)
            .HasMaxLength(4000);

        builder.Property(company => company.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(company => company.Verified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(company => company.CreatedAt)
            .IsRequired();

        builder.Property(company => company.UpdatedAt)
            .IsRequired();
    }
}
