using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SharpAtlas.Graph;
using SharpAtlas.Roslyn;

namespace SharpAtlas.Tests;

public sealed class TypeDependencyCollectorTests
{
    [Fact]
    public void CollectFindsConstructorFieldPropertyMethodInheritanceAndGenericDependencies()
    {
        const string source = """
            namespace App
            {
                public interface IThing { }
                public class BaseThing { }
                public class Dependency { }
                public class Other { }
                public class Source : BaseThing, IThing
                {
                    private readonly Dependency field;
                    public Other Property { get; }
                    public Source(Dependency dependency) { field = dependency; }
                    public System.Collections.Generic.List<Other> Run(Dependency dependency) => new();
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "App",
            [CSharpSyntaxTree.ParseText(source)],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location)
            ]);

        var sourceType = compilation.GetTypeByMetadataName("App.Source")!;
        var dependencies = TypeDependencyCollector.Collect(sourceType);
        var pairs = dependencies.Select(dependency => (TypeSymbolFormatter.GetId(dependency.Type), dependency.Relationship)).ToHashSet();

        Assert.Contains(("App.BaseThing", ArchitectureRelationship.Inherits), pairs);
        Assert.Contains(("App.IThing", ArchitectureRelationship.Implements), pairs);
        Assert.Contains(("App.Dependency", ArchitectureRelationship.Field), pairs);
        Assert.Contains(("App.Other", ArchitectureRelationship.Property), pairs);
        Assert.Contains(("App.Dependency", ArchitectureRelationship.Constructor), pairs);
        Assert.Contains(("App.Dependency", ArchitectureRelationship.MethodParameter), pairs);
        Assert.Contains(("System.Collections.Generic.List<T>", ArchitectureRelationship.MethodReturn), pairs);
        Assert.Contains(("App.Other", ArchitectureRelationship.Generic), pairs);
    }
}
