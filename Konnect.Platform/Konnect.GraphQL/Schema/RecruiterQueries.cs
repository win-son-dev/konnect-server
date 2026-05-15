namespace Konnect.GraphQL.Schema;

/// <summary>
/// Wrapper type for recruiter-scoped GraphQL query fields. Mounted under
/// <c>Query.recruiter</c>; the parent field carries the
/// <c>RecruiterAudience</c> policy so every nested field is gated by virtue
/// of being reachable. Concrete fields live on per-feature
/// <c>[ExtendObjectType(typeof(RecruiterQueries))]</c> classes (see e.g.
/// <c>RecruiterCompanyQueries</c>) — each owns its own constructor-injected
/// services.
/// </summary>
public class RecruiterQueries
{
}
