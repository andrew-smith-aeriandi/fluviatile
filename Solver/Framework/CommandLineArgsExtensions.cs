namespace Solver.Framework;

public static class CommandLineArgsExtensions
{
    public static bool GetFlag(
        this string[] args,
        string tag)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrEmpty(tag);

        return Array.FindIndex(args, s => s == tag) >= 0;
    }

    public static string GetString(
        this string[] args,
        string tag,
        string defaultValue = "")
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrEmpty(tag);

        var index = Array.FindIndex(args, s => s == tag) + 1;
        return index > 0 && index < args.Length ? args[index] : defaultValue;
    }

    public static int GetInteger(
        this string[] args,
        string tag,
        int defaultValue = default)
    {
        var stringValue = args.GetString(tag);

        return stringValue is not null &&
            int.TryParse(stringValue, out var value)
                ? value
                : defaultValue;
    }

    public static TEnum GetEnum<TEnum>(
        this string[] args,
        string tag,
        TEnum defaultValue = default)
        where TEnum : struct
    {
        var stringValue = args.GetString(tag);

        return stringValue is not null &&
            Enum.TryParse<TEnum>(stringValue, true, out var value)
                ? value
                : defaultValue;
    }
}
