using Konnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Konnect.Repositories;

/// <summary>
/// Single registration entry point for everything in <c>Konnect.Repositories</c> —
/// the DbContext (with snake_case naming) and every <c>IXxxRepository</c> binding.
/// All entity Repositories are <c>Scoped</c> to match the DbContext lifetime.
/// </summary>
public static class RepositoriesServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectRepositories(this IServiceCollection services)
    {
        // Resolve the connection string lazily through IOptions so test
        // overrides (services.Configure<DatabaseOptions>(...)) win over the
        // production binding regardless of registration order.
        services.AddDbContext<KonnectDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider
                .GetRequiredService<IOptions<DatabaseOptions>>().Value;

            options.UseNpgsql(databaseOptions.PostgresConnectionString);
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        return services;
    }
}
