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
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
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

    public async Task DisposeAsync()
    {
        await container.DisposeAsync();
    }

    public KonnectDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<KonnectDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new KonnectDbContext(options);
    }
}

[CollectionDefinition(Name)]
public sealed class DatabaseCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Database";
}
