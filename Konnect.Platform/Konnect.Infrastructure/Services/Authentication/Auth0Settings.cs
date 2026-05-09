namespace Konnect.Infrastructure.Services.Authentication;

/// <summary>
/// Auth0 OIDC settings consumed by the JwtBearer scheme. Bound from the
/// <c>Auth0</c> section of <c>appsettings.json</c> at startup. Konnect's API
/// never initiates logins or calls the Auth0 Management API, so no
/// client_secret is configured here — only the public OIDC discovery values.
/// </summary>
public sealed record Auth0Settings
{
    public const string SectionName = "Auth0";

    /// <summary>
    /// The Auth0 tenant domain, e.g. <c>konnect-dev.au.auth0.com</c>. The
    /// JwtBearer Authority is derived as <c>https://{Domain}/</c> and used to
    /// fetch the JWKS document and validate the <c>iss</c> claim.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// The audience identifier of the seeker-side Auth0 API resource —
    /// becomes the JWT <c>aud</c> for tokens issued to the Seeker SPA.
    /// Example: <c>https://api.konnect.dev/seeker</c>.
    /// </summary>
    public string SeekerAudience { get; set; } = string.Empty;

    /// <summary>
    /// The audience identifier of the employer-side Auth0 API resource.
    /// Example: <c>https://api.konnect.dev/employer</c>.
    /// </summary>
    public string EmployerAudience { get; set; } = string.Empty;
}
