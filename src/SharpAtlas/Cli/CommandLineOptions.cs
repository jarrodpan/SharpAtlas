using SharpAtlas.Graph;

namespace SharpAtlas.Cli;

public enum OutputFormat
{
    Json,
    Mermaid,
    All
}

public enum GroupByMode
{
    Namespace,
    Assembly
}

public sealed record CommandLineOptions
{
    public string? SolutionPath { get; init; }
    public string? ProjectPath { get; init; }
    public string OutputPath { get; init; } = Path.Combine("artifacts", "architecture");
    public OutputFormat Format { get; init; } = OutputFormat.All;
    public bool IncludeTests { get; init; }
    public bool IncludeExternal { get; init; }
    public GroupByMode GroupBy { get; init; } = GroupByMode.Namespace;
    public IReadOnlySet<string> Relationships { get; init; } = ArchitectureRelationship.All;
    public bool ShowHelp { get; init; }
    public ArchitectureGraphSource? Source { get; init; }
}
