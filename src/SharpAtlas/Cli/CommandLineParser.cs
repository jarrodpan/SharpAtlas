using SharpAtlas.Graph;

namespace SharpAtlas.Cli;

public sealed record CommandLineParseResult(bool Success, CommandLineOptions? Options, string? ErrorMessage)
{
    public static CommandLineParseResult Ok(CommandLineOptions options) => new(true, options, null);
    public static CommandLineParseResult Fail(string errorMessage) => new(false, null, errorMessage);
}

public static class CommandLineParser
{
    public static CommandLineParseResult Parse(string[] args)
    {
        var solutionPath = (string?)null;
        var projectPath = (string?)null;
        var outputPath = Path.Combine("artifacts", "architecture");
        var format = OutputFormat.All;
        var includeTests = false;
        var includeExternal = false;
        var groupBy = GroupByMode.Namespace;
        var relationships = ArchitectureRelationship.All.ToHashSet(StringComparer.Ordinal);
        var explicitRelationships = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--help":
                case "-h":
                    return CommandLineParseResult.Ok(new CommandLineOptions { ShowHelp = true });
                case "--solution":
                    if (!TryReadValue(args, ref i, out solutionPath))
                    {
                        return CommandLineParseResult.Fail("--solution requires a path.");
                    }

                    break;
                case "--project":
                    if (!TryReadValue(args, ref i, out projectPath))
                    {
                        return CommandLineParseResult.Fail("--project requires a path.");
                    }

                    break;
                case "--output":
                    if (!TryReadValue(args, ref i, out outputPath))
                    {
                        return CommandLineParseResult.Fail("--output requires a path.");
                    }

                    break;
                case "--format":
                    if (!TryReadValue(args, ref i, out var formatValue) || !TryParseFormat(formatValue, out format))
                    {
                        return CommandLineParseResult.Fail("--format must be json, mermaid, or all.");
                    }

                    break;
                case "--include-tests":
                    includeTests = true;
                    break;
                case "--include-external":
                    includeExternal = true;
                    break;
                case "--relationship":
                    if (!TryReadValue(args, ref i, out var relationshipValue))
                    {
                        return CommandLineParseResult.Fail("--relationship requires a comma-separated value.");
                    }

                    var parsedRelationships = ParseRelationships(relationshipValue);
                    if (parsedRelationships is null)
                    {
                        return CommandLineParseResult.Fail($"Unsupported relationship value. Supported: {string.Join(", ", ArchitectureRelationship.All)}.");
                    }

                    if (!explicitRelationships)
                    {
                        relationships.Clear();
                        explicitRelationships = true;
                    }

                    foreach (var relationship in parsedRelationships)
                    {
                        relationships.Add(relationship);
                    }

                    break;
                case "--group-by":
                    if (!TryReadValue(args, ref i, out var groupByValue) || !TryParseGroupBy(groupByValue, out groupBy))
                    {
                        return CommandLineParseResult.Fail("--group-by must be namespace or assembly.");
                    }

                    break;
                default:
                    return CommandLineParseResult.Fail($"Unknown option: {arg}");
            }
        }

        if (!string.IsNullOrWhiteSpace(solutionPath) && !string.IsNullOrWhiteSpace(projectPath))
        {
            return CommandLineParseResult.Fail("Provide either --solution or --project, not both.");
        }

        return CommandLineParseResult.Ok(new CommandLineOptions
        {
            SolutionPath = solutionPath,
            ProjectPath = projectPath,
            OutputPath = outputPath,
            Format = format,
            IncludeTests = includeTests,
            IncludeExternal = includeExternal,
            GroupBy = groupBy,
            Relationships = relationships
        });
    }

    public static string GetHelpText() =>
        """
        SharpAtlas generates class/type dependency maps for C# codebases.

        Usage:
          sharp-atlas --solution path/to/App.sln --output artifacts/architecture --format all
          sharp-atlas --project path/to/App.csproj --output artifacts/architecture --format json

        Options:
          --solution <path>       C# solution to scan.
          --project <path>        C# project to scan.
          --output <path>         Output directory. Default: artifacts/architecture.
          --format <value>        json, mermaid, or all. Default: all.
          --include-tests         Include test projects and files.
          --include-external      Include external/library/framework type nodes.
          --relationship <list>   Comma-separated relationship filters.
          --group-by <value>      namespace or assembly. Default: namespace.
          --help                  Show help.
        """;

    public static IReadOnlySet<string>? ParseRelationships(string value)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return null;
        }

        foreach (var part in parts)
        {
            if (!ArchitectureRelationship.All.Contains(part))
            {
                return null;
            }

            result.Add(part);
        }

        return result;
    }

    private static bool TryReadValue(string[] args, ref int index, out string value)
    {
        value = string.Empty;
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            return false;
        }

        value = args[++index];
        return true;
    }

    private static bool TryParseFormat(string value, out OutputFormat format)
    {
        format = value.ToLowerInvariant() switch
        {
            "json" => OutputFormat.Json,
            "mermaid" => OutputFormat.Mermaid,
            "all" => OutputFormat.All,
            _ => default
        };

        return value is "json" or "mermaid" or "all";
    }

    private static bool TryParseGroupBy(string value, out GroupByMode groupBy)
    {
        groupBy = value.ToLowerInvariant() switch
        {
            "namespace" => GroupByMode.Namespace,
            "assembly" => GroupByMode.Assembly,
            _ => default
        };

        return value is "namespace" or "assembly";
    }
}
