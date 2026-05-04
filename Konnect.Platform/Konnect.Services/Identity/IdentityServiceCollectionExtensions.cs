using Konnect.Infrastructure.Services.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Services.Identity;

/// <summary>
/// Registration entry point for Identity-domain services that live in
/// <c>Konnect.Services.Identity</c>.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<IRoleSeederService, RoleSeederService>();
        return services;
    }
}
