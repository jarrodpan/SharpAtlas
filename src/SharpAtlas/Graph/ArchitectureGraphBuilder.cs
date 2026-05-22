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

    public ArchitectureGraphOptions Options => _options;

    public bool ContainsNode(string id) => _nodes.ContainsKey(id);

    public void AddNode(ArchitectureNode node)
    {
        _nodes.TryAdd(node.Id, node);
    }

    public void AddEdge(string from, string to, string relationship)
    {
        if (!AllowsRelationship(relationship))
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

        if (_options.ClassReferencesOnly)
        {
            if (!IsClassReferenceNode(from) || !IsClassReferenceNode(to))
            {
                return;
            }

            relationship = ArchitectureRelationship.References;
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
        var edges = _edges
            .OrderBy(edge => edge.From, StringComparer.Ordinal)
            .ThenBy(edge => edge.To, StringComparer.Ordinal)
            .ThenBy(edge => edge.Relationship, StringComparer.Ordinal)
            .ToArray();

        var nodes = GetOutputNodes(edges)
            .OrderBy(node => node.Namespace, StringComparer.Ordinal)
            .ThenBy(node => node.Label, StringComparer.Ordinal)
            .ThenBy(node => node.Id, StringComparer.Ordinal)
            .ToArray();

        var nodeIds = nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var entrypoints = _entrypoints
            .Where(nodeIds.Contains)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        return new ArchitectureGraph("1.0", DateTime.UtcNow, _source, _options, entrypoints, nodes, edges);
    }

    private IEnumerable<ArchitectureNode> GetOutputNodes(IReadOnlyList<ArchitectureEdge> edges)
    {
        if (!_options.HideIsolated)
        {
            return _nodes.Values;
        }

        var connectedIds = edges
            .SelectMany(edge => new[] { edge.From, edge.To })
            .ToHashSet(StringComparer.Ordinal);

        return _nodes.Values.Where(node => connectedIds.Contains(node.Id));
    }

    private bool IsClassReferenceNode(string id)
    {
        return _nodes.TryGetValue(id, out var node) && node.Kind is "class" or "record";
    }

    private bool AllowsRelationship(string relationship)
    {
        return _options.Relationships.Count == 0 ||
            _options.Relationships.Contains(relationship) ||
            (_options.ClassReferencesOnly && _options.Relationships.Contains(ArchitectureRelationship.References));
    }
}
