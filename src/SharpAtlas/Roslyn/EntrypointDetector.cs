using Microsoft.CodeAnalysis;

namespace SharpAtlas.Roslyn;

public static class EntrypointDetector
{
    private static readonly HashSet<string> EntrypointNames = new(StringComparer.Ordinal)
    {
        "Program",
        "Startup",
        "App",
        "Application",
        "HostBuilder"
    };

    public static bool IsPossibleEntrypoint(INamedTypeSymbol type)
    {
        if (EntrypointNames.Contains(type.Name))
        {
            return true;
        }

        return type.GetMembers().OfType<IMethodSymbol>().Any(method =>
            method.Name == "Main" &&
            method.IsStatic &&
            method.MethodKind == MethodKind.Ordinary);
    }
}
