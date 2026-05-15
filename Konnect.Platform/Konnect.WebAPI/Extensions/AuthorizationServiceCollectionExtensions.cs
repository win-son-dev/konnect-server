using Konnect.Infrastructure.Contracts.Enums;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.WebAPI.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Konnect.WebAPI.Extensions;

/// <summary>
/// Registers Konnect's named authorization policies and the requirement
/// handlers that back them. One well-known place to look for what a policy
/// name means — the constants live in
/// <see cref="AuthorizationPolicyNames"/>, the requirement + handler live
/// alongside in <c>Konnect.WebAPI/Authorization</c>, and the wiring lives
/// here so <c>Program.cs</c> stays small.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, AudienceRequirementHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicyNames.SeekerAudience,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AudienceRequirement(AudienceType.JobSeeker)));

            options.AddPolicy(
                AuthorizationPolicyNames.RecruiterAudience,
                policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AudienceRequirement(AudienceType.Recruiter)));
        });

        return services;
    }
}
