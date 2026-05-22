using SharpAtlas.Cli;
using SharpAtlas.Graph;

namespace SharpAtlas.Output;

public static class ArchitectureGraphMermaidWriter
{
    public static string Write(ArchitectureGraph graph, string outputDirectory, GroupByMode groupBy)
    {
        return Write(Render(graph, groupBy), outputDirectory);
    }

    public static string Write(string mermaidSource, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, "class-graph.mmd");
        File.WriteAllText(path, mermaidSource);
        return path;
    }

    public static string Render(ArchitectureGraph graph, GroupByMode groupBy)
    {
        if (groupBy == GroupByMode.NamespaceHierarchy)
        {
            return RenderNamespaceHierarchy(graph);
        }

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
        var value = groupBy switch
        {
            GroupByMode.Assembly => node.Assembly,
            GroupByMode.Project => node.Project,
            _ => node.Namespace
        };

        return string.IsNullOrWhiteSpace(value) ? "Global" : value;
    }

    private static string RenderNamespaceHierarchy(ArchitectureGraph graph)
    {
        var lines = new List<string>
        {
            "flowchart LR",
            string.Empty
        };

        var root = new NamespaceGroup("Global", string.Empty);
        foreach (var node in graph.Nodes.OrderBy(node => node.Namespace, StringComparer.Ordinal).ThenBy(node => node.Label, StringComparer.Ordinal))
        {
            AddNodeToNamespaceTree(root, node);
        }

        foreach (var child in root.Children.Values.OrderBy(child => child.FullName, StringComparer.Ordinal))
        {
            RenderNamespaceGroup(lines, child, 1);
        }

        foreach (var node in root.Nodes.OrderBy(node => node.Label, StringComparer.Ordinal).ThenBy(node => node.Id, StringComparer.Ordinal))
        {
            lines.Add($"    {MermaidIdSanitizer.Sanitize(node.Id)}[\"{Escape(node.Label)}\"]");
        }

        lines.Add(string.Empty);

        foreach (var edge in graph.Edges)
        {
            lines.Add($"    {MermaidIdSanitizer.Sanitize(edge.From)} -->|{Escape(edge.Relationship)}| {MermaidIdSanitizer.Sanitize(edge.To)}");
        }

        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddNodeToNamespaceTree(NamespaceGroup root, ArchitectureNode node)
    {
        if (string.IsNullOrWhiteSpace(node.Namespace))
        {
            root.Nodes.Add(node);
            return;
        }

        var current = root;
        var parts = node.Namespace.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var fullName = string.Empty;
        foreach (var part in parts)
        {
            fullName = string.IsNullOrWhiteSpace(fullName) ? part : $"{fullName}.{part}";
            if (!current.Children.TryGetValue(part, out var child))
            {
                child = new NamespaceGroup(part, fullName);
                current.Children.Add(part, child);
            }

            current = child;
        }

        current.Nodes.Add(node);
    }

    private static void RenderNamespaceGroup(List<string> lines, NamespaceGroup group, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var groupId = MermaidIdSanitizer.Sanitize("namespace_" + group.FullName);
        lines.Add($"{indent}subgraph {groupId}[\"{Escape(group.FullName)}\"]");

        foreach (var child in group.Children.Values.OrderBy(child => child.FullName, StringComparer.Ordinal))
        {
            RenderNamespaceGroup(lines, child, indentLevel + 1);
        }

        foreach (var node in group.Nodes.OrderBy(node => node.Label, StringComparer.Ordinal).ThenBy(node => node.Id, StringComparer.Ordinal))
        {
            lines.Add($"{indent}    {MermaidIdSanitizer.Sanitize(node.Id)}[\"{Escape(node.Label)}\"]");
        }

        lines.Add($"{indent}end");
        lines.Add(string.Empty);
    }

    private sealed class NamespaceGroup
    {
        public NamespaceGroup(string name, string fullName)
        {
            Name = name;
            FullName = fullName;
        }

        public string Name { get; }
        public string FullName { get; }
        public Dictionary<string, NamespaceGroup> Children { get; } = new(StringComparer.Ordinal);
        public List<ArchitectureNode> Nodes { get; } = [];
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
}
