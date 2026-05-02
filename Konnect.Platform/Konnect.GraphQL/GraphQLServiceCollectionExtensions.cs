using Konnect.GraphQL.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.GraphQL;

public static class GraphQLServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<Query>();

        return services;
    }
}
