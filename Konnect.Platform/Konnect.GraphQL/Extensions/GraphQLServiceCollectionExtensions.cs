using Konnect.GraphQL.Schema;
using Konnect.GraphQL.Schema.Companies;
using Microsoft.Extensions.DependencyInjection;

namespace Konnect.GraphQL.Extensions;

public static class GraphQLServiceCollectionExtensions
{
    public static IServiceCollection AddKonnectGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<Query>()
            .AddType<CompanyType>()
            .AddTypeExtension<CompanyQueries>()
            .AddTypeExtension<RecruiterCompanyQueries>();

        return services;
    }
}
