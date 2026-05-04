using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Identity;
using Konnect.Repositories;
using Konnect.Services.Identity;
using Konnect.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Tests.Services.Identity;

/// <summary>
/// Integration test for <see cref="RoleSeederService"/> against a real
/// Postgres container. Mock-based unit tests are skipped here because
/// <c>RoleManager</c> is the framework-provided "role repository" — wrapping
/// it just to mock it would add abstraction without value. The integration
/// test verifies seeding + idempotency end-to-end.
/// </summary>
[Collection(DatabaseCollection.Name)]
public class RoleSeederServiceTests
{
    private readonly PostgresFixture postgresFixture;

    public RoleSeederServiceTests(PostgresFixture postgresFixture)
    {
        this.postgresFixture = postgresFixture;
    }

    [Fact]
    public async Task Should_SeedAllFourRequiredRoles_When_DatabaseIsEmpty()
    {
        await using var serviceProvider = BuildServiceProvider();

        await using var initialScope = serviceProvider.CreateAsyncScope();
        var seeder = initialScope.ServiceProvider.GetRequiredService<IRoleSeederService>();
        await seeder.SeedRequiredRolesAsync(CancellationToken.None);

        await using var verifyContext = postgresFixture.CreateDbContext();
        var roleNames = await verifyContext.Roles
            .Select(role => role.Name!)
            .OrderBy(name => name)
            .ToListAsync(CancellationToken.None);

        Assert.Contains("Admin", roleNames);
        Assert.Contains("CompanyAdmin", roleNames);
        Assert.Contains("JobSeeker", roleNames);
        Assert.Contains("Recruiter", roleNames);
    }

    [Fact]
    public async Task Should_BeIdempotent_When_RunTwiceInARow()
    {
        await using var serviceProvider = BuildServiceProvider();

        await using (var firstScope = serviceProvider.CreateAsyncScope())
        {
            var firstSeeder = firstScope.ServiceProvider.GetRequiredService<IRoleSeederService>();
            await firstSeeder.SeedRequiredRolesAsync(CancellationToken.None);
        }

        await using (var secondScope = serviceProvider.CreateAsyncScope())
        {
            var secondSeeder = secondScope.ServiceProvider.GetRequiredService<IRoleSeederService>();
            await secondSeeder.SeedRequiredRolesAsync(CancellationToken.None);
        }

        await using var verifyContext = postgresFixture.CreateDbContext();
        var roleCount = await verifyContext.Roles.CountAsync(CancellationToken.None);

        // Four required roles, no duplicates.
        Assert.Equal(4, roleCount);
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDataProtection();

        services.AddDbContext<KonnectDbContext>(options =>
        {
            options.UseNpgsql(postgresFixture.ConnectionString);
            options.UseSnakeCaseNamingConvention();
        });

        services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<KonnectDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IRoleSeederService, RoleSeederService>();

        return services.BuildServiceProvider();
    }
}
