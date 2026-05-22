using SharpAtlas.Graph;

namespace SharpAtlas.Tests;

public sealed class GraphBuilderTests
{
    [Fact]
    public void BuildRemovesDuplicateEdges()
    {
        var builder = new ArchitectureGraphBuilder(
            new ArchitectureGraphSource("project", "app.csproj"),
            new ArchitectureGraphOptions(false, false, "namespace", ArchitectureRelationship.All));

        builder.AddNode(new ArchitectureNode("App.A", "A", "App", "App", "class", "A.cs", false));
        builder.AddNode(new ArchitectureNode("App.B", "B", "App", "App", "class", "B.cs", false));

        builder.AddEdge("App.A", "App.B", ArchitectureRelationship.Constructor);
        builder.AddEdge("App.A", "App.B", ArchitectureRelationship.Constructor);

        var graph = builder.Build();

        Assert.Single(graph.Edges);
    }

    [Fact]
    public void BuildSkipsExternalEdgesByDefault()
    {
        var builder = new ArchitectureGraphBuilder(
            new ArchitectureGraphSource("project", "app.csproj"),
            new ArchitectureGraphOptions(false, false, "namespace", ArchitectureRelationship.All));

        builder.AddNode(new ArchitectureNode("App.A", "A", "App", "App", "class", "A.cs", false));
        builder.AddEdge("App.A", "System.String", ArchitectureRelationship.Property);

        Assert.Empty(builder.Build().Edges);
    }

    [Fact]
    public void ClassReferencesOnlyCollapsesClassEdgesToReferences()
    {
        var builder = new ArchitectureGraphBuilder(
            new ArchitectureGraphSource("project", "app.csproj"),
            new ArchitectureGraphOptions(false, false, "namespace", ArchitectureRelationship.All, true));

        builder.AddNode(new ArchitectureNode("App.A", "A", "App", "App", "class", "A.cs", false));
        builder.AddNode(new ArchitectureNode("App.B", "B", "App", "App", "class", "B.cs", false));

        builder.AddEdge("App.A", "App.B", ArchitectureRelationship.MethodParameter);
        builder.AddEdge("App.A", "App.B", ArchitectureRelationship.Field);

        var edge = Assert.Single(builder.Build().Edges);
        Assert.Equal(ArchitectureRelationship.References, edge.Relationship);
    }

    [Fact]
    public void ClassReferencesOnlySkipsNonClassEdges()
    {
        var builder = new ArchitectureGraphBuilder(
            new ArchitectureGraphSource("project", "app.csproj"),
            new ArchitectureGraphOptions(false, false, "namespace", ArchitectureRelationship.All, true));

        builder.AddNode(new ArchitectureNode("App.A", "A", "App", "App", "class", "A.cs", false));
        builder.AddNode(new ArchitectureNode("App.IThing", "IThing", "App", "App", "interface", "IThing.cs", false));

        builder.AddEdge("App.A", "App.IThing", ArchitectureRelationship.Implements);

        Assert.Empty(builder.Build().Edges);
    }

    [Fact]
    public void ClassReferencesOnlyAllowsReferencesRelationshipFilter()
    {
        var builder = new ArchitectureGraphBuilder(
            new ArchitectureGraphSource("project", "app.csproj"),
            new ArchitectureGraphOptions(
                false,
                false,
                "namespace",
                new HashSet<string>(StringComparer.Ordinal) { ArchitectureRelationship.References },
                true));

        builder.AddNode(new ArchitectureNode("App.A", "A", "App", "App", "class", "A.cs", false));
        builder.AddNode(new ArchitectureNode("App.B", "B", "App", "App", "class", "B.cs", false));

        builder.AddEdge("App.A", "App.B", ArchitectureRelationship.MethodReturn);

        var edge = Assert.Single(builder.Build().Edges);
        Assert.Equal(ArchitectureRelationship.References, edge.Relationship);
    }
}
