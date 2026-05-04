using Microsoft.AspNetCore.Identity;

namespace Konnect.Infrastructure.Entities;

/// <summary>
/// Abstract base for every Konnect account. The audience-specific subclasses
/// (<see cref="JobSeekerUser"/>, <see cref="RecruiterUser"/>) are the entities
/// callers actually instantiate — this base only carries the columns shared by
/// both audiences. Persisted via TPH (table-per-hierarchy) — one
/// <c>asp_net_users</c> table with an <c>audience</c> discriminator column.
/// </summary>
public abstract class User : IdentityUser<Guid>
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
