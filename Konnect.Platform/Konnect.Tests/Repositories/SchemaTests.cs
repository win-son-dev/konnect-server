using Konnect.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Konnect.Tests.Repositories;

/// <summary>
/// Pins the schema produced by all migrations applied to a fresh database.
/// If a future entity / configuration / migration change drifts from the
/// expected post-Auth0-pivot shape, this test starts failing. Especially
/// guards against the asp_net_* Identity tables sneaking back in.
/// </summary>
[Collection(DatabaseTestSuite.Name)]
public class SchemaTests(PostgresFixture postgresFixture)
{
    [Fact]
    public async Task Should_HaveExpectedTableSet_When_AllMigrationsApplied()
    {
        await using var dbContext = postgresFixture.CreateDbContext();

        var tableNames = (await dbContext.Database
            .SqlQuery<string>(
                $"SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename")
            .ToListAsync()).ToHashSet();

        string[] expectedTables =
        [
            "companies",
            "job_postings",
            "users",
        ];
        foreach (var expectedTable in expectedTables)
        {
            Assert.Contains(expectedTable, tableNames);
        }

        // The asp_net_* Identity tables MUST NOT exist after the
        // DropIdentityAndUseExternalId migration.
        string[] forbiddenTables =
        [
            "roles",
            "role_claims",
            "user_roles",
            "user_claims",
            "user_logins",
            "user_tokens",
        ];
        foreach (var forbiddenTable in forbiddenTables)
        {
            Assert.DoesNotContain(forbiddenTable, tableNames);
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

    [Fact]
    public async Task Should_NotHaveIdentityColumns_OnUsersTable()
    {
        await using var dbContext = postgresFixture.CreateDbContext();

        var columnNames = (await dbContext.Database
            .SqlQuery<string>(
                $"SELECT column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'users'")
            .ToListAsync()).ToHashSet();

        string[] forbiddenColumns =
        [
            "password_hash",
            "normalized_email",
            "normalized_user_name",
            "user_name",
            "concurrency_stamp",
            "security_stamp",
            "two_factor_enabled",
            "lockout_enabled",
            "lockout_end",
            "access_failed_count",
            "phone_number",
            "phone_number_confirmed",
            "email_confirmed",
        ];
        foreach (var forbiddenColumn in forbiddenColumns)
        {
            Assert.DoesNotContain(forbiddenColumn, columnNames);
        }
    }
}
