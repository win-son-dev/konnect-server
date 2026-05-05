using Konnect.WebAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
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
public sealed class KonnectWebApplicationFactory : WebApplicationFactory<WebApiEntryPoint>
{
    public TestJwtTokenFactory TokenFactory { get; } = new();

    public const string SeekerAudience = "https://api.konnect.test/seeker";

    public const string EmployerAudience = "https://api.konnect.test/employer";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Real Postgres is irrelevant for auth-pipeline tests — every
                // assertion is about the bearer scheme + middleware. The
                // connection string just has to be present so AddDbContext
                // does not throw at startup.
                ["ConnectionStrings:Postgres"] =
                    "Host=127.0.0.1;Port=5432;Database=konnect_unused;Username=u;Password=p",
                ["Auth0:Domain"] = "konnect-test.auth0.local",
                ["Auth0:SeekerAudience"] = SeekerAudience,
                ["Auth0:EmployerAudience"] = EmployerAudience,
            });
        });

        builder.ConfigureTestServices(services =>
        {
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
    private sealed class TestJwtBearerOptionsPostConfigure : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly TestJwtTokenFactory tokenFactory;

        public TestJwtBearerOptionsPostConfigure(TestJwtTokenFactory tokenFactory)
        {
            this.tokenFactory = tokenFactory;
        }

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
