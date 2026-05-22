using SharpAtlas.Cli;
using SharpAtlas.Graph;
using SharpAtlas.Output;

namespace SharpAtlas.Tests;

public sealed class ArchitectureGraphMermaidWriterTests
{
    [Fact]
    public void RenderCanGroupByProject()
    {
        var graph = CreateGraph();

        var mermaid = ArchitectureGraphMermaidWriter.Render(graph, GroupByMode.Project);

        Assert.Contains("src_App_App_csproj", mermaid);
        Assert.Contains("[\"src/App/App.csproj\"]", mermaid);
    }

    [Fact]
    public void RenderCanNestNamespaceHierarchy()
    {
        var graph = CreateGraph();

        var mermaid = ArchitectureGraphMermaidWriter.Render(graph, GroupByMode.NamespaceHierarchy);

        Assert.Contains("subgraph namespace_App[\"App\"]", mermaid);
        Assert.Contains("subgraph namespace_App_Core[\"App.Core\"]", mermaid);
        Assert.Contains("subgraph namespace_App_Core_Engine[\"App.Core.Engine\"]", mermaid);
        Assert.Contains("App_Core_Engine_Level[\"Level\"]", mermaid);
    }

    private static ArchitectureGraph CreateGraph()
    {
        var level = new ArchitectureNode(
            "App.Core.Engine.Level",
            "Level",
            "App.Core.Engine",
            "App",
            "class",
            "Level.cs",
            false)
        {
            Project = "src/App/App.csproj"
        };

        var controller = new ArchitectureNode(
            "App.Controllers.LevelController",
            "LevelController",
            "App.Controllers",
            "App",
            "class",
            "LevelController.cs",
            false)
        {
            Project = "src/App/App.csproj"
        };

        return new ArchitectureGraph(
            "1.0",
            new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            new ArchitectureGraphSource("project", "App.csproj"),
            new ArchitectureGraphOptions(false, false, "namespace", ArchitectureRelationship.All),
            [],
            [level, controller],
            [new ArchitectureEdge(controller.Id, level.Id, ArchitectureRelationship.Constructor)]);
    }
}
