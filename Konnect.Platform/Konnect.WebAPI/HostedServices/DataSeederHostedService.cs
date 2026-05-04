using Konnect.Infrastructure.Services.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Konnect.WebAPI.HostedServices;

/// <summary>
/// Runs every required data-seeding step before the app starts accepting
/// requests. Implements <see cref="IHostedLifecycleService"/> so the work
/// happens in <see cref="StartingAsync"/> — strictly <em>before</em> the
/// HTTP listener opens, not concurrently with it. Future seeders are invoked
/// from this same orchestrator.
/// </summary>
public sealed class DataSeederHostedService : IHostedLifecycleService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<DataSeederHostedService> logger;

    public DataSeederHostedService(
        IServiceProvider serviceProvider,
        ILogger<DataSeederHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("DataSeederHostedService running pre-start seeding.");

        using var scope = serviceProvider.CreateScope();

        var roleSeederService = scope.ServiceProvider.GetRequiredService<IRoleSeederService>();
        await roleSeederService.SeedRequiredRolesAsync(cancellationToken);

        logger.LogInformation("DataSeederHostedService completed pre-start seeding.");
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
