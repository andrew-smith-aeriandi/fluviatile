using Fluviatile.Grid;
using GridWriter.Settings;
using System.Xml;

namespace GridWriter;

public class SvgWriter
{
    private const float Zero = 0f;
    private const float One = 1f;
    private const float Three = 3f;
    private const float OneThird = 1f / 3f;
    private const float TwoThirds = 2f / 3f;
    private const float Half = 0.5f;
    private const float Cos60 = 0.5f;
    private const float Sin60 = 0.8660254f;

    private readonly int _size;
    private readonly int _height;
    private readonly int _width;
    private readonly float _scale;
    private readonly float _xoffset;
    private readonly float _yoffset;
    private readonly float _textSize;
    private readonly (float x, float y) _origin;

    private readonly XmlWriter _xmlWriter;

    public SvgWriter(XmlWriter xmlWriter, int size, SvgOptions options)
    {
        _xmlWriter = xmlWriter;

        _size = size;
        _height = options.Height;
        _width = options.Width;
        _scale = options.Scale;

        _xoffset = _scale * (_size + 3f);
        _yoffset = _scale * (_size + 2f);
        _textSize = _scale * 0.4f;
        _origin = Transform((0f, 0f));
    }

    private (float x, float y) Transform((float x, float y) point) => (
        _xoffset + _scale * (Cos60 * point.x + point.y),
        _yoffset + _scale * (Sin60 * point.x));

    public void WritePolygon(
        IEnumerable<(float x, float y)> vertices,
        string? className = null,
        string? id = null,
        Dictionary<string, string>? metadata = null)
    {
        // <polygon points="200,10 250,190 160,210" />
        _xmlWriter.WriteStartElement("polygon");
        _xmlWriter.WriteAttributeString("points", string.Join(" ", vertices.Select(p => $"{p.x},{p.y}")));

        if (!string.IsNullOrEmpty(id))
        {
            _xmlWriter.WriteAttributeString("id", id);
        }

        if (!string.IsNullOrEmpty(className))
        {
            _xmlWriter.WriteAttributeString("class", className);
        }

        if (metadata is not null && metadata.Count > 0)
        {
            foreach (var (key, value) in metadata)
            {
                _xmlWriter.WriteAttributeString($"data-{key}", value);
            }
        }

        _xmlWriter.WriteFullEndElement();
    }

    public void WritePolyLine(
        IEnumerable<(float x, float y)> vertices,
        string? className = null,
        string? id = null,
        Dictionary<string, string>? metadata = null)
    {
        // <polyline points="20,20 40,25 60,40 80,120 120,140 200,180" />
        _xmlWriter.WriteStartElement("polyline");
        _xmlWriter.WriteAttributeString("points", string.Join(" ", vertices.Select(p => $"{p.x},{p.y}")));

        if (!string.IsNullOrEmpty(id))
        {
            _xmlWriter.WriteAttributeString("id", id);
        }

        if (!string.IsNullOrEmpty(className))
        {
            _xmlWriter.WriteAttributeString("class", className);
        }

        if (metadata is not null && metadata.Count > 0)
        {
            foreach (var (key, value) in metadata)
            {
                _xmlWriter.WriteAttributeString($"data-{key}", value);
            }
        }

        _xmlWriter.WriteFullEndElement();
    }

    public void WriteLine(
        (float x, float y) start,
        (float x, float y) end,
        string? className = null,
        string? id = null,
        Dictionary<string, string>? metadata = null)
    {
        // <line x1="0" y1="0" x2="200" y2="200" />
        _xmlWriter.WriteStartElement("line");
        _xmlWriter.WriteAttributeString("x1", $"{start.x}");
        _xmlWriter.WriteAttributeString("y1", $"{start.y}");
        _xmlWriter.WriteAttributeString("x2", $"{end.x}");
        _xmlWriter.WriteAttributeString("y2", $"{end.y}");

        if (!string.IsNullOrEmpty(id))
        {
            _xmlWriter.WriteAttributeString("id", id);
        }

        if (!string.IsNullOrEmpty(className))
        {
            _xmlWriter.WriteAttributeString("class", className);
        }

        if (metadata is not null && metadata.Count > 0)
        {
            foreach (var (key, value) in metadata)
            {
                _xmlWriter.WriteAttributeString($"data-{key}", value);
            }
        }

        _xmlWriter.WriteFullEndElement();
    }

    public void WriteText(
        string text,
        (float x, float y) start,
        (float degrees, float x, float y) rotation,
        string? className = null,
        string? id = null,
        Dictionary<string, string>? metadata = null)
    {
        // <text x="30" y="15" fill="blue" font-family="Tahoma" transform="rotate(60)">Label</text>
        _xmlWriter.WriteStartElement("text");
        _xmlWriter.WriteAttributeString("x", $"{start.x}");
        _xmlWriter.WriteAttributeString("y", $"{start.y}");
        _xmlWriter.WriteAttributeString("transform", $"rotate({rotation.degrees}, {rotation.x}, {rotation.y})");

        if (!string.IsNullOrEmpty(id))
        {
            _xmlWriter.WriteAttributeString("id", id);
        }

        if (!string.IsNullOrEmpty(className))
        {
            _xmlWriter.WriteAttributeString("class", className);
        }

        if (metadata is not null && metadata.Count > 0)
        {
            foreach (var (key, value) in metadata)
            {
                _xmlWriter.WriteAttributeString($"data-{key}", value);
            }
        }

        _xmlWriter.WriteString(text);
        _xmlWriter.WriteFullEndElement();
    }

    public void WriteStartSvg(
        IGrid grid,
        float height,
        float width)
    {
        // <svg height="500" width="500">
        _xmlWriter.WriteStartElement("svg");
        _xmlWriter.WriteAttributeString("id", "fluviatile-grid");
        _xmlWriter.WriteAttributeString("data-size", $"{grid.Size}");
        _xmlWriter.WriteAttributeString("height", $"{height}");
        _xmlWriter.WriteAttributeString("width", $"{width}");
    }

    public void WriteEndSvg()
    {
        _xmlWriter.WriteFullEndElement();
    }

    public void WriteSvg(IGrid grid)
    {
        WriteStartSvg(grid, _height, _width);

        foreach (var item in grid.GetMargins())
        {
            WritePolygon(
                vertices: item.Select(p => Transform(p)),
                className: "margin");
        }

        foreach (var (from, to) in grid.MarginLines())
        {
            WriteLine(
                start: Transform(from),
                end: Transform(to),
                className: "margin line");
        }

        foreach (var (position, polygon) in grid.GridCells())
        {
            var (x, y) = Transform((x: position.x / Three, y: position.y / Three));
            var metadata = new Dictionary<string, string>
            {
                ["u"] = position.x.ToString(),
                ["v"] = position.y.ToString(),
                ["x"] = (x - _origin.x).ToString(),
                ["y"] = (y - _origin.y).ToString()
            };

            WritePolygon(
                vertices: polygon.Select(p => Transform(p)),
                className: "cell",
                id: string.Concat("cell", position.x.ToString("+#;-#"), position.y.ToString("+#;-#")),
                metadata: metadata);
        }

        foreach (var (group, index, x, y, count, max) in grid.NodeCounts())
        {
            var text = count.ToString();
            var (xt, yt) = Transform((x, y));

            var rotation = (group) switch
            {
                "x" => 0f,
                "y" => 60f,
                "z" => -60f,
                _ => 0f
            };

            WriteText(
                text: count.ToString(),
                start: (xt - 0.3f * _textSize, yt + 0.4f * _textSize),
                rotation: (rotation, xt, yt),
                className: "node-count",
                id: $"node-count-{group}{index}",
                metadata: new Dictionary<string, string>
                {
                    ["group"] = group.ToString(),
                    ["index"] = index.ToString(),
                    ["count"] = count.ToString(),
                    ["max"] = max.ToString()
                });
        }

        _xmlWriter.WriteStartElement("defs");

        _xmlWriter.WriteStartElement("circle");
        _xmlWriter.WriteAttributeString("id", "channel-1");
        _xmlWriter.WriteAttributeString("cx", $"{_origin.x}");
        _xmlWriter.WriteAttributeString("cy", $"{_origin.y}");
        _xmlWriter.WriteAttributeString("r", "2");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement(); // circle

        var p = Transform((x: TwoThirds, y: -OneThird));
        var p0 = (x: p.x - _origin.x, y: p.y - _origin.y);

        var radius = Half * _scale;
        var p1 = Transform((x: One, y: -Half));
        var p2 = Transform((x: Half, y: Zero));
        var p3 = Transform((x: Half, y: -Half));

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-7");
        _xmlWriter.WriteAttributeString("d", $"M {p1.x - p0.x},{p1.y - p0.y} A {radius} {radius} 60 0 1 {p2.x - p0.x},{p2.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-13");
        _xmlWriter.WriteAttributeString("d", $"M {p2.x - p0.x},{p2.y - p0.y} A {radius} {radius} 60 0 1 {p3.x - p0.x},{p3.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-11");
        _xmlWriter.WriteAttributeString("d", $"M {p3.x - p0.x},{p3.y - p0.y} A {radius} {radius} 60 0 1 {p1.x - p0.x},{p1.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-3");
        _xmlWriter.WriteAttributeString("d", $"M {p1.x - p0.x},{p1.y - p0.y} L {p.x - p0.x},{p.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-5");
        _xmlWriter.WriteAttributeString("d", $"M {p2.x - p0.x},{p2.y - p0.y} L {p.x - p0.x},{p.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-9");
        _xmlWriter.WriteAttributeString("d", $"M {p3.x - p0.x},{p3.y - p0.y} L {p.x - p0.x},{p.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        var q = Transform((x: OneThird, y: OneThird));
        var q0 = (x: q.x - _origin.x, y: q.y - _origin.y);

        var q1 = Transform((x: Zero, y: Half));
        var q2 = Transform((x: Half, y: Zero));
        var q3 = Transform((x: Half, y: Half));

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-49");
        _xmlWriter.WriteAttributeString("d", $"M {q1.x - q0.x},{q1.y - q0.y} A {radius} {radius} 60 0 1 {q2.x - q0.x},{q2.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-97");
        _xmlWriter.WriteAttributeString("d", $"M {q2.x - q0.x},{q2.y - q0.y} A {radius} {radius} 60 0 1 {q3.x - q0.x},{q3.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-81");
        _xmlWriter.WriteAttributeString("d", $"M {q3.x - q0.x},{q3.y - q0.y} A {radius} {radius} 60 0 1 {q1.x - q0.x},{q1.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-17");
        _xmlWriter.WriteAttributeString("d", $"M {q1.x - q0.x},{q1.y - q0.y} L {q.x - q0.x},{q.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-33");
        _xmlWriter.WriteAttributeString("d", $"M {q2.x - q0.x},{q2.y - q0.y} L {q.x - q0.x},{q.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-65");
        _xmlWriter.WriteAttributeString("d", $"M {q3.x - q0.x},{q3.y - q0.y} L {q.x - q0.x},{q.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteFullEndElement(); // defs

        var displayText = $"Counts: {grid.DisplayText}";
        var displayTextMetadata = new Dictionary<string, string>
        {
            ["copy-text"] = grid.DisplayText
        };

        WriteText(displayText, (5f * _textSize, 2f * _textSize), (0f, 0f, 0f), className: "title", id: "display-text", metadata: displayTextMetadata);
        WriteText("Save", (5f * _textSize, 4f * _textSize), (0f, 0f, 0f), className: "button", id: "save-state");
        WriteText("Revert", (5f * _textSize, 5f * _textSize), (0f, 0f, 0f), className: "button", id: "revert-state");

        WriteEndSvg();
    }
}
