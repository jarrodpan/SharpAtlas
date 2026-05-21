using SharpAtlas.Cli;

namespace SharpAtlas.Tests;

public sealed class SourceDiscoveryTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SharpAtlas.Tests", Guid.NewGuid().ToString("N"));

    public SourceDiscoveryTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Fact]
    public void ResolveFindsSingleSolution()
    {
        var solution = Path.Combine(_directory, "App.sln");
        File.WriteAllText(solution, string.Empty);

        var result = SourceDiscovery.Resolve(new CommandLineOptions(), _directory);

        Assert.True(result.Success);
        Assert.Equal("solution", result.Source!.Kind);
        Assert.Equal(solution, result.Source.Path);
    }

    [Fact]
    public void ResolveRequiresExplicitSolutionWhenMultipleFound()
    {
        File.WriteAllText(Path.Combine(_directory, "A.sln"), string.Empty);
        File.WriteAllText(Path.Combine(_directory, "B.sln"), string.Empty);

        var result = SourceDiscovery.Resolve(new CommandLineOptions(), _directory);

        Assert.False(result.Success);
        Assert.Contains("--solution", result.ErrorMessage);
    }

    [Fact]
    public void ResolveFallsBackToSingleProject()
    {
        var project = Path.Combine(_directory, "App.csproj");
        File.WriteAllText(project, string.Empty);

        var result = SourceDiscovery.Resolve(new CommandLineOptions(), _directory);

        Assert.True(result.Success);
        Assert.Equal("project", result.Source!.Kind);
        Assert.Equal(project, result.Source.Path);
    }
}
