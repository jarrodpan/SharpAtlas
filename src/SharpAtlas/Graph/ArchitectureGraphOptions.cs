namespace SharpAtlas.Graph;

public sealed record ArchitectureGraphOptions(
    bool IncludeTests,
    bool IncludeExternal,
    string GroupBy,
    IReadOnlySet<string> Relationships);
