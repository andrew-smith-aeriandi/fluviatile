using Fluviatile.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SvgGenerator;

public class SvgWriter
{
    private readonly static int Height = 800;
    private readonly static int Width = 1000;

    private readonly static float Scale = 80f;
    private readonly static float XOffset = 4f;
    private readonly static float YOffset = 2f;
    private readonly static float XFactor = 0.5f;
    private readonly static float YFactor = 0.5f * (float)Math.Sqrt(3);
    private readonly static float TextSize = 0.4f * Scale;
    private readonly static (float x, float y) Origin = Transform((0f, 0f));

    private static (float x, float y) Transform((float x, float y) point) =>
        (Scale * (XOffset + point.x - XFactor * point.y), Scale * (YOffset + point.y * YFactor));

    private readonly XmlWriter _xmlWriter;

    public SvgWriter(XmlWriter xmlWriter)
    {
        _xmlWriter = xmlWriter;
    }

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
            foreach(var (key, value) in metadata)
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
        float height,
        float width)
    {
        // <svg height="500" width="500">
        _xmlWriter.WriteStartElement("svg");
        _xmlWriter.WriteAttributeString("id", "fluviatile-grid");
        _xmlWriter.WriteAttributeString("height", $"{height}");
        _xmlWriter.WriteAttributeString("width", $"{width}");
    }

    public void WriteEndSvg()
    {
        _xmlWriter.WriteFullEndElement();
    }

    public void WriteSvg(IGrid grid)
    {
        WriteStartSvg(Height, Width);

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
            var (x, y) = Transform((x: position.x / 3f, y: position.y / 3f));
            var metadata = new Dictionary<string, string>
            {
                ["u"] = position.x.ToString(),
                ["v"] = position.y.ToString(),
                ["x"] = (x - Origin.x).ToString(),
                ["y"] = (y - Origin.y).ToString()
            };

            WritePolygon(
                vertices: polygon.Select(p => Transform(p)),
                className: "cell",
                id: $"cell-{position.x}-{position.y}",
                metadata: metadata);
        }

        foreach (var (group, index, x, y, count, max) in grid.NodeCounts())
        {
            var text = count.ToString();
            var (xt, yt) = Transform((x, y));

            var rotation = (group) switch
            {
                "x" => -60f,
                "y" => 0f,
                "z" => 60f,
                _ => 0f
            };

            WriteText(
                text: count.ToString(),
                start: (xt - 0.3f * TextSize, yt + 0.4f * TextSize),
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
        _xmlWriter.WriteAttributeString("cx", $"{Origin.x}");
        _xmlWriter.WriteAttributeString("cy", $"{Origin.y}");
        _xmlWriter.WriteAttributeString("r", "2");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement(); // circle

        var p = Transform((x: 1f / 3f, y: 2f / 3f));
        var p0 = (x: p.x - Origin.x, y: p.y - Origin.y);

        var radius = 0.5f * Scale;
        var p1 = Transform((x: 0f, y: 0.5f));
        var p2 = Transform((x: 0.5f, y: 0.5f));
        var p3 = Transform((x: 0.5f, y: 1f));

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-7");
        _xmlWriter.WriteAttributeString("d", $"M {p1.x - p0.x},{p1.y - p0.y} A {radius} {radius} 60 0 0 {p2.x - p0.x},{p2.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-13");
        _xmlWriter.WriteAttributeString("d", $"M {p2.x - p0.x},{p2.y - p0.y} A {radius} {radius} 60 0 0 {p3.x - p0.x},{p3.y - p0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-11");
        _xmlWriter.WriteAttributeString("d", $"M {p3.x - p0.x},{p3.y - p0.y} A {radius} {radius} 60 0 0 {p1.x - p0.x},{p1.y - p0.y}");
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

        var q = Transform((x: 2f / 3f, y: 1f / 3f));
        var q0 = (x: q.x - Origin.x, y: q.y - Origin.y);

        var q1 = Transform((x: 1f, y: 0.5f));
        var q2 = Transform((x: 0.5f, y: 0.5f));
        var q3 = Transform((x: 0.5f, y: 0f));

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-49");
        _xmlWriter.WriteAttributeString("d", $"M {q1.x - q0.x},{q1.y - q0.y} A {radius} {radius} 60 0 0 {q2.x - q0.x},{q2.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-97");
        _xmlWriter.WriteAttributeString("d", $"M {q2.x - q0.x},{q2.y - q0.y} A {radius} {radius} 60 0 0 {q3.x - q0.x},{q3.y - q0.y}");
        _xmlWriter.WriteAttributeString("class", "river");
        _xmlWriter.WriteFullEndElement();

        _xmlWriter.WriteStartElement("path");
        _xmlWriter.WriteAttributeString("id", "channel-81");
        _xmlWriter.WriteAttributeString("d", $"M {q3.x - q0.x},{q3.y - q0.y} A {radius} {radius} 60 0 0 {q1.x - q0.x},{q1.y - q0.y}");
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

        WriteText("Save", Transform((-2.5f, -0.5f)), (0f, 0f, 0f), className: "button", id: "save-state");
        WriteText("Revert", Transform((-2.5f, 0f)), (0f, 0f, 0f), className: "button", id: "revert-state");

        WriteEndSvg();
    }
}
