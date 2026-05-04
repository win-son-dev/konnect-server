namespace Konnect.Infrastructure.Entities;

/// <summary>
/// A recruiter account. Belongs to exactly one <see cref="Company"/>; a
/// company has many recruiters (the SEEK-style multi-recruiter model). FK
/// is M:1 (no unique constraint on <see cref="CompanyId"/>) — a single
/// company row can be referenced by many recruiter rows.
/// </summary>
public sealed class RecruiterUser : User
{
    public Guid CompanyId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public Company Company { get; set; } = null!;
}
