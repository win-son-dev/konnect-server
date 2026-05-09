namespace Konnect.Infrastructure.Services.Onboarding;

/// <summary>
/// SPA-supplied payload for the post-Auth0 recruiter onboarding handshake.
/// The recruiter's identity (<c>external_id</c>, <c>email</c>) is taken from
/// the JWT — never from this input — so a recruiter cannot onboard another
/// user. Input is the company-plus-personal data that Auth0 doesn't capture
/// during sign-up.
/// </summary>
public sealed record OnboardRecruiterInput(
    string CompanyName,
    string CompanySlug,
    string? CompanyDescription,
    string? CompanyWebsiteUrl,
    string FirstName,
    string LastName,
    string JobTitle);
