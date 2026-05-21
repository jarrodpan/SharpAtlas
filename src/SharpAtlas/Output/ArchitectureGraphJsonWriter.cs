using System.Text.Json;
using SharpAtlas.Graph;

namespace SharpAtlas.Output;

public static class ArchitectureGraphJsonWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Write(ArchitectureGraph graph, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, "class-graph.json");
        File.WriteAllText(path, JsonSerializer.Serialize(graph, JsonOptions));
        return path;
    }
}
