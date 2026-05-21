namespace SharpAtlas.Graph;

public sealed record ArchitectureNode(
    string Id,
    string Label,
    string Namespace,
    string Assembly,
    string Kind,
    string? File,
    bool IsExternal);
