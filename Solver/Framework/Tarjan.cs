namespace Solver.Framework;

public static class Tarjan
{
    // Main function to find articulation points in the graph
    public static IEnumerable<T> GetArticulationPoints<T>(
        IEnumerable<T> vertices,
        Func<T, IEnumerable<T>> adjacency)
        where T : notnull
    {
        var items = InitialiseState(vertices, adjacency);
        var time = 0;

        // Run depth-first search from each vertex if not already visited (to handle disconnected graphs)
        foreach (var item in items)
        {
            if (!item.IsVisited)
            {
                FindArticulationPoints(item, null!, ref time);
            }
        }

        // Collect all vertices that are articulation points
        return items
            .Where(s => s.IsArticulationPoint)
            .Select(s => s.Value);
    }

    private static List<VertexState<T>> InitialiseState<T>(
        IEnumerable<T> vertices,
        Func<T, IEnumerable<T>> adjacency)
        where T : notnull
    {
        var state = vertices.ToDictionary(
            v => v,
            v => new VertexState<T>(v));

        foreach (var (vertex, vertexState) in state)
        {
            foreach (var adjacentVertex in adjacency(vertex))
            {
                vertexState.Adjacency.Add(state[adjacentVertex]);
            }
        }

        return [.. state.Values];
    }

    // Helper function to perform depth-first search and find articulation
    // points using Tarjan's algorithm.
    private static void FindArticulationPoints<T>(
        VertexState<T> current,
        VertexState<T> parent,
        ref int time)
    {
        // Mark current vertex as visited
        time++;
        current.SetVisited(time);

        var children = 0;

        foreach (var other in current.Adjacency)
        {
            if (!other.IsVisited)
            {
                children++;
                FindArticulationPoints(other, current, ref time);

                // Check if subtree has a connection to an ancestor of the current vertex
                if (other.Low < current.Low)
                {
                    current.Low = other.Low;
                }

                // If current vertex is not a root and the low time is greater than or equal
                // to the discovered time, then the current vertex is an articulation point
                if (parent is not null && other.Low >= current.Discovered)
                {
                    current.SetArticulationPoint();
                }
            }
            else if (other != parent && other.Discovered < current.Low)
            {
                // Update low time of current vertex for back edge
                current.Low = other.Discovered;
            }
        }

        // If current vertex is root of the depth-first search tree and has more than
        // one child then it is an articulation point
        if (parent is null && children > 1)
        {
            current.SetArticulationPoint();
        }
    }
}
