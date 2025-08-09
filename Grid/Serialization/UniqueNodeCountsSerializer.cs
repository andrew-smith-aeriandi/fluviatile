using System;
using System.Collections.Generic;
using System.IO;

namespace Fluviatile.Grid.Serialization
{
    public static class UniqueNodeCountsSerializer
    {
        public static IReadOnlyList<byte[]> Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var nodeCounts = new List<byte[]>();
            using var reader = File.OpenText(filePath);

            string line;
            while ((line = reader.ReadLine()) is not null)
            {
                var base64Data = line.Trim();
                if (base64Data.Length > 0)
                {
                    nodeCounts.Add(Convert.FromBase64String(base64Data));
                }
            }

            return nodeCounts;
        }

        public static void Save(string filePath, IReadOnlyList<byte[]> nodeCounts)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using var writer = File.CreateText(filePath);

            foreach (var item in nodeCounts)
            {
                writer.WriteLine(Convert.ToBase64String(item));
            }
        }
    }
}
