using Konnect.Infrastructure.Contracts.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Konnect.WebAPI.Authorization;

/// <summary>
/// Pins an endpoint to one side of the marketplace. Carries the
/// <see cref="AudienceType"/> the endpoint is meant for;
/// <see cref="AudienceRequirementHandler"/> verifies both that the JWT
/// <c>aud</c> claim matches the configured audience URL for that side AND
/// that the principal carries the role expected for that side.
/// </summary>
public sealed class AudienceRequirement(AudienceType audience) : IAuthorizationRequirement
{
    public AudienceType Audience { get; } = audience;
}
