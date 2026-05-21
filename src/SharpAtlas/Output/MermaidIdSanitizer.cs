using System.Text;

namespace SharpAtlas.Output;

public static class MermaidIdSanitizer
{
    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Node";
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        var result = builder.ToString().Trim('_');
        if (result.Length == 0)
        {
            result = "Node";
        }

        if (char.IsDigit(result[0]))
        {
            result = "n_" + result;
        }

        return result;
    }
}
