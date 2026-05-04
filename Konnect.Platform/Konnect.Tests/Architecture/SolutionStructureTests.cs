using System.Reflection;

namespace Konnect.Tests.Architecture;

public class SolutionStructureTests
{
    [Fact]
    public void WebApi_DoesNotReference_Serverless()
    {
        var webApiAssembly = Assembly.Load("Konnect.WebAPI");

        var referencedAssemblyNames = webApiAssembly.GetReferencedAssemblies()
            .Select(referencedAssembly => referencedAssembly.Name)
            .ToArray();

        Assert.DoesNotContain("Konnect.Serverless", referencedAssemblyNames);
    }

    [Fact]
    public void Serverless_DoesNotReference_WebApi()
    {
        var serverlessAssembly = Assembly.Load("Konnect.Serverless");

        var referencedAssemblyNames = serverlessAssembly.GetReferencedAssemblies()
            .Select(referencedAssembly => referencedAssembly.Name)
            .ToArray();

        Assert.DoesNotContain("Konnect.WebAPI", referencedAssemblyNames);
    }

    [Fact]
    public void WebApi_References_GraphQL()
    {
        var webApiAssembly = Assembly.Load("Konnect.WebAPI");

        var referencedAssemblyNames = webApiAssembly.GetReferencedAssemblies()
            .Select(referencedAssembly => referencedAssembly.Name)
            .ToArray();

        Assert.Contains("Konnect.GraphQL", referencedAssemblyNames);
    }

    [Fact]
    public void Infrastructure_ContainsNoConcreteImplementationClasses()
    {
        var infrastructureAssembly = Assembly.Load("Konnect.Infrastructure");

        // Entity POCOs (data shapes for EF Core / domain models) are explicitly
        // permitted in Konnect.Infrastructure.Entities — they're contracts, not
        // implementations. The Repository / Service interfaces in Infrastructure
        // need to reference them, so they must live alongside.
        var concreteImplementationTypes = infrastructureAssembly.GetExportedTypes()
            .Where(exportedType => exportedType.IsClass)
            .Where(exportedType => !exportedType.IsAbstract)
            .Where(exportedType => !IsRecordType(exportedType))
            .Where(exportedType => !IsCompilerGenerated(exportedType))
            .Where(exportedType => exportedType.Namespace != "Konnect.Infrastructure.Entities")
            .ToArray();

        Assert.Empty(concreteImplementationTypes);
    }

    private static bool IsRecordType(Type type) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Any(method => method.Name == "<Clone>$");

    private static bool IsCompilerGenerated(Type type) =>
        type.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false).Length > 0;
}
