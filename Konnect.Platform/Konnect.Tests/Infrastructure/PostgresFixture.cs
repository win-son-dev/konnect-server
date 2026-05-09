using Konnect.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Konnect.Tests.Infrastructure;

/// <summary>
/// Spins up a Postgres container for the duration of a test collection.
/// Uses the same <c>pgvector/pgvector:pg17</c> image as the dev compose stack
/// so the test schema matches what runs locally and in production. Applies
/// the EF migrations once on startup.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder("pgvector/pgvector:pg17")
        .WithDatabase("konnect_test")
        .WithUsername("konnect_test")
        .WithPassword("konnect_test_only")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await container.DisposeAsync();

    public KonnectDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<KonnectDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new KonnectDbContext(options);
    }
}

/// <summary>
/// xUnit collection-definition marker. Test classes that need the shared
/// Postgres container declare <c>[Collection(DatabaseTestSuite.Name)]</c> —
/// xUnit groups them under one collection so they share a single
/// <see cref="PostgresFixture"/> instance across the test run.
/// </summary>
[CollectionDefinition(Name)]
public sealed class DatabaseTestSuite : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Database";
}
