namespace Konnect.Infrastructure.Contracts.Companies;

/// <summary>
/// Recruiter-supplied payload for editing the company profile they own.
/// Slug is intentionally NOT updatable here — renaming a company's public
/// URL slug needs its own flow with uniqueness validation and a redirect
/// from the old slug, none of which Phase 1 ships. <c>Verified</c> is also
/// off-limits to recruiter self-edits — it's an admin/moderation flag.
/// </summary>
public sealed record UpdateCompanyInput(
    string Name,
    string? Description,
    string? WebsiteUrl);
