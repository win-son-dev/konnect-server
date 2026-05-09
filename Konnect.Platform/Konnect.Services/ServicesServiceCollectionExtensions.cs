using Konnect.Infrastructure.Services.Onboarding;
using Konnect.Services.Onboarding;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Services;

/// <summary>
/// Single registration entry point for everything in <c>Konnect.Services</c>.
/// All Services are <c>Scoped</c> so they share the request-scoped DbContext
/// transitively reached through their injected repositories.
/// </summary>
public static class ServicesServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectServices(this IServiceCollection services)
    {
        services.AddScoped<IRecruiterOnboardingService, RecruiterOnboardingService>();
        services.AddScoped<IJobSeekerOnboardingService, JobSeekerOnboardingService>();

        return services;
    }
}
