using Konnect.Infrastructure.Entities;

namespace Konnect.Infrastructure.Repositories;

/// <summary>
/// Reads against the polymorphic <see cref="User"/> hierarchy keyed off the
/// Auth0 <c>external_id</c> Guid. Onboarding services lean on this to decide
/// whether a JWT-authenticated caller already has a domain row (idempotency)
/// or needs one provisioned. Subtype-specific writes for first-time
/// provisioning live on dedicated members:
/// <list type="bullet">
///   <item><see cref="AddJobSeekerAsync"/> — creates the seeker row</item>
///   <item>Recruiter creation is paired with company creation and lives on
///   <see cref="ICompanyRepository.AddWithFirstRecruiterAsync"/> so both rows
///   land in one transaction.</item>
/// </list>
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid externalId, CancellationToken cancellationToken);

    Task AddJobSeekerAsync(JobSeekerUser jobSeeker, CancellationToken cancellationToken);
}
