using Konnect.Infrastructure.Contracts.Companies;
using Konnect.Infrastructure.Entities;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Infrastructure.Services.Companies.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Konnect.WebAPI.Controllers.Recruiter;

/// <summary>
/// Recruiter-scoped Company commands. Identity always comes from the JWT —
/// never from the route or body — so a recruiter can only ever modify their
/// own company. Reads live on the GraphQL surface
/// (<c>Query.recruiter.company</c>); this controller is the write side.
/// </summary>
[ApiController]
[Route("api/recruiter/company")]
[Authorize(Policy = AuthorizationPolicyNames.RecruiterAudience)]
public sealed class RecruiterCompanyController(ICompanyCommandService companyCommandService) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateCompanyInput input,
        CancellationToken cancellationToken)
    {
        var company = await companyCommandService.UpdateByRecruiterIdAsync(
            User.GetExternalId(),
            input,
            cancellationToken);

        return Ok(ToResponse(company));
    }

    private static UpdateCompanyResponse ToResponse(Company company)
        => new(
            company.Id,
            company.Name,
            company.Slug,
            company.Description,
            company.WebsiteUrl,
            company.Verified,
            company.UpdatedAt);
}

public sealed record UpdateCompanyResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    bool Verified,
    DateTimeOffset UpdatedAt);
