using Konnect.Infrastructure.Entities;

namespace Konnect.GraphQL.Schema.Companies;

/// <summary>
/// Public GraphQL representation of <see cref="Company"/>. Navigation
/// collections (<c>Recruiters</c>, <c>JobPostings</c>) are intentionally
/// hidden: the public schema exposes companies for browse/profile only, and
/// recruiter rosters are never world-readable. Job postings will be exposed
/// via a separate top-level field once Issue #25 lands rather than as a
/// nested projection here.
/// </summary>
public sealed class CompanyType : ObjectType<Company>
{
    protected override void Configure(IObjectTypeDescriptor<Company> descriptor)
    {
        descriptor.Description("An employer organisation that posts jobs.");
        descriptor.Ignore(company => company.Recruiters);
        descriptor.Ignore(company => company.JobPostings);
    }
}
