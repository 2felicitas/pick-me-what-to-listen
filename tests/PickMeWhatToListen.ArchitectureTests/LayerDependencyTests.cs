using NetArchTest.Rules;
using PickMeWhatToListen.Application;
using PickMeWhatToListen.Domain;
using PickMeWhatToListen.Infrastructure;
using PickMeWhatToListen.Wpf;

namespace PickMeWhatToListen.ArchitectureTests;

/// <summary>
/// Mechanically enforces the layer diagram in ARCHITECTURE.md:
/// Domain &lt;- Application &lt;- Infrastructure/Wpf, one-way only.
/// </summary>
public class LayerDependencyTests
{
    [Fact]
    public void Domain_Should_Not_DependOn_OuterLayers()
    {
        var result = Types.InAssembly(typeof(Artist).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "PickMeWhatToListen.Application",
                "PickMeWhatToListen.Infrastructure",
                "PickMeWhatToListen.Wpf")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_Should_Not_DependOn_InfrastructureOrWpf()
    {
        var result = Types.InAssembly(typeof(ArtistCatalogService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "PickMeWhatToListen.Infrastructure",
                "PickMeWhatToListen.Wpf")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Wpf_Should_Not_DependOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(App).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
