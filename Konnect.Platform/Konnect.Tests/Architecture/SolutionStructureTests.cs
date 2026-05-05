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

    [Theory]
    [InlineData("Konnect.Infrastructure")]
    [InlineData("Konnect.Repositories")]
    [InlineData("Konnect.Services")]
    [InlineData("Konnect.GraphQL")]
    [InlineData("Konnect.WebAPI")]
    [InlineData("Konnect.Worker")]
    [InlineData("Konnect.Serverless")]
    public void NoProject_References_AspNetCoreIdentity(string assemblyName)
    {
        // Konnect outsources credentials to Auth0; ASP.NET Core Identity is
        // intentionally absent from the entire solution. Catching the
        // dependency creeping back in (via a transitive package add or a
        // copy-pasted snippet) is the point of this test.
        var assembly = Assembly.Load(assemblyName);

        var referencedAssemblyNames = assembly.GetReferencedAssemblies()
            .Select(referencedAssembly => referencedAssembly.Name)
            .ToArray();

        Assert.DoesNotContain("Microsoft.AspNetCore.Identity", referencedAssemblyNames);
        Assert.DoesNotContain("Microsoft.AspNetCore.Identity.EntityFrameworkCore", referencedAssemblyNames);
        Assert.DoesNotContain("Microsoft.Extensions.Identity.Core", referencedAssemblyNames);
        Assert.DoesNotContain("Microsoft.Extensions.Identity.Stores", referencedAssemblyNames);
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
