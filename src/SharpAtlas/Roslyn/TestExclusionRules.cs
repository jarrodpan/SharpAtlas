using Microsoft.CodeAnalysis;

namespace SharpAtlas.Roslyn;

public static class TestExclusionRules
{
    private static readonly string[] TestAttributeNames =
    {
        "FactAttribute",
        "TheoryAttribute",
        "TestAttribute",
        "TestCaseAttribute",
        "TestClassAttribute",
        "TestFixtureAttribute"
    };

    public static bool IsTestProject(Project project)
    {
        return ContainsTestToken(project.Name) ||
            ContainsTestToken(project.AssemblyName) ||
            IsTestPath(project.FilePath);
    }

    public static bool IsTestFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var fileName = Path.GetFileNameWithoutExtension(path);
        return ContainsTestToken(fileName) || IsTestPath(path);
    }

    public static bool IsTestType(INamedTypeSymbol symbol)
    {
        return IsTestNamespace(symbol.ContainingNamespace?.ToDisplayString()) ||
            HasTestAttributes(symbol) ||
            symbol.GetMembers().Any(member => member.GetAttributes().Any(IsTestAttribute));
    }

    public static bool IsTestNamespace(string? namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
        {
            return false;
        }

        return namespaceName.Contains(".Test", StringComparison.OrdinalIgnoreCase) ||
            namespaceName.Contains(".Tests", StringComparison.OrdinalIgnoreCase) ||
            namespaceName.EndsWith("Test", StringComparison.OrdinalIgnoreCase) ||
            namespaceName.EndsWith("Tests", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTestPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/Tests/", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains(".Tests", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsTestToken(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            value.Contains("Test", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasTestAttributes(ISymbol symbol) => symbol.GetAttributes().Any(IsTestAttribute);

    private static bool IsTestAttribute(AttributeData attribute)
    {
        var name = attribute.AttributeClass?.Name;
        return name is not null && TestAttributeNames.Contains(name, StringComparer.Ordinal);
    }
}
