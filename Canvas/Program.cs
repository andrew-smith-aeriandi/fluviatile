using Fluviatile.Grid;
using Fluviatile.Grid.Random;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Canvas
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        async static Task Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var seedArgIndex = Array.IndexOf<string>(args, "-r") + 1;
            var jsonArgIndex = Array.IndexOf<string>(args, "-s") + 1;
            var pathArgIndex = Array.IndexOf<string>(args, "-f") + 1;
            var countArgIndex = Array.IndexOf<string>(args, "-c") + 1;

            var seed = seedArgIndex > 0 && args.Length > seedArgIndex && int.TryParse(args[seedArgIndex], out var arg)
                ? arg
                : Environment.TickCount;

            var random = new Pseudorandom(seed);

            const int size = 3;
            var shape = new Hexagon(size);
            var routeFinder = new RouteFinder(random, shape);

            var grid = new HexGrid(size, 0.3d, random);
            // var grid = new Grid(8, 0.3d, random);

            var pathJson = (string)null;
            if (jsonArgIndex > 0 && args.Length > jsonArgIndex)
            {
                pathJson = args[jsonArgIndex];
            }
            else if (pathArgIndex > 0 && args.Length > pathArgIndex && File.Exists(args[pathArgIndex]))
            {
                using var textReader = File.OpenText(args[pathArgIndex]);
                pathJson = textReader.ReadToEnd();
            }

            var countJson = (string)null;
            if (countArgIndex > 0 && args.Length > countArgIndex)
            {
                countJson = args[countArgIndex];
            }

            if (!string.IsNullOrEmpty(pathJson))
            {
                var path = JsonSerializer.Deserialize<List<Coordinates>>(pathJson);
                grid.SetSequence(path.Select(coord => (coord.X, coord.Y)));
            }
            else if (!string.IsNullOrEmpty(countJson))
            {
                var nodeCounts = JsonSerializer.Deserialize<List<int>>(countJson);
                grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
            }
            else
            {
                await routeFinder.Initiate(Configuration.NodeCountsFilename(shape));

                var nodeCounts = routeFinder.SelectRandomNodeCount();
                grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
            }

            Application.Run(new Canvas(grid, routeFinder));
        }
    }
}
