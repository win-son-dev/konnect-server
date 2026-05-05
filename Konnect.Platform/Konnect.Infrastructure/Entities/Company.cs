namespace Konnect.Infrastructure.Entities;

/// <summary>
/// An employer organisation. Owns many <see cref="RecruiterUser"/> and many
/// <see cref="JobPosting"/>. The <see cref="Slug"/> is the URL-safe public
/// identifier (e.g. <c>/companies/acme-corp</c>) and is unique across the
/// system.
/// </summary>
public class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public bool Verified { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RecruiterUser> Recruiters { get; set; } = [];

    public ICollection<JobPosting> JobPostings { get; set; } = [];
}
