using Fluviatile.Grid;
using GridWriter.Settings;
using System.Xml;

namespace GridWriter;

public static class GridHtmlWriterExtensions
{
    public static void WriteStyles(this XmlWriter xmlWriter, IGrid grid)
    {
        var content = EmbeddedResourceProvider.GetCssContent(grid);
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        xmlWriter.WriteStartElement("style");
        xmlWriter.WriteRaw(content);
        xmlWriter.WriteFullEndElement();
    }

    public static void WriteScript(this XmlWriter xmlWriter, IGrid grid)
    {
        var content = EmbeddedResourceProvider.GetScriptContent(grid);
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        xmlWriter.WriteStartElement("script");
        xmlWriter.WriteRaw(content);
        xmlWriter.WriteFullEndElement();
    }

    public static void WriteInitialState(this XmlWriter xmlWriter, IGrid grid)
    {
        var initialState = grid.GetInitialState();
        if (initialState is null || !initialState.Any())
        {
            return;
        }

        xmlWriter.WriteStartElement("script");
        xmlWriter.WriteRaw("\nconst initialState = {\n");
        xmlWriter.WriteRaw(string.Join(",\n", initialState.Select(item => $"   \"cell{item.X:+#;-#}{item.Y:+#;-#}\": {item.Value}")));
        xmlWriter.WriteRaw("\n};\n");
        xmlWriter.WriteFullEndElement();
    }

    public static void WriteSvg(this XmlWriter xmlWriter, IGrid grid, SvgOptions options)
    {
        var svgWriter = new SvgWriter(xmlWriter, grid.Size, options);
        svgWriter.WriteSvg(grid);
    }
}
