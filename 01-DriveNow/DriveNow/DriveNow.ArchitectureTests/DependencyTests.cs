using NetArchTest.Rules;
using Shouldly;
using System.Reflection;

namespace DriveNow.ArchitectureTests;

public class DependencyTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOtherLayer()
    {
        var domainAssembly = Assembly.Load(ProjectNames.Domain);

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ProjectNames.Application,
                ProjectNames.Infrastructure,
                ProjectNames.ConsoleApp)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Application_ShouldDependOnlyOnDomain()
    {
        var applicationAssembly = Assembly.Load(ProjectNames.Application);

        var result = Types.InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ProjectNames.Infrastructure,
                ProjectNames.ConsoleApp)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnConsoleApp()
    {
        var infrastructureAssembly = Assembly.Load(ProjectNames.Infrastructure);

        var result = Types.InAssembly(infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ProjectNames.ConsoleApp)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
