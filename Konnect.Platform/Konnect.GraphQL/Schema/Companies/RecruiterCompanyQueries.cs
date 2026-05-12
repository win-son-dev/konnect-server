using System.Security.Claims;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Infrastructure.Services.Companies.Queries;

namespace Konnect.GraphQL.Schema.Companies;

/// <summary>
/// Recruiter-scoped Company query — adds <c>RecruiterQueries.company</c> to
/// the schema via HotChocolate's type-extension mechanism. The target
/// company is derived from the recruiter's JWT external_id, so a recruiter
/// can only ever read their own company through this field.
/// </summary>
[ExtendObjectType(typeof(RecruiterQueries))]
public sealed class RecruiterCompanyQueries(ICompanyQueryService companyQueryService)
{
    public Task<Company> Company(
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken)
        => companyQueryService.GetByRecruiterIdAsync(
            claimsPrincipal.GetExternalId(),
            cancellationToken);
}
