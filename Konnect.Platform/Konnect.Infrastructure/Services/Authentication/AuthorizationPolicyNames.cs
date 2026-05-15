namespace Konnect.Infrastructure.Services.Authentication;

/// <summary>
/// Named ASP.NET Core authorization policies. Referenced from
/// <c>[Authorize(Policy = ...)]</c> attributes on REST controllers and from
/// HotChocolate <c>[Authorize(Policy = ...)]</c> attributes on GraphQL
/// resolvers. The constants live in <c>Konnect.Infrastructure</c> so both
/// <c>Konnect.WebAPI</c> and <c>Konnect.GraphQL</c> can reference the same
/// names without circular project references. Concrete policy registration
/// (requirements + handlers) lives in <c>Konnect.WebAPI/Authorization</c>.
/// </summary>
public static class AuthorizationPolicyNames
{
    /// <summary>
    /// JWT must carry the seeker-side <c>aud</c> claim AND the
    /// <c>JobSeeker</c> role. Both checks are independent — a recruiter token
    /// re-issued with the seeker audience (or vice versa) is still rejected.
    /// </summary>
    public const string SeekerAudience = "SeekerAudience";

    /// <summary>
    /// JWT must carry the recruiter-side <c>aud</c> claim AND the
    /// <c>Recruiter</c> role.
    /// </summary>
    public const string RecruiterAudience = "RecruiterAudience";
}
