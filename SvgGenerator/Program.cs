using Fluviatile.Grid;
using Fluviatile.Grid.Random;
using GridWriter;
using GridWriter.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SvgGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

        var seedArgIndex = Array.IndexOf<string>(args, "-r") + 1;
        var jsonArgIndex = Array.IndexOf<string>(args, "-s") + 1;
        var pathArgIndex = Array.IndexOf<string>(args, "-f") + 1;
        var countArgIndex = Array.IndexOf<string>(args, "-c") + 1;

        var seed = seedArgIndex > 0 &&
            args.Length > seedArgIndex &&
            int.TryParse(args[seedArgIndex], out var arg)
                ? arg
                : Environment.TickCount;

        var random = new Pseudorandom(seed);

        const int size = 3;
        var shape = new Hexagon(size);
        var routeFinder = new RouteFinder(random, shape);

        var grid = new HexGrid(size, 0.3d, random);

        var pathJson = (string?)null;
        if (jsonArgIndex > 0 && args.Length > jsonArgIndex)
        {
            pathJson = args[jsonArgIndex];
        }
        else if (pathArgIndex > 0 && args.Length > pathArgIndex && File.Exists(args[pathArgIndex]))
        {
            using var textReader = File.OpenText(args[pathArgIndex]);
            pathJson = textReader.ReadToEnd();
        }

        var countJson = (string?)null;
        if (countArgIndex > 0 && args.Length > countArgIndex)
        {
            countJson = args[countArgIndex];
        }

        var nodeCounts = (IReadOnlyList<int>?)null;

        if (!string.IsNullOrEmpty(pathJson))
        {
            var path = JsonSerializer.Deserialize<List<Coordinates>>(pathJson);
            if (path is not null)
            {
                grid.SetSequence(path.Select(coord => (coord.X, coord.Y)));
            }
        }
        else if (!string.IsNullOrEmpty(countJson))
        {
            nodeCounts = JsonSerializer.Deserialize<List<int>>(countJson);
            if (nodeCounts is not null)
            {
                grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
            }
        }
        else
        {
            await routeFinder.Initiate(Configuration.NodeCountsFilename(shape));
            nodeCounts = routeFinder.SelectRandomNodeCount();

            if (nodeCounts is not null)
            {
                grid.SetNodeCounts(NodeCountHelper.MapNodeCounts(nodeCounts));
            }
            else
            {
                grid.CreateSequence();
            }
        }

        //grid.SetInitialState([new(4, -2, 1), new(4, 1, 256)]);
        var initalState = grid.GetInitialState();

        var options = new GridHtmlWriterOptions();
        var htmlWriter = new GridHtmlWriter(options);
        var outputPath = htmlWriter.Write(grid);

        Console.WriteLine($"Output written to:\n{outputPath}");
        Console.WriteLine($"Node Counts: [{string.Join(", ", NodeCountHelper.MapNodeCountsForSolver(nodeCounts))}]");
        Console.ReadLine();
    }
}
