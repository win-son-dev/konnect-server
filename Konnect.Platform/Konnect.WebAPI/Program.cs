using Konnect.GraphQL;
using Konnect.Infrastructure.Repositories;
using Konnect.Infrastructure.Services.Authentication;
using Konnect.Repositories;
using Konnect.Services;
using Konnect.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddKonnectGraphQL();

// Strongly-typed options for every infrastructure dependency. Bound lazily
// so the validators run when the options are first resolved, and so test
// overrides via services.Configure<T> apply cleanly.
builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.PostgresConnectionString),
        "Database:PostgresConnectionString is required.");

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddOptions<DatabaseOptions>().ValidateOnStart();
}

builder.Services.AddKonnectRepositories();
builder.Services.AddKonnectServices();

// Bind the Auth0 section lazily. Eager reads (e.g. .Get<Auth0Settings>())
// would happen before WebApplicationFactory's in-memory configuration
// overlay reaches the configuration tree, so integration tests cannot
// substitute their own values. The validator below runs at host start in
// non-test environments to catch a missing prod config early.
builder.Services
    .AddOptions<Auth0Settings>()
    .Bind(builder.Configuration.GetSection(Auth0Settings.SectionName))
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Domain),
        "Auth0:Domain is required.");

if (!builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddOptions<Auth0Settings>().ValidateOnStart();
}

var requireHttpsMetadata = !builder.Environment.IsDevelopment();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<Auth0Settings>>((jwtBearerOptions, auth0Options) =>
    {
        var auth0Settings = auth0Options.Value;
        var auth0Authority = $"https://{auth0Settings.Domain}/";

        jwtBearerOptions.Authority = auth0Authority;
        jwtBearerOptions.RequireHttpsMetadata = requireHttpsMetadata;
        jwtBearerOptions.MapInboundClaims = false;
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = auth0Authority,
            ValidateAudience = true,
            ValidAudiences =
            [
                auth0Settings.SeekerAudience,
                auth0Settings.EmployerAudience,
            ],
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            // The Post-Login Action stamps the role under our namespaced
            // claim — wire it through so [Authorize(Roles = "JobSeeker")]
            // reads the right value.
            RoleClaimType = KonnectClaimTypes.Role,
            NameClaimType = KonnectClaimTypes.ExternalId,
        };
    });

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<KonnectAuthenticationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

app.Run();
