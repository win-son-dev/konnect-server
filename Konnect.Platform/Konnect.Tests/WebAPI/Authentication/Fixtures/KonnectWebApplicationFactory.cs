using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.WebAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Konnect.Tests.WebAPI.Authentication.Fixtures;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> wired so the JwtBearer
/// scheme accepts tokens minted by <see cref="TestJwtTokenFactory"/> instead
/// of real Auth0 tokens. The override pre-populates
/// <see cref="JwtBearerOptions.Configuration"/> with our test signing key so
/// the bearer scheme never tries to fetch OIDC discovery from the
/// (non-existent) Auth0 tenant.
/// </summary>
public class KonnectWebApplicationFactory : WebApplicationFactory<WebApiEntryPoint>
{
    public TestJwtTokenFactory TokenFactory { get; } = new();

    public const string SeekerAudience = "https://api.konnect.test/seeker";

    public const string RecruiterAudience = "https://api.konnect.test/recruiter";

    /// <summary>
    /// Postgres connection string the test host should bind to. Defaults to a
    /// dummy value that's good enough for tests which never open a real
    /// connection (the entire auth-pipeline suite); DB-backed integration
    /// tests assign a Testcontainers-backed connection string from
    /// <see cref="Konnect.Tests.Infrastructure.PostgresFixture"/> before
    /// creating a client. xUnit's <c>IClassFixture&lt;T&gt;</c> requires a
    /// single parameterless ctor, so the override path is a settable
    /// property rather than a ctor parameter.
    /// </summary>
    public string PostgresConnectionString { get; set; }
        = "Host=127.0.0.1;Port=5432;Database=konnect_unused;Username=u;Password=p";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Override every typed-options binding via Configure<T>.
        // ConfigureTestServices runs LATE (after Program.cs has registered
        // its bindings) and last-call-wins semantics mean these reliably
        // substitute the test values regardless of when Program.cs reads
        // configuration — which dodges the host-builder timing race that
        // ConfigureAppConfiguration is subject to in minimal hosting.
        builder.ConfigureTestServices(services =>
        {
            services.Configure<DatabaseOptions>(options =>
            {
                options.PostgresConnectionString = PostgresConnectionString;
            });

            services.Configure<Auth0Settings>(options =>
            {
                options.Domain = "konnect-test.auth0.local";
                options.SeekerAudience = SeekerAudience;
                options.RecruiterAudience = RecruiterAudience;
            });

            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(
                _ => new TestJwtBearerOptionsPostConfigure(TokenFactory));
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            TokenFactory.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Runs after the production <c>JwtBearerOptions</c> setup and replaces
    /// the issuer + signing key with the test fixture's values. By setting
    /// <see cref="JwtBearerOptions.Configuration"/> directly we bypass the
    /// metadata-fetch path entirely — the bearer scheme never makes an HTTP
    /// call out of the test process.
    /// </summary>
    private sealed class TestJwtBearerOptionsPostConfigure(TestJwtTokenFactory tokenFactory)
        : IPostConfigureOptions<JwtBearerOptions>
    {
        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.Authority = null;
            options.RequireHttpsMetadata = false;

            var configuration = new OpenIdConnectConfiguration
            {
                Issuer = TestJwtTokenFactory.Issuer,
            };
            configuration.SigningKeys.Add(tokenFactory.SigningKey);
            options.Configuration = configuration;

            options.TokenValidationParameters.ValidIssuer = TestJwtTokenFactory.Issuer;
            options.TokenValidationParameters.IssuerSigningKey = tokenFactory.SigningKey;
        }
    }
}
