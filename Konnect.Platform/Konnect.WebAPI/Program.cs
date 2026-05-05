using Konnect.GraphQL;
using Konnect.Repositories;
using Konnect.Infrastructure.Entities;
using Konnect.Services.Identity;
using Konnect.WebAPI.HostedServices;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddKonnectGraphQL();

var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Connection string 'Postgres' is required.");

builder.Services.AddKonnectRepositories(postgresConnectionString);

// Required by AddDefaultTokenProviders below — token providers need
// a registered IDataProtectionProvider to encrypt their tokens.
builder.Services.AddDataProtection();

builder.Services
    .AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 10;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<KonnectDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddKonnectIdentityServices();

builder.Services.AddHostedService<DataSeederHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

app.Run();
