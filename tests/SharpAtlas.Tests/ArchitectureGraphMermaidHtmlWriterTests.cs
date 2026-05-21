using SharpAtlas.Output;

namespace SharpAtlas.Tests;

public sealed class ArchitectureGraphMermaidHtmlWriterTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "SharpAtlas.MermaidHtml.Tests", Guid.NewGuid().ToString("N"));

    public ArchitectureGraphMermaidHtmlWriterTests()
    {
        Directory.CreateDirectory(_directory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, true);
        }
    }

    [Fact]
    public void WriteCreatesMermaidViewerFile()
    {
        var path = ArchitectureGraphMermaidHtmlWriter.Write(CreateMermaidSource(), _directory);

        Assert.True(File.Exists(path));
        Assert.Equal("class-graph-mermaid.html", Path.GetFileName(path));
    }

    [Fact]
    public void RenderIncludesTitleCdnControlsAndSourceToggle()
    {
        var html = ArchitectureGraphMermaidHtmlWriter.Render(CreateMermaidSource());

        Assert.Contains("<title>SharpAtlas Mermaid Viewer</title>", html);
        Assert.Contains("<h1>SharpAtlas Mermaid Viewer</h1>", html);
        Assert.Contains("https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js", html);
        Assert.Contains("Zoom In", html);
        Assert.Contains("Zoom Out", html);
        Assert.Contains("Reset Zoom", html);
        Assert.Contains("Toggle Source", html);
        Assert.Contains("Download SVG", html);
        Assert.Contains("Mermaid failed to render this diagram.", html);
        Assert.Contains("maxTextSize: 50000000", html);
        Assert.Contains("maxEdges: 100000", html);
    }

    [Fact]
    public void RenderEmbedsMermaidSourceAsBase64()
    {
        const string source = "flowchart LR\n    A[\"</script>\"] --> B[\"Ω\"]";

        var html = ArchitectureGraphMermaidHtmlWriter.Render(source);

        Assert.DoesNotContain(source, html);
        Assert.Contains("const mermaidSourceBase64", html);
        Assert.Contains("decodeBase64Utf8", html);
        Assert.Contains(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(source)), html);
    }

    private static string CreateMermaidSource() =>
        """
        flowchart LR
            A["Program"] -->|constructor| B["Service"]
        """;
}
