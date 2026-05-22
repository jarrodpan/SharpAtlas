using SharpAtlas.Cli;
using SharpAtlas.Graph;

namespace SharpAtlas.Tests;

public sealed class CommandLineParserTests
{
    [Fact]
    public void ParseRejectsSolutionAndProjectTogether()
    {
        var result = CommandLineParser.Parse(["--solution", "a.sln", "--project", "b.csproj"]);

        Assert.False(result.Success);
        Assert.Contains("either --solution or --project", result.ErrorMessage);
    }

    [Fact]
    public void ParseUsesExpectedDefaults()
    {
        var result = CommandLineParser.Parse([]);

        Assert.True(result.Success);
        Assert.Equal(OutputFormat.All, result.Options!.Format);
        Assert.False(result.Options.IncludeTests);
        Assert.False(result.Options.IncludeExternal);
        Assert.Equal(GroupByMode.Namespace, result.Options.GroupBy);
        Assert.Equal(ArchitectureRelationship.All.Count, result.Options.Relationships.Count);
    }

    [Fact]
    public void ParseRelationshipFilterAllowsCommaSeparatedValues()
    {
        var result = CommandLineParser.Parse(["--relationship", "constructor,field"]);

        Assert.True(result.Success);
        Assert.Equal(["constructor", "field"], result.Options!.Relationships.OrderBy(value => value));
    }

    [Fact]
    public void ParseRelationshipFilterRejectsUnknownValue()
    {
        var result = CommandLineParser.Parse(["--relationship", "constructor,unknown"]);

        Assert.False(result.Success);
    }

    [Fact]
    public void ParseSupportsHtmlFormat()
    {
        var result = CommandLineParser.Parse(["--format", "html"]);

        Assert.True(result.Success);
        Assert.Equal(OutputFormat.Html, result.Options!.Format);
    }

    [Fact]
    public void HtmlFormatAlsoWritesJson()
    {
        Assert.True(OutputFormatSelection.ShouldWriteJson(OutputFormat.Html));
        Assert.True(OutputFormatSelection.ShouldWriteHtml(OutputFormat.Html));
        Assert.False(OutputFormatSelection.ShouldWriteMermaid(OutputFormat.Html));
    }

    [Fact]
    public void AllFormatIncludesHtmlOutput()
    {
        Assert.True(OutputFormatSelection.ShouldWriteJson(OutputFormat.All));
        Assert.True(OutputFormatSelection.ShouldWriteMermaid(OutputFormat.All));
        Assert.True(OutputFormatSelection.ShouldWriteHtml(OutputFormat.All));
        Assert.True(OutputFormatSelection.ShouldWriteMermaidViewer(OutputFormat.All));
    }

    [Fact]
    public void MermaidFormatWritesMermaidAndMermaidViewerOnly()
    {
        Assert.False(OutputFormatSelection.ShouldWriteJson(OutputFormat.Mermaid));
        Assert.True(OutputFormatSelection.ShouldWriteMermaid(OutputFormat.Mermaid));
        Assert.True(OutputFormatSelection.ShouldWriteMermaidViewer(OutputFormat.Mermaid));
        Assert.False(OutputFormatSelection.ShouldWriteHtml(OutputFormat.Mermaid));
    }

    [Theory]
    [InlineData("project", GroupByMode.Project)]
    [InlineData("csproj", GroupByMode.Project)]
    [InlineData("namespace-hierarchy", GroupByMode.NamespaceHierarchy)]
    [InlineData("namespace-tree", GroupByMode.NamespaceHierarchy)]
    public void ParseSupportsProjectAndNamespaceHierarchyGrouping(string value, GroupByMode expected)
    {
        var result = CommandLineParser.Parse(["--group-by", value]);

        Assert.True(result.Success);
        Assert.Equal(expected, result.Options!.GroupBy);
    }
}
