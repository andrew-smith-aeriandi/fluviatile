using System.IO;

namespace Fluviatile.Grid
{
    public static class Configuration
    {
        public static string FolderName => @"C:\Users\AndrewSmith\Documents\Personal\Fluviatile";
        public static string Filename(Shape shape, int id) => Path.Combine(FolderName, $"routes_{shape.Name}_{shape.Size}_{id}.json");
        public static string BackupFilename(string path) => Path.ChangeExtension(path, ".backup.json");
    }
}
