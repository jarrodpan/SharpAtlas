using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using SharpAtlas.Cli;
using SharpAtlas.Graph;

namespace SharpAtlas.Roslyn;

public sealed class RoslynArchitectureScanner
{
    private readonly IScanReporter _reporter;

    public RoslynArchitectureScanner(IScanReporter reporter)
    {
        _reporter = reporter;
    }

    public async Task<ArchitectureGraph> ScanAsync(CommandLineOptions options, CancellationToken cancellationToken)
    {
        EnsureMSBuildRegistered();

        var source = options.Source ?? throw new InvalidOperationException("No source was resolved.");
        var graphOptions = new ArchitectureGraphOptions(
            options.IncludeTests,
            options.IncludeExternal,
            options.GroupBy.ToString().ToLowerInvariant(),
            options.Relationships);

        var builder = new ArchitectureGraphBuilder(source, graphOptions);
        var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed += (_, e) => _reporter.Warning(e.Diagnostic.Message);

        var projects = source.Kind == "solution"
            ? (await workspace.OpenSolutionAsync(source.Path, cancellationToken: cancellationToken)).Projects
            : new[] { await workspace.OpenProjectAsync(source.Path, cancellationToken: cancellationToken) };

        var includedProjects = projects
            .Where(project => options.IncludeTests || !TestExclusionRules.IsTestProject(project))
            .ToArray();

        var symbols = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);
        var sourceRoot = Path.GetDirectoryName(source.Path) ?? Directory.GetCurrentDirectory();

        foreach (var project in includedProjects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var compilation = await project.GetCompilationAsync(cancellationToken);
            if (compilation is null)
            {
                _reporter.Warning($"Could not compile project: {project.Name}");
                continue;
            }

            AddDeclaredTypes(project, compilation, sourceRoot, options.IncludeTests, builder, symbols, cancellationToken);
        }

        foreach (var symbol in symbols.Values)
        {
            if (EntrypointDetector.IsPossibleEntrypoint(symbol))
            {
                builder.AddEntrypoint(TypeSymbolFormatter.GetId(symbol));
            }

            foreach (var dependency in TypeDependencyCollector.Collect(symbol))
            {
                AddDependencyEdge(builder, symbols, dependency, symbol, options.IncludeExternal);
            }
        }

        return builder.Build();
    }

    private static void EnsureMSBuildRegistered()
    {
        if (!MSBuildLocator.IsRegistered)
        {
            MSBuildLocator.RegisterDefaults();
        }
    }

    private void AddDeclaredTypes(
        Project project,
        Compilation compilation,
        string sourceRoot,
        bool includeTests,
        ArchitectureGraphBuilder builder,
        Dictionary<string, INamedTypeSymbol> symbols,
        CancellationToken cancellationToken)
    {
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!includeTests && TestExclusionRules.IsTestFile(syntaxTree.FilePath))
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot(cancellationToken);

            foreach (var declaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol symbol)
                {
                    continue;
                }

                AddDeclaredType(project, sourceRoot, includeTests, builder, symbols, symbol);
            }

            foreach (var declaration in root.DescendantNodes().OfType<DelegateDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol symbol)
                {
                    continue;
                }

                AddDeclaredType(project, sourceRoot, includeTests, builder, symbols, symbol);
            }

            if (root.DescendantNodes().OfType<GlobalStatementSyntax>().Any())
            {
                var programId = $"{project.AssemblyName ?? project.Name}.Program";
                builder.AddNode(new ArchitectureNode(
                    programId,
                    "Program",
                    project.AssemblyName ?? project.Name,
                    project.AssemblyName ?? project.Name,
                    "class",
                    Path.GetRelativePath(sourceRoot, syntaxTree.FilePath),
                    false));
                builder.AddEntrypoint(programId);
            }
        }
    }

    private static void AddDeclaredType(
        Project project,
        string sourceRoot,
        bool includeTests,
        ArchitectureGraphBuilder builder,
        Dictionary<string, INamedTypeSymbol> symbols,
        INamedTypeSymbol symbol)
    {
        if (!includeTests && TestExclusionRules.IsTestType(symbol))
        {
            return;
        }

        var id = TypeSymbolFormatter.GetId(symbol);
        if (symbols.ContainsKey(id))
        {
            return;
        }

        symbols.Add(id, symbol);
        builder.AddNode(new ArchitectureNode(
            id,
            TypeSymbolFormatter.GetLabel(symbol),
            TypeSymbolFormatter.GetNamespace(symbol),
            symbol.ContainingAssembly?.Name ?? project.AssemblyName ?? project.Name,
            TypeSymbolFormatter.GetKind(symbol),
            GetRelativeFile(symbol, sourceRoot),
            false));
    }

    private static string? GetRelativeFile(INamedTypeSymbol symbol, string sourceRoot)
    {
        var path = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.FilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return Path.GetRelativePath(sourceRoot, path);
    }

    private static void AddDependencyEdge(
        ArchitectureGraphBuilder builder,
        IReadOnlyDictionary<string, INamedTypeSymbol> declaredSymbols,
        TypeDependency dependency,
        INamedTypeSymbol sourceSymbol,
        bool includeExternal)
    {
        var from = TypeSymbolFormatter.GetId(sourceSymbol);
        var to = TypeSymbolFormatter.GetId(dependency.Type);
        if (from == to)
        {
            return;
        }

        if (includeExternal && !builder.ContainsNode(to))
        {
            builder.AddNode(new ArchitectureNode(
                to,
                TypeSymbolFormatter.GetLabel(dependency.Type),
                TypeSymbolFormatter.GetNamespace(dependency.Type),
                dependency.Type.ContainingAssembly?.Name ?? string.Empty,
                TypeSymbolFormatter.GetKind(dependency.Type),
                null,
                true));
        }

        if (declaredSymbols.ContainsKey(to) || includeExternal)
        {
            builder.AddEdge(from, to, dependency.Relationship);
        }
    }
}
