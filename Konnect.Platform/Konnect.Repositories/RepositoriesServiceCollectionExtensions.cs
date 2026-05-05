using Konnect.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Repositories;

/// <summary>
/// Single registration entry point for everything in <c>Konnect.Repositories</c> —
/// the DbContext (with snake_case naming) and every <c>IXxxRepository</c> binding.
/// All entity Repositories are <c>Scoped</c> to match the DbContext lifetime.
/// </summary>
public static class RepositoriesServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectRepositories(
        this IServiceCollection services,
        string postgresConnectionString)
    {
        services.AddDbContext<KonnectDbContext>(options =>
        {
            options.UseNpgsql(postgresConnectionString);
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IJobRepository, JobRepository>();

        return services;
    }
}
