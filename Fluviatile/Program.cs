using System;
using System.Threading;
using System.Windows.Forms;

//using Combinations;

namespace WindowsFormsApp2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var seed = (int?)null;
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int arg))
                {
                    seed = arg;
                }
            }
            Application.Run(new Form1(seed));

            /*
            var builder = new TableauBuilder(1);
            var tableau = builder.Build();
            var pathFinder = new PathFinder(tableau);

            var startNode = tableau.TerminalNodes[0];
            var endNodes = new[] { 1, 2 }
                .Select(index => tableau.TerminalNodes[index]);

            pathFinder.Explore(startNode, endNodes, tokenSource.Token)
                .Subscribe(step => Interlocked.Increment(ref count));

            startNode = tableau.TerminalNodes[0];
            endNodes = new[] { 3 }
                .Select(index => tableau.TerminalNodes[index]);

            pathFinder.Explore(startNode, endNodes, tokenSource.Token)
                .Subscribe(step => Interlocked.Increment(ref count));
            */

            /*
            var builder = new TableauBuilder(2);
            var tableau = builder.Build();
            var pathFinder = new PathFinder(tableau);

            var waitHandle = new ManualResetEvent(false);
            var pathSource = pathFinder.GetObservable();

            pathSource
                .Buffer(TimeSpan.FromSeconds(10))
                .Subscribe(
                    steps => Trace.WriteLine($"Paths={Interlocked.Add(ref count, steps.Count)}"),
                    () => waitHandle.Set(),
                    tokenSource.Token);

            var startNode = tableau.TerminalNodes[0];
            var endNodes = new[] { 1, 2, 3, 4, 5, 6, 7, 9, 11 }
                .Select(index => tableau.TerminalNodes[index]);

            pathFinder.Explore(startNode, endNodes, tokenSource.Token);

            waitHandle.WaitOne();
            tokenSource.Cancel();
            */

            /*
            var tableau = new HexagonTableauBuilder(3).Build();
            var pathFinder = new PathFinder(tableau, 1);

            var pathSource = pathFinder.GetObservable();
            pathSource
                .Buffer(TimeSpan.FromSeconds(1))
                .Subscribe(
                    steps => Trace.WriteLine($"Paths={Interlocked.Add(ref count, steps.Count)}"),
                    tokenSource.Token);

            var startNode = tableau.TerminalNodes[0];
            var endNodes = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 14, 16, 17 }
                .Select(index => tableau.TerminalNodes[index]);

            await pathFinder.Explore(startNode, endNodes, TimeSpan.FromSeconds(10), tokenSource.Token);

            Trace.WriteLine($"Final Paths={count}");
            */

            //var tableau = new HexagonTableauBuilder(4).Build();
            //var pathFinder = new PathFinder(tableau, 4);

            /*
            var pathSource = pathFinder.GetObservable();
            pathSource
                .Buffer(TimeSpan.FromSeconds(1))
                .Subscribe(
                    steps => Trace.WriteLine($"Paths={Interlocked.Add(ref count, steps.Count)}"),
                    tokenSource.Token);
            */

            //var startNode = tableau.TerminalNodes[0];
            //var endNodes = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 17, 18, 19, 21, 22, 23}
            //    .Select(index => tableau.TerminalNodes[index]);

            //await pathFinder.Explore(startNode, endNodes, TimeSpan.FromSeconds(10), tokenSource.Token);

            //Trace.WriteLine($"Final Paths={count}");

            /*
            var startNode = tableau.TerminalNodes[1];
            var endNodes = new[] { 2, 5, 6, 9, 10, 13, 14, 18, 22 }
                .Select(index => tableau.TerminalNodes[index]);

            pathFinder.Explore(startNode, endNodes, tokenSource.Token);
            waitHandle.WaitOne();

            Console.ReadLine();
            */

            //var factory = new HexGridFactory();
            //var tableau = factory.Create(4);

            //var routeParser = new RouteParser<Coordinate>(
            //    new CoordinateParser(),
            //    tableau);

            //var routeFinder = new RouteFinder<Coordinate>(tableau, routeParser);

            //var totalCount = routeFinder.FindAllRoutes(tokenSource.Token);

            //Trace.WriteLine($"total count: {totalCount}");
        }
    }
}
