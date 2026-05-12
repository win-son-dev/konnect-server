using HotChocolate.Authorization;
using Konnect.Infrastructure.Services.Authentication;

namespace Konnect.GraphQL.Schema;

/// <summary>
/// Root <c>Query</c> type. Concrete read fields live on per-feature
/// <c>[ExtendObjectType(typeof(Query))]</c> classes (see e.g.
/// <c>CompanyQueries</c>) so each feature owns one resolver class with its
/// own constructor-injected services. The recruiter-scoped subtree is
/// reachable through the <see cref="Recruiter"/> wrapper, which is gated
/// once at this level so every nested field is gated by virtue of being
/// reachable.
/// </summary>
public class Query
{
    public string Healthcheck() => "ok";

    [Authorize(Roles = [JwtRoles.Recruiter])]
    public RecruiterQueries Recruiter() => new();
}
