using SharpAtlas.Graph;

namespace SharpAtlas.Cli;

public sealed record SourceDiscoveryResult(bool Success, ArchitectureGraphSource? Source, string? ErrorMessage)
{
    public static SourceDiscoveryResult Ok(ArchitectureGraphSource source) => new(true, source, null);
    public static SourceDiscoveryResult Fail(string errorMessage) => new(false, null, errorMessage);
}

public static class SourceDiscovery
{
    public static SourceDiscoveryResult Resolve(CommandLineOptions options, string currentDirectory)
    {
        if (!string.IsNullOrWhiteSpace(options.SolutionPath))
        {
            return ResolveExplicit("solution", options.SolutionPath);
        }

        if (!string.IsNullOrWhiteSpace(options.ProjectPath))
        {
            return ResolveExplicit("project", options.ProjectPath);
        }

        var solutions = Directory.EnumerateFiles(currentDirectory, "*.sln", SearchOption.TopDirectoryOnly).ToArray();
        if (solutions.Length == 1)
        {
            return SourceDiscoveryResult.Ok(new ArchitectureGraphSource("solution", Path.GetFullPath(solutions[0])));
        }

        if (solutions.Length > 1)
        {
            return SourceDiscoveryResult.Fail("Multiple .sln files found. Provide --solution.");
        }

        var projects = Directory.EnumerateFiles(currentDirectory, "*.csproj", SearchOption.TopDirectoryOnly).ToArray();
        if (projects.Length == 1)
        {
            return SourceDiscoveryResult.Ok(new ArchitectureGraphSource("project", Path.GetFullPath(projects[0])));
        }

        if (projects.Length > 1)
        {
            return SourceDiscoveryResult.Fail("Multiple .csproj files found. Provide --project.");
        }

        return SourceDiscoveryResult.Fail("No .sln or .csproj found. Provide --solution or --project.");
    }

    private static SourceDiscoveryResult ResolveExplicit(string kind, string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
        {
            return SourceDiscoveryResult.Fail($"{kind} not found: {path}");
        }

        return SourceDiscoveryResult.Ok(new ArchitectureGraphSource(kind, fullPath));
    }
}
