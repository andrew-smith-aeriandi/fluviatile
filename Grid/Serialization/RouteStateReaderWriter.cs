using Fluviatile.Grid.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Fluviatile.Grid
{
    public static class RouteStateReaderWriter
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                new StringEnumConverter()
            }
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
            using var streamWriter = new StreamWriter(stream, Encoding.UTF8);
            using var jsonWriter = new JsonTextWriter(streamWriter);

            var jsonSerializer = JsonSerializer.Create(JsonSerializerSettings);
            jsonSerializer.Serialize(jsonWriter, stateToPersist);
            Trace.WriteLine($"Wrote {stateToPersist.Steps.Count} steps to file '{filePath}'");
        }

        public static RouteLog ReadFromFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(streamReader);

            var jsonSerializer = JsonSerializer.Create(JsonSerializerSettings);
            return jsonSerializer.Deserialize<RouteLog>(jsonReader);
        }
    }
}
