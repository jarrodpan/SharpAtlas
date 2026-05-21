namespace SharpAtlas.Graph;

public sealed record ArchitectureGraph(
    string SchemaVersion,
    DateTime GeneratedAtUtc,
    ArchitectureGraphSource Source,
    ArchitectureGraphOptions Options,
    IReadOnlyList<string> Entrypoints,
    IReadOnlyList<ArchitectureNode> Nodes,
    IReadOnlyList<ArchitectureEdge> Edges);
