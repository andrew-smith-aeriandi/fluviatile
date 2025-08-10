namespace GridWriter.Settings;

public class GridHtmlWriterOptions
{
    public string Title { get; set; } = "Fluviatile";

    public string BaseFilePath { get; set; } = @"C:\git\Scratch\Fluviatile\Results";

    public string DefaultFilename { get; set; } = @"Test.html";

    public SvgOptions SvgOptions { get; set; } = new();
}
