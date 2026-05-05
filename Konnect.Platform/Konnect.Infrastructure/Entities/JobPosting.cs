using Konnect.Infrastructure.Contracts.Enums;

namespace Konnect.Infrastructure.Entities;

/// <summary>
/// A job opening published by a <see cref="Company"/>. The pgvector embedding
/// column lands in Phase 3; the lexical <c>tsvector</c> column lands in
/// Phase 4. Salary range is optional — postings can be listed without one.
/// </summary>
public class JobPosting
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public EmploymentType EmploymentType { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTimeOffset PostedAt { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public bool IsActive { get; set; }

    public Company Company { get; set; } = null!;
}
