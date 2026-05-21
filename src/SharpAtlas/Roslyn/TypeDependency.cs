using Microsoft.CodeAnalysis;

namespace SharpAtlas.Roslyn;

public sealed record TypeDependency(INamedTypeSymbol Type, string Relationship);
