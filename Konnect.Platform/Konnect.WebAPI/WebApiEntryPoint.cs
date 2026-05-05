namespace Konnect.WebAPI;

/// <summary>
/// Marker type for <c>WebApplicationFactory&lt;TEntryPoint&gt;</c>. The
/// auto-generated <c>Program</c> class from top-level statements collides
/// with the <c>Program</c> class in <c>Konnect.Serverless</c> when both
/// projects are referenced from <c>Konnect.Tests</c>; this typed marker
/// disambiguates the WebAPI entry point.
/// </summary>
public sealed class WebApiEntryPoint;
