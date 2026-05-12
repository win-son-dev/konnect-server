using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Companies.Queries;

namespace Konnect.GraphQL.Schema.Companies;

/// <summary>
/// Public Company query field — adds <c>Query.company(slug)</c> to the
/// schema via HotChocolate's type-extension mechanism. Services are
/// constructor-injected so the resolver method only carries GraphQL field
/// arguments + the cancellation token.
/// </summary>
[ExtendObjectType(typeof(Query))]
public sealed class CompanyQueries(ICompanyQueryService companyQueryService)
{
    public Task<Company?> Company(string slug, CancellationToken cancellationToken)
        => companyQueryService.GetBySlugAsync(slug, cancellationToken);
}
