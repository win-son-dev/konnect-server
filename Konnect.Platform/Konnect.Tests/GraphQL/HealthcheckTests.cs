using HotChocolate;
using HotChocolate.Execution;
using Konnect.GraphQL.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Tests.GraphQL;

public class HealthcheckTests
{
    [Fact]
    public async Task GraphQL_HealthcheckQuery_Returns_Ok()
    {
        var services = new ServiceCollection();
        // GraphQL's authorization middleware needs ASP.NET Core's
        // IAuthorizationService (which itself needs logging) at execution
        // time, even for fields that aren't gated. Production gets these via
        // Program.cs; this standalone harness wires the bare minimum.
        services.AddLogging();
        services.AddAuthorization();
        services.AddKonnectGraphQL();
        await using var serviceProvider = services.BuildServiceProvider();

        var executor = await serviceProvider
            .GetRequiredService<IRequestExecutorProvider>()
            .GetExecutorAsync();

        await using var result = await executor.ExecuteAsync("{ healthcheck }");

        var json = result.ToJson();
        Assert.Contains("\"healthcheck\": \"ok\"", json);
    }
}
