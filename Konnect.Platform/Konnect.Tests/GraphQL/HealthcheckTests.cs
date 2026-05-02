using HotChocolate;
using HotChocolate.Execution;
using Konnect.GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.Tests.GraphQL;

public class HealthcheckTests
{
    [Fact]
    public async Task GraphQL_HealthcheckQuery_Returns_Ok()
    {
        var services = new ServiceCollection();
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
