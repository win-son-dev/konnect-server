namespace Konnect.Infrastructure.Services.Identity;

/// <summary>
/// Ensures every required Identity role exists in the database. Idempotent —
/// safe to call repeatedly (skips roles that already exist).
/// </summary>
public interface IRoleSeederService
{
    Task SeedRequiredRolesAsync(CancellationToken cancellationToken);
}
