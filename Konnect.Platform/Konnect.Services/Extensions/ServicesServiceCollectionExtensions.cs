using Konnect.Infrastructure.Services.Companies.Commands;
using Konnect.Infrastructure.Services.Companies.Queries;
using Konnect.Infrastructure.Services.Onboarding;
using Konnect.Services.Companies.Commands;
using Konnect.Services.Companies.Queries;
using Konnect.Services.Onboarding;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Services.Extensions;

/// <summary>
/// Single registration entry point for everything in <c>Konnect.Services</c>.
/// All Services are <c>Scoped</c> so they share the request-scoped DbContext
/// transitively reached through their injected repositories. Domain areas
/// follow the CQRS-lite split — one Query service + one Command service per
/// aggregate.
/// </summary>
public static class ServicesServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectServices(this IServiceCollection services)
    {
        services.AddScoped<IRecruiterOnboardingService, RecruiterOnboardingService>();
        services.AddScoped<IJobSeekerOnboardingService, JobSeekerOnboardingService>();

        services.AddScoped<ICompanyQueryService, CompanyQueryService>();
        services.AddScoped<ICompanyCommandService, CompanyCommandService>();

        return services;
    }
}
