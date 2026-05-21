using SharpAtlas.Cli;
using SharpAtlas.Output;
using SharpAtlas.Reporting;
using SharpAtlas.Roslyn;

var reporter = new ConsoleReporter();

try
{
    var parseResult = CommandLineParser.Parse(args);
    if (!parseResult.Success)
    {
        reporter.Error(parseResult.ErrorMessage ?? "Invalid command line.");
        return 1;
    }

    var options = parseResult.Options!;
    if (options.ShowHelp)
    {
        Console.WriteLine(CommandLineParser.GetHelpText());
        return 0;
    }

    var sourceResult = SourceDiscovery.Resolve(options, Directory.GetCurrentDirectory());
    if (!sourceResult.Success)
    {
        reporter.Error(sourceResult.ErrorMessage ?? "Could not resolve source.");
        return 1;
    }

    options = options with { Source = sourceResult.Source };

    var scanner = new RoslynArchitectureScanner(reporter);
    var graph = await scanner.ScanAsync(options, CancellationToken.None);

    string? jsonPath = null;
    string? mermaidPath = null;
    string? htmlPath = null;
    string? mermaidViewerPath = null;

    if (OutputFormatSelection.ShouldWriteJson(options.Format))
    {
        jsonPath = ArchitectureGraphJsonWriter.Write(graph, options.OutputPath);
    }

    if (OutputFormatSelection.ShouldWriteMermaid(options.Format))
    {
        var mermaidSource = ArchitectureGraphMermaidWriter.Render(graph, options.GroupBy);
        mermaidPath = ArchitectureGraphMermaidWriter.Write(mermaidSource, options.OutputPath);

        if (OutputFormatSelection.ShouldWriteMermaidViewer(options.Format))
        {
            mermaidViewerPath = ArchitectureGraphMermaidHtmlWriter.Write(mermaidSource, options.OutputPath);
        }
    }

    if (OutputFormatSelection.ShouldWriteHtml(options.Format))
    {
        htmlPath = ArchitectureGraphHtmlWriter.Write(graph, options.OutputPath);
    }

    reporter.Summary(graph, options, jsonPath, mermaidPath, htmlPath, mermaidViewerPath);
    return 0;
}
catch (Exception ex)
{
    reporter.Error($"SharpAtlas failed: {ex.Message}");
    return 1;
}
