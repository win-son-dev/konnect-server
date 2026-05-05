namespace Konnect.Infrastructure.Services.Authentication;

/// <summary>
/// Stable role names assigned to the JWT <c>https://konnect.dev/role</c>
/// claim by the Auth0 Post-Login Action. Kept here so authorization policies
/// reference the constants instead of magic strings.
/// </summary>
public static class JwtRoles
{
    public const string JobSeeker = "JobSeeker";

    public const string Recruiter = "Recruiter";
}
