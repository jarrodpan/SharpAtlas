using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SharpAtlas.Roslyn;

namespace SharpAtlas.Tests;

public sealed class TestExclusionRulesTests
{
    [Theory]
    [InlineData(@"C:\repo\Tests\Thing.cs")]
    [InlineData(@"C:\repo\App.Tests\Thing.cs")]
    [InlineData(@"C:\repo\src\ThingTests.cs")]
    public void IsTestFileMatchesCommonPatterns(string path)
    {
        Assert.True(TestExclusionRules.IsTestFile(path));
    }

    [Theory]
    [InlineData("App.Tests")]
    [InlineData("App.UnitTest")]
    public void IsTestNamespaceMatchesCommonPatterns(string namespaceName)
    {
        Assert.True(TestExclusionRules.IsTestNamespace(namespaceName));
    }

    [Fact]
    public void IsTestTypeDetectsFactAttributeOnMember()
    {
        const string source = """
            namespace Xunit { public sealed class FactAttribute : System.Attribute { } }
            namespace App.Tests
            {
                public sealed class EngineSpec
                {
                    [Xunit.Fact]
                    public void Works() { }
                }
            }
            """;

        var compilation = CSharpCompilation.Create(
            "App.Tests",
            [CSharpSyntaxTree.ParseText(source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        var type = compilation.GetTypeByMetadataName("App.Tests.EngineSpec");

        Assert.NotNull(type);
        Assert.True(TestExclusionRules.IsTestType(type!));
    }
}
