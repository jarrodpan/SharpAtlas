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
}
