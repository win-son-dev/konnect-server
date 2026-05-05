namespace Konnect.Infrastructure.Entities;

/// <summary>
/// Abstract base for every Konnect account. Konnect does not store passwords —
/// authentication is delegated to Auth0 — so this entity holds only the
/// profile fields shared by both audiences. <see cref="Id"/> is the same Guid
/// that Auth0 stores in <c>app_metadata.external_id</c> for the same user
/// (set by the Pre-User-Registration Action) and that arrives in every JWT
/// as the <c>https://konnect.dev/external_id</c> custom claim. Persisted via
/// TPH (table-per-hierarchy) — one <c>users</c> table with an <c>audience</c>
/// discriminator column.
/// </summary>
public abstract class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
