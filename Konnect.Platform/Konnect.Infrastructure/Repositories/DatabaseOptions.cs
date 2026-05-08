namespace Konnect.Infrastructure.Repositories;

/// <summary>
/// Strongly-typed configuration for the Postgres-backed
/// <c>KonnectDbContext</c>. Bound from the <c>Database</c> section of
/// <c>appsettings.json</c> (and overridden per-environment via the standard
/// <c>appsettings.{Environment}.json</c>, environment variables, or
/// in-memory test settings). Keeping every infrastructure dependency behind
/// its own typed options class — <see cref="DatabaseOptions"/>,
/// <c>Auth0Settings</c>, future Redis / MinIO / Auth0-management options —
/// means the test pipeline can override one slice via
/// <c>services.Configure&lt;T&gt;(...)</c> without racing the
/// host-configuration build order, and the production code never reads
/// loose <c>IConfiguration</c> strings inline.
/// </summary>
public sealed record DatabaseOptions
{
    public const string SectionName = "Database";

    public string PostgresConnectionString { get; set; } = string.Empty;
}
