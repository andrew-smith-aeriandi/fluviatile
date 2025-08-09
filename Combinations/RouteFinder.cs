using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Combinations;

public class RouteFinder<T>
{
    private readonly Queue<IRoute<INode<T>>> _workQueue;
    private readonly Tableau<T> _tableau;

    private readonly Queue<string> _tempFilePaths;
    private readonly RouteParser<T> _routeParser;

    public RouteFinder(Tableau<T> tableau, RouteParser<T> routeParser)
    {
        _tableau = tableau;
        _workQueue = new Queue<IRoute<INode<T>>>();
        _tempFilePaths = new Queue<string>();
        _routeParser = routeParser;
    }

    public long FindAllRoutes(CancellationToken token)
    {
        var totalCount = (long)0;
        var terminalNodes = _tableau.TerminalNodes.Take(1).ToList();

        foreach (var terminalNode in terminalNodes)
        {
            _tableau.TerminalNodes.Remove(terminalNode);
            var initialRoute = new Route<INode<T>>(terminalNode, null);

            var count = FindRoutes(initialRoute, 0, 0, token);
            totalCount += count;

            Trace.WriteLine($"terminal node {terminalNode}: {count}");
        }

        return totalCount * 6;
    }

    public long FindRoutes(IRoute<INode<T>> route, long count, long writtenToDisk, CancellationToken token)
    {
        const long queueLimit = 1000000L;
        var streamReader = (StreamReader)null;

        while (route != null)
        {
            foreach (var link in route.Value.Links.Except(route.AllValues))
            {
                if (link is TerminalNode)
                {
                    //Console.WriteLine(new Route<INode<T>>(link, route));
                    count++;

                    if (count % 100000 == 0)
                    {
                        Trace.WriteLine($"Route count: {count}, Queue count: {_workQueue.Count} in memory, {writtenToDisk} on disk");
                    }
                }
                else
                {
                    _workQueue.Enqueue(new Route<INode<T>>(link, route));
                }
            }

            if (_workQueue.Count >= queueLimit || token.IsCancellationRequested)
            {
                var writtenCount = (long)0;
                var tempFilePath = Path.GetTempFileName();
                Trace.WriteLine($"Writing routes to file {tempFilePath}");

                using (var sw = File.CreateText(tempFilePath))
                {
                    while (_workQueue.Count > 0)
                    {
                        var queuedRoute = _workQueue.Dequeue();
                        if (!IsBlocked(queuedRoute))
                        {
                            sw.WriteLine(queuedRoute);
                            writtenCount++;
                        }
                    }
                }

                writtenToDisk += writtenCount;
                Trace.WriteLine($"Wrote {writtenCount} routes to file {tempFilePath}, total routes written to file: {writtenToDisk}");
                _tempFilePaths.Enqueue(tempFilePath);

                if (token.IsCancellationRequested)
                {
                    if (streamReader != null)
                    {
                        tempFilePath = Path.GetTempFileName();
                        using (var sw = File.CreateText(tempFilePath))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                sw.WriteLine(streamReader.ReadLine());
                            }
                        }

                        streamReader.Close();
                        _tempFilePaths.Enqueue(tempFilePath);
                    }

                    var resultFilePath = Path.Combine(Path.GetTempPath(), "InterimResult.txt");
                    using (var sw = File.CreateText(resultFilePath))
                    {
                        sw.WriteLine(count);
                        sw.WriteLine(writtenToDisk);
                        while (_tempFilePaths.Count > 0)
                        {
                            sw.WriteLine(_tempFilePaths.Dequeue());
                        }
                    }
                }
            }

            if (_workQueue.Count > 0)
            {
                route = _workQueue.Dequeue();
                continue;
            }

            if (streamReader != null && streamReader.EndOfStream)
            {
                streamReader.Close();
                streamReader = null;
            }

            if (_tempFilePaths.Count > 0 && streamReader == null)
            {
                streamReader = File.OpenText(_tempFilePaths.Dequeue());
            }

            if (streamReader != null && !streamReader.EndOfStream)
            {
                route = _routeParser.Parse(streamReader.ReadLine());
                writtenToDisk -= 1;
            }
            else
            {
                route = null;
            }
        }

        return count;
    }

    private static bool IsBlocked(IRoute<INode<T>> route)
    {
        return IsBlocked(
            new HashSet<INode<T>> { route.Value },
            new HashSet<INode<T>>(route.AllValues));
    }

    private static bool IsBlocked(ISet<INode<T>> currentNodes, ISet<INode<T>> blockingNodes)
    {
        while (currentNodes.Any())
        {
            var linkedNodes = new HashSet<INode<T>>();

            foreach (var node in currentNodes)
            {
                foreach (var link in node.Links)
                {
                    if (!blockingNodes.Add(link))
                    {
                        continue;
                    }

                    if (link is TerminalNode)
                    {
                        return false;
                    }

                    linkedNodes.Add(link);
                }
            }

            currentNodes = linkedNodes;
        }

        return true;
    }
}
