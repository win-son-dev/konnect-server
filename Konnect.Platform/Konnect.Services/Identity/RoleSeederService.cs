using Konnect.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Konnect.Services.Identity;

/// <summary>
/// Seeds the four Identity roles every authorization policy in Konnect
/// references. Run on every app start via <c>DataSeederHostedService</c>;
/// the <see cref="RoleManager{TRole}.RoleExistsAsync"/> check makes the
/// operation idempotent so re-runs on an already-seeded database are safe.
/// </summary>
public sealed class RoleSeederService : IRoleSeederService
{
    private static readonly string[] RequiredRoleNames =
    [
        "JobSeeker",
        "Recruiter",
        "CompanyAdmin",
        "Admin",
    ];

    private readonly RoleManager<IdentityRole<Guid>> roleManager;
    private readonly ILogger<RoleSeederService> logger;

    public RoleSeederService(
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<RoleSeederService> logger)
    {
        this.roleManager = roleManager;
        this.logger = logger;
    }

    public async Task SeedRequiredRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in RequiredRoleNames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var newRole = new IdentityRole<Guid>(roleName);
            var result = await roleManager.CreateAsync(newRole);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
                throw new InvalidOperationException($"Failed to seed Identity role '{roleName}': {errors}");
            }

            logger.LogInformation("Seeded Identity role {RoleName}.", roleName);
        }
    }
}
