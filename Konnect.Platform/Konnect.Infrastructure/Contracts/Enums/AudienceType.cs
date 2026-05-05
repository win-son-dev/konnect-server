namespace Konnect.Infrastructure.Contracts.Enums;

/// <summary>
/// Identifies which side of the marketplace an account belongs to.
/// An account is exactly one audience for its lifetime — never both.
/// Stored as a string column so values stay readable in the database.
/// </summary>
public enum AudienceType
{
    JobSeeker,
    Recruiter,
}
