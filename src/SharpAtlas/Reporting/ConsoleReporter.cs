using SharpAtlas.Cli;
using SharpAtlas.Graph;
using SharpAtlas.Roslyn;

namespace SharpAtlas.Reporting;

public sealed class ConsoleReporter : IScanReporter
{
    public void Warning(string message) => Console.Error.WriteLine($"Warning: {message}");

    public void Error(string message) => Console.Error.WriteLine(message);

    public void Summary(ArchitectureGraph graph, CommandLineOptions options, string? jsonPath, string? mermaidPath, string? htmlPath, string? mermaidViewerPath)
    {
        Console.WriteLine("SharpAtlas architecture graph generated.");
        Console.WriteLine($"Source: {graph.Source.Path}");
        Console.WriteLine($"Nodes: {graph.Nodes.Count}");
        Console.WriteLine($"Edges: {graph.Edges.Count}");
        Console.WriteLine($"Entrypoints: {graph.Entrypoints.Count}");
        Console.WriteLine($"Tests included: {options.IncludeTests.ToString().ToLowerInvariant()}");
        Console.WriteLine($"External nodes included: {options.IncludeExternal.ToString().ToLowerInvariant()}");

        if (jsonPath is not null)
        {
            Console.WriteLine($"JSON: {jsonPath}");
        }

        if (mermaidPath is not null)
        {
            Console.WriteLine($"Mermaid: {mermaidPath}");
        }

        if (htmlPath is not null)
        {
            Console.WriteLine($"HTML: {htmlPath}");
        }

        if (mermaidViewerPath is not null)
        {
            Console.WriteLine($"Mermaid Viewer: {mermaidViewerPath}");
        }
    }
}
