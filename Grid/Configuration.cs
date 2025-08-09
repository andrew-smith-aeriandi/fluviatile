using System.IO;

namespace Fluviatile.Grid
{
    public static class Configuration
    {
        public static string FolderName => @"C:\Users\AndrewSmith\Documents\Personal\Fluviatile";
        public static string RoutesFilename(Shape shape, string name) => Path.Combine(FolderName, $"routes_{shape.Name}_{shape.Size}_{name}.json");
        public static string NodeCountsFilename(Shape shape) => Path.Combine(FolderName, $"nodecounts_{shape.Name}_{shape.Size}.txt");
        public static string BackupFilename(string path) => Path.ChangeExtension(path, ".backup.json");
    }
}
