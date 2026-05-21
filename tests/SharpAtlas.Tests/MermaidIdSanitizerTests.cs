using SharpAtlas.Output;

namespace SharpAtlas.Tests;

public sealed class MermaidIdSanitizerTests
{
    [Theory]
    [InlineData("MyApp.Core.Repository<T>", "MyApp_Core_Repository_T")]
    [InlineData("123.Name", "n_123_Name")]
    [InlineData("!!!", "Node")]
    public void SanitizeProducesSafeMermaidId(string value, string expected)
    {
        Assert.Equal(expected, MermaidIdSanitizer.Sanitize(value));
    }
}
