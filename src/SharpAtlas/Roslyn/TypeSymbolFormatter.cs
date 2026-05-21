using Microsoft.CodeAnalysis;

namespace SharpAtlas.Roslyn;

public static class TypeSymbolFormatter
{
    public static string GetId(INamedTypeSymbol symbol)
    {
        var parts = new Stack<string>();
        INamedTypeSymbol? current = symbol.OriginalDefinition;
        while (current is not null)
        {
            parts.Push(GetLabel(current));
            current = current.ContainingType;
        }

        var typeName = string.Join(".", parts);
        var namespaceName = GetNamespace(symbol);
        return string.IsNullOrWhiteSpace(namespaceName) ? typeName : $"{namespaceName}.{typeName}";
    }

    public static string GetLabel(INamedTypeSymbol symbol)
    {
        var name = symbol.MetadataName;
        var tickIndex = name.IndexOf('`', StringComparison.Ordinal);
        if (tickIndex >= 0)
        {
            name = name[..tickIndex];
        }

        if (symbol.TypeParameters.Length == 0)
        {
            return name;
        }

        return $"{name}<{string.Join(", ", symbol.TypeParameters.Select(parameter => parameter.Name))}>";
    }

    public static string GetNamespace(INamedTypeSymbol symbol)
    {
        var namespaceSymbol = symbol.ContainingNamespace;
        return namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace
            ? string.Empty
            : namespaceSymbol.ToDisplayString();
    }

    public static string GetKind(INamedTypeSymbol symbol)
    {
        return symbol.TypeKind switch
        {
            TypeKind.Class when symbol.IsRecord => "record",
            TypeKind.Class => "class",
            TypeKind.Struct when symbol.IsRecord => "record struct",
            TypeKind.Struct => "struct",
            TypeKind.Interface => "interface",
            TypeKind.Enum => "enum",
            TypeKind.Delegate => "delegate",
            _ => symbol.TypeKind.ToString().ToLowerInvariant()
        };
    }
}
