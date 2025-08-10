using Fluviatile.Grid;
using System.Reflection;

namespace GridWriter;

public static class EmbeddedResourceProvider
{
    public static string GetScriptContent(IGrid grid, string filename = "main.js")
    {
        if (!Path.HasExtension(filename))
        {
            filename = Path.ChangeExtension(filename, "js");
        }

        return GetContent(grid, filename);
    }

    public static string GetCssContent(IGrid grid, string filename = "styles.css")
    {
        if (!Path.HasExtension(filename))
        {
            filename = Path.ChangeExtension(filename, "css");
        }

        return GetContent(grid, filename);
    }

    public static string GetContent(IGrid grid, string filename)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentException.ThrowIfNullOrEmpty(filename);

        var gridType = grid.GetType();
        var resourceName = $"GridWriter.{gridType.Name}.{filename}";

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
