using Fluviatile.Grid.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.IO;

namespace Fluviatile.Grid
{
    public static class RouteStateReaderWriter
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = {
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        public static void WriteToFile(RouteLog stateToPersist, string filePath)
        {
            if (File.Exists(filePath))
            {
                var backupFilePath = Configuration.BackupFilename(filePath);
                if (File.Exists(backupFilePath))
                {
                    File.Delete(backupFilePath);
                }

                File.Move(filePath, backupFilePath);
            }

            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            JsonSerializer.Serialize<RouteLog>(stream, stateToPersist, JsonSerializerOptions);
            Trace.WriteLine($"Wrote {stateToPersist.Steps.Count} steps to file '{filePath}'");
        }

        public static RouteLog ReadFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return JsonSerializer.Deserialize<RouteLog>(stream, JsonSerializerOptions);
        }
    }
}
