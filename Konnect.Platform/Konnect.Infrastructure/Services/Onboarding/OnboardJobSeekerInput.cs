namespace Konnect.Infrastructure.Services.Onboarding;

/// <summary>
/// SPA-supplied payload for the post-Auth0 job-seeker onboarding handshake.
/// Identity (<c>external_id</c>, <c>email</c>) comes from the JWT. All
/// fields here are profile-side values the seeker can edit later.
/// </summary>
public sealed record OnboardJobSeekerInput(
    string? Headline,
    string? Location,
    bool OpenToWork);
