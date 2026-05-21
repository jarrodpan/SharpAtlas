namespace SharpAtlas.Graph;

public sealed class ArchitectureGraphBuilder
{
    private readonly ArchitectureGraphSource _source;
    private readonly ArchitectureGraphOptions _options;
    private readonly Dictionary<string, ArchitectureNode> _nodes = new(StringComparer.Ordinal);
    private readonly HashSet<ArchitectureEdge> _edges = new();
    private readonly HashSet<string> _entrypoints = new(StringComparer.Ordinal);

    public ArchitectureGraphBuilder(ArchitectureGraphSource source, ArchitectureGraphOptions options)
    {
        _source = source;
        _options = options;
    }

    public bool ContainsNode(string id) => _nodes.ContainsKey(id);

    public void AddNode(ArchitectureNode node)
    {
        _nodes.TryAdd(node.Id, node);
    }

    public void AddEdge(string from, string to, string relationship)
    {
        if (_options.Relationships.Count > 0 && !_options.Relationships.Contains(relationship))
        {
            return;
        }

        if (!_nodes.ContainsKey(from))
        {
            return;
        }

        if (!_options.IncludeExternal && !_nodes.ContainsKey(to))
        {
            return;
        }

        _edges.Add(new ArchitectureEdge(from, to, relationship));
    }

    public void AddEntrypoint(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            _entrypoints.Add(id);
        }
    }

    public ArchitectureGraph Build()
    {
        var nodes = _nodes.Values
            .OrderBy(node => node.Namespace, StringComparer.Ordinal)
            .ThenBy(node => node.Label, StringComparer.Ordinal)
            .ThenBy(node => node.Id, StringComparer.Ordinal)
            .ToArray();

        var edges = _edges
            .OrderBy(edge => edge.From, StringComparer.Ordinal)
            .ThenBy(edge => edge.To, StringComparer.Ordinal)
            .ThenBy(edge => edge.Relationship, StringComparer.Ordinal)
            .ToArray();

        var entrypoints = _entrypoints
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        return new ArchitectureGraph("1.0", DateTime.UtcNow, _source, _options, entrypoints, nodes, edges);
    }
}
