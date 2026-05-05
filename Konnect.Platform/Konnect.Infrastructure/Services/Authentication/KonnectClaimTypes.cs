namespace Konnect.Infrastructure.Services.Authentication;

/// <summary>
/// JWT custom-claim names stamped by the Auth0 Post-Login Action. Auth0
/// requires custom claims to be URL-namespaced so they can never collide
/// with reserved JWT claims — the URLs do not have to resolve, they are just
/// stable identifiers.
/// </summary>
public static class KonnectClaimTypes
{
    public const string Namespace = "https://konnect.dev";

    /// <summary>
    /// Konnect's internal user Guid. Same value as Auth0's
    /// <c>app_metadata.external_id</c> — the Pre-User-Registration Action
    /// generates it, the Post-Login Action stamps it into every access token.
    /// Konnect's provisioning service parses this claim and uses it as the
    /// primary key into the <c>users</c> table.
    /// </summary>
    public const string ExternalId = $"{Namespace}/external_id";

    /// <summary>
    /// "JobSeeker" or "Recruiter". The Post-Login Action picks the value
    /// based on which client_id requested the token.
    /// </summary>
    public const string Role = $"{Namespace}/role";
}
