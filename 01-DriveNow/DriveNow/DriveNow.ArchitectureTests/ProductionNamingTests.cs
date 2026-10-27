using NetArchTest.Rules;
using Shouldly;
using System.Reflection;

namespace DriveNow.ArchitectureTests;

public class ProductionNamingTests
{
    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        var assemblies = new[]
        {
            GetAssembly(ProjectNames.Domain),
            GetAssembly(ProjectNames.Application),
            GetAssembly(ProjectNames.Infrastructure),
            GetAssembly(ProjectNames.ConsoleApp)
        };

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            result.IsSuccessful.ShouldBeTrue();
        }
    }

    [Theory]
    [MemberData(nameof(ProjectNames.NamingRules), MemberType = typeof(ProjectNames))]
    public void Classes_ShouldFollowNamingConvention(string endWith, string inNamespace, string layer)
    {
        var assembly = GetAssembly(layer);
        string suffix = endWith;
        string requiredNamespace = inNamespace;

        ShouldFollowNamingRule(assembly, suffix, requiredNamespace);
    }

    private static Assembly GetAssembly(string assemblyName)
    {
        return Assembly.Load(assemblyName);
    }

    private static void ShouldFollowNamingRule(Assembly assembly, string suffix, string requiredNamespace)
    {
        var result = Types.InAssembly(assembly)
            .That()
            .AreClasses()
            .And()
            .ResideInNamespaceStartingWith(ProjectNames.ProjectName)
            .And()
            .HaveNameEndingWith(suffix)
            .Should()
            .ResideInNamespaceMatching($@".*\.{requiredNamespace}$")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
