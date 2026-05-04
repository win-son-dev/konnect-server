using Konnect.Infrastructure.Contracts.Enums;
using Konnect.Infrastructure.Entities;
using Konnect.Repositories;
using Konnect.Tests.Infrastructure;

namespace Konnect.Tests.Repositories;

/// <summary>
/// Round-trips a <see cref="JobPosting"/> through every <see cref="JobRepository"/>
/// CRUD method against a real Postgres container. Pins the JobRepository
/// integration contract for sub-issue #25 to build on.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class JobRepositoryTests
{
    private readonly PostgresFixture postgresFixture;

    public JobRepositoryTests(PostgresFixture postgresFixture)
    {
        this.postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task Should_RoundtripJobPosting_Through_AddGetUpdateDelete()
    {
        await using var dbContext = postgresFixture.CreateDbContext();
        var company = await CreateAndPersistCompanyAsync(dbContext);

        var jobRepository = new JobRepository(dbContext);

        // Add
        var newPosting = new JobPosting
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Title = "Senior .NET Engineer",
            Description = "Build the Konnect platform.",
            Location = "Sydney, Australia",
            EmploymentType = EmploymentType.FullTime,
            Currency = "AUD",
            SalaryMin = 150_000m,
            SalaryMax = 180_000m,
            PostedAt = DateTimeOffset.UtcNow,
            IsActive = true,
        };
        await jobRepository.AddAsync(newPosting, CancellationToken.None);

        // Get
        var fetchedPosting = await jobRepository.GetByIdAsync(newPosting.Id, CancellationToken.None);
        Assert.NotNull(fetchedPosting);
        Assert.Equal(newPosting.Title, fetchedPosting.Title);
        Assert.Equal(EmploymentType.FullTime, fetchedPosting.EmploymentType);
        Assert.Equal("AUD", fetchedPosting.Currency);

        // Update — modify on the tracked entity, expect partial UPDATE
        fetchedPosting.Title = "Staff .NET Engineer";
        fetchedPosting.SalaryMax = 200_000m;
        await jobRepository.UpdateAsync(fetchedPosting, CancellationToken.None);

        await using var verifyContext = postgresFixture.CreateDbContext();
        var verifyRepository = new JobRepository(verifyContext);
        var afterUpdate = await verifyRepository.GetByIdAsync(newPosting.Id, CancellationToken.None);
        Assert.NotNull(afterUpdate);
        Assert.Equal("Staff .NET Engineer", afterUpdate.Title);
        Assert.Equal(200_000m, afterUpdate.SalaryMax);

        // Delete
        var deleted = await verifyRepository.DeleteAsync(newPosting.Id, CancellationToken.None);
        Assert.True(deleted);

        var afterDelete = await verifyRepository.GetByIdAsync(newPosting.Id, CancellationToken.None);
        Assert.Null(afterDelete);
    }

    [Fact]
    public async Task Should_ReturnFalse_When_DeletingMissingId()
    {
        await using var dbContext = postgresFixture.CreateDbContext();
        var jobRepository = new JobRepository(dbContext);

        var deleted = await jobRepository.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(deleted);
    }

    private static async Task<Company> CreateAndPersistCompanyAsync(KonnectDbContext dbContext)
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = $"Acme {Guid.NewGuid():N}",
            Slug = $"acme-{Guid.NewGuid():N}",
            Verified = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync();
        return company;
    }
}
