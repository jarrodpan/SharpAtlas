using SharpAtlas.Cli;
using SharpAtlas.Graph;

namespace SharpAtlas.Output;

public static class ArchitectureGraphMermaidWriter
{
    public static string Write(ArchitectureGraph graph, string outputDirectory, GroupByMode groupBy)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, "class-graph.mmd");
        File.WriteAllText(path, Render(graph, groupBy));
        return path;
    }

    public static string Render(ArchitectureGraph graph, GroupByMode groupBy)
    {
        var lines = new List<string>
        {
            "flowchart LR",
            string.Empty
        };

        var nodesByGroup = graph.Nodes.GroupBy(node => GetGroup(node, groupBy))
            .OrderBy(group => group.Key, StringComparer.Ordinal);

        foreach (var group in nodesByGroup)
        {
            var groupId = MermaidIdSanitizer.Sanitize("group_" + group.Key);
            lines.Add($"    subgraph {groupId}[\"{Escape(group.Key)}\"]");
            foreach (var node in group.OrderBy(node => node.Label, StringComparer.Ordinal).ThenBy(node => node.Id, StringComparer.Ordinal))
            {
                lines.Add($"        {MermaidIdSanitizer.Sanitize(node.Id)}[\"{Escape(node.Label)}\"]");
            }

            lines.Add("    end");
            lines.Add(string.Empty);
        }

        foreach (var edge in graph.Edges)
        {
            lines.Add($"    {MermaidIdSanitizer.Sanitize(edge.From)} -->|{Escape(edge.Relationship)}| {MermaidIdSanitizer.Sanitize(edge.To)}");
        }

        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static string GetGroup(ArchitectureNode node, GroupByMode groupBy)
    {
        var value = groupBy == GroupByMode.Assembly ? node.Assembly : node.Namespace;
        return string.IsNullOrWhiteSpace(value) ? "Global" : value;
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
