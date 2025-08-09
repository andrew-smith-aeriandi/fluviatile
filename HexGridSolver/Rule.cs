using Fluviatile.Grid;

namespace GridSolver
{

    public class SolutionTableau : Tableau
    {
        private readonly Dictionary<Node, bool?> _vertices;
        private readonly Dictionary<NodePair, bool?> _edges;

        public SolutionTableau(
            Shape shape,
            IEnumerable<Node> nodes,
            IEnumerable<TerminalNode> terminalNodes)
            : base(
                shape,
                nodes,
                terminalNodes) 
        {
            _vertices = new Dictionary<Node, bool?>();
            _edges = new Dictionary<NodePair, bool?>();
        }

        public IReadOnlyDictionary<Node, bool?> Vertices => _vertices;

        public IReadOnlyDictionary<NodePair, bool?> Edges => _edges;
    }

    public static class SolutionTableauExtensions
    {
        public IEnumerable<HashSet<Node>> PartionReachableNodes(this SolutionTableau solutionTableau)
        {
            var nodes = new HashSet<Node>(
                solutionTableau.Vertices
                    .Where(kv => kv.Value != false)
                    .Select(kv => kv.Key));

            var initialNode = nodes.FirstOrDefault();
            while (initialNode is not null)
            {
                nodes.Remove(initialNode);
                var partition = new HashSet<Node>
                {
                    initialNode
                };

                var queue = new Queue<Node>();
                queue.Enqueue(initialNode);

                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    foreach (var linkedNode in node.Links.Values)
                    {
                        if (linkedNode is not TerminalNode &&
                            solutionTableau.Edges[new NodePair(node, linkedNode)] != false &&
                            partition.Add(linkedNode))
                        {
                            nodes.Remove(linkedNode);   
                            queue.Enqueue(linkedNode);
                        }
                    }
                }

                yield return partition;
            }
        }
    }

    public class HexagonState
    {
        public Aisle GetAisle(Direction direction, int index)
        {

        }

        public Cell GetCell(Coordinates coordinates)
        {

        }

        public IEnumerable<Cell> GetCells()
        {

        }

        public IEnumerable<Cell> GetCells(Aisle aisle)
        {

        }




    }

    public enum CellStatus
    {
        Unresolved = 0,
        Channel = 1,
        Plain = 2
    }

    [Flags]
    public enum LinkStatus
    {
        Unresolved = 0,
        Linked = 1,
        Unlinked = 2,
        Exit = 128
    }

    public class Cell
    {
        public CellStatus Status { get; }

        public Coordinates Coordinates { get; }

        public IReadOnlyList<Cell> AdjacentCells { get; }

        public IReadOnlyList<Aisle> Aisles { get; }

        public Aisle Aisle(Axis axis) => Aisles[(int)axis];

        public Cell AdjacentCell(Axis axis) => AdjacentCells[(int)axis];

        public IReadOnlyList<LinkStatus> CellLinkStatuses { get; }

        public int LinkedCellCount =>
            CellLinkStatuses.Count(status => status.HasFlag(LinkStatus.Linked));

        public int UnlinkedCellCount =>
            CellLinkStatuses.Count(status => status.HasFlag(LinkStatus.Unlinked));
    }

    public enum Axis : byte
    {
        X = 1,
        Y = 2,
        Z = 3
    }


    public class Aisle
    {
        public Axis Axis { get; }

        public int Index { get; }

        public int CellCount { get; }

        public int ChannelCount { get; }

        public IReadOnlyList<Cell> Cells { get; }

        public int GetCellCount(CellStatus status)
        {
            return Cells.Count(cell => cell.Status == status);
        }

        public int GetExitCount()
        {
            return Cells.Count(cell => cell.HasExit);
        }
    }


    public class AdjacencyRule : IRule<HexGrid>
    {
        public string Name => nameof(AdjacencyRule);

        public RuleOutcome TryApply(HexGrid grid)
        {
            throw new NotImplementedException();
        }
    }



    public interface IRule<T> where T : IGrid
    {
        public string Name { get; }

        public RuleOutcome TryApply(T grid);
    }

    public record RuleOutcome
    {
        CheckResult Result { get; init; }

        string? RuleName { get; init; }

        string? Description { get; init; }
    }

    public enum CheckResult
    {
        NoOp = 0,
        Applied = 1
    }
}
