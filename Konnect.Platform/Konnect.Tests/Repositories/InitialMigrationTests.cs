using Konnect.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Tests.Repositories;

/// <summary>
/// Pins the schema produced by the <c>InitialCreate</c> migration. If a future
/// edit to entities or configurations changes the table set without an
/// accompanying migration, this test starts failing — making schema drift
/// visible before it ships.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class InitialMigrationTests
{
    private readonly PostgresFixture postgresFixture;

    public InitialMigrationTests(PostgresFixture postgresFixture)
    {
        this.postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task Should_CreateAllExpectedTables_When_InitialMigrationApplied()
    {
        await using var dbContext = postgresFixture.CreateDbContext();

        var tableNames = await dbContext.Database
            .SqlQuery<string>(
                $"SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename")
            .ToListAsync();

        string[] expectedTables =
        [
            "companies",
            "job_postings",
            "role_claims",
            "roles",
            "user_claims",
            "user_logins",
            "user_roles",
            "user_tokens",
            "users",
        ];

        foreach (var expectedTable in expectedTables)
        {
            Assert.Contains(expectedTable, tableNames);
        }
    }

    [Fact]
    public async Task Should_HaveAudienceDiscriminatorColumn_OnUsersTable()
    {
        await using var dbContext = postgresFixture.CreateDbContext();

        var columnNames = await dbContext.Database
            .SqlQuery<string>(
                $"SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'users'")
            .ToListAsync();

        Assert.Contains("audience", columnNames);
    }
}
