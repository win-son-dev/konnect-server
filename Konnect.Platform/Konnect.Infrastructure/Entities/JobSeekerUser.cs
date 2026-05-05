namespace Konnect.Infrastructure.Entities;

/// <summary>
/// A job-seeker account. Holds the seeker-specific fields directly — there is
/// no separate "profile" satellite table.
/// </summary>
public sealed class JobSeekerUser : User
{
    public string Headline { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public bool OpenToWork { get; set; }
}
