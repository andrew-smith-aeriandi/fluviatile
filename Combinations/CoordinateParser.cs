using System;
using System.Text.RegularExpressions;

namespace Combinations;

public partial class CoordinateParser : IValueParser<Coordinate>
{
    private static readonly Regex CoordinatePattern = CoordinateRegex();

    public Coordinate Parse(string text)
    {
        var match = CoordinatePattern.Match(text);
        if (!match.Success)
        {
            throw new Exception($"Unable to parse '{text}' as a {nameof(Coordinate)} value");
        }

        return new Coordinate(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value));
    }

    [GeneratedRegex(@"\(\s*([+-]?[0-9]+)\s*,\s*([+-]?[0-9]+)\s*\)")]
    private static partial Regex CoordinateRegex();
}