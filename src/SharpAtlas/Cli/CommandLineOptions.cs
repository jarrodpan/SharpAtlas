using SharpAtlas.Graph;

namespace SharpAtlas.Cli;

public enum OutputFormat
{
    Json,
    Mermaid,
    Html,
    All
}

public enum GroupByMode
{
    Namespace,
    Assembly,
    Project,
    NamespaceHierarchy
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

public static class OutputFormatSelection
{
    public static bool ShouldWriteJson(OutputFormat format) => format is OutputFormat.Json or OutputFormat.Html or OutputFormat.All;

    public static bool ShouldWriteMermaid(OutputFormat format) => format is OutputFormat.Mermaid or OutputFormat.All;

    public static bool ShouldWriteMermaidViewer(OutputFormat format) => ShouldWriteMermaid(format);

    public static bool ShouldWriteHtml(OutputFormat format) => format is OutputFormat.Html or OutputFormat.All;
}
