using SharpAtlas.Graph;
using SharpAtlas.Output;

namespace SharpAtlas.Tests;

public sealed class ArchitectureGraphHtmlWriterTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SharpAtlas.Html.Tests", Guid.NewGuid().ToString("N"));

    public ArchitectureGraphHtmlWriterTests()
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
    public void WriteCreatesHtmlOutputFile()
    {
        var path = ArchitectureGraphHtmlWriter.Write(CreateGraph(), _directory);

        Assert.True(File.Exists(path));
        Assert.Equal("class-graph.html", Path.GetFileName(path));
    }

    [Fact]
    public void RenderEmbedsGraphJsonAndSharpAtlasHeader()
    {
        var html = ArchitectureGraphHtmlWriter.Render(CreateGraph());

        Assert.Contains("<h1>SharpAtlas</h1>", html);
        Assert.Contains("application/json", html);
        Assert.Contains("SharpAtlas.Cli.CommandLineParser", html);
        Assert.Contains("cytoscape", html);
    }

    [Fact]
    public void RenderSafelyEscapesEmbeddedJson()
    {
        var graph = CreateGraph(new ArchitectureNode(
            "SharpAtlas.Bad</script><script>alert(1)</script>",
            "Bad</script>",
            "SharpAtlas",
            "SharpAtlas",
            "class",
            "Bad.cs",
            false));

        var html = ArchitectureGraphHtmlWriter.Render(graph);

        Assert.DoesNotContain("SharpAtlas.Bad</script><script>alert(1)</script>", html);
        Assert.Contains("SharpAtlas.Bad\\u003C/script\\u003E\\u003Cscript\\u003Ealert", html);
    }

    private static ArchitectureGraph CreateGraph(ArchitectureNode? extraNode = null)
    {
        var nodes = new List<ArchitectureNode>
        {
            new(
                "SharpAtlas.Program",
                "Program",
                "SharpAtlas",
                "SharpAtlas",
                "class",
                "Program.cs",
                false),
            new(
                "SharpAtlas.Cli.CommandLineParser",
                "CommandLineParser",
                "SharpAtlas.Cli",
                "SharpAtlas",
                "class",
                "Cli/CommandLineParser.cs",
                false)
        };

        if (extraNode is not null)
        {
            nodes.Add(extraNode);
        }

        return new ArchitectureGraph(
            "1.0",
            new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            new ArchitectureGraphSource("project", "SharpAtlas.csproj"),
            new ArchitectureGraphOptions(false, false, "namespace", ArchitectureRelationship.All),
            ["SharpAtlas.Program"],
            nodes,
            [new ArchitectureEdge("SharpAtlas.Program", "SharpAtlas.Cli.CommandLineParser", ArchitectureRelationship.MethodParameter)]);
    }
}
