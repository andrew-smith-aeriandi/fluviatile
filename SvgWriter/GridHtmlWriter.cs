using Fluviatile.Grid;
using GridWriter.Settings;
using System.Text;
using System.Xml;

namespace GridWriter;

public class GridHtmlWriter
{
    private readonly static XmlWriterSettings XmlWriterSettings = new()
    {
        Indent = true,
        IndentChars = "  ",
        OmitXmlDeclaration = true
    };

    private readonly GridHtmlWriterOptions _options;

    public GridHtmlWriter(GridHtmlWriterOptions options)
    {
        _options = options;
    }

    public string Write(IGrid grid, string? filename = null)
    {
        var outputFilename = string.IsNullOrEmpty(filename) ? _options.DefaultFilename : filename;
        var outputPath = Path.Combine(_options.BaseFilePath, outputFilename);

        using var fileStream = new StreamWriter(outputPath, false, Encoding.UTF8);
        using var xmlWriter = XmlWriter.Create(fileStream, XmlWriterSettings);

        xmlWriter.WriteDocType("html", null, null, null);
        xmlWriter.WriteStartElement("html");
        xmlWriter.WriteAttributeString("lang", "en");
        xmlWriter.WriteStartElement("head");
        xmlWriter.WriteStartElement("meta");
        xmlWriter.WriteAttributeString("charset", "utf-8");
        xmlWriter.WriteFullEndElement(); // meta
        xmlWriter.WriteStartElement("meta");
        xmlWriter.WriteAttributeString("name", "viewport");
        xmlWriter.WriteAttributeString("content", "width=device-width, initial-scale=1");
        xmlWriter.WriteFullEndElement(); // meta
        xmlWriter.WriteElementString("title", _options.Title);
        xmlWriter.WriteStyles(grid);
        xmlWriter.WriteScript(grid);
        xmlWriter.WriteFullEndElement(); // head
        xmlWriter.WriteStartElement("body");
        xmlWriter.WriteInitialState(grid);
        xmlWriter.WriteSvg(grid, _options.SvgOptions);
        xmlWriter.WriteFullEndElement(); // body
        xmlWriter.WriteFullEndElement(); // html

        return outputPath;
    }
}
