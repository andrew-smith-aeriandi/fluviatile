using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public interface IGrid
    {
        int Size { get; }

        string DisplayText { get; }

        void CreateSequence();
        void SetSequence(IEnumerable<(int x, int y)> sequence);
        IEnumerable<(int x, int y)> Sequence();

        void SetNodeCounts(IEnumerable<int> nodeCounts);
        IEnumerable<(string group, int index, float x, float y, int count, int max)> NodeCounts();

        IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines();
        IEnumerable<((int x, int y) position, IEnumerable<(float x, float y)> polygon)> GridCells();
        IEnumerable<IEnumerable<(float x, float y)>> GetMargins();
        IEnumerable<((float x, float y) from, (float x, float y) to)> MarginLines();

        void SetInitialState(IEnumerable<NodeState> state);
        IEnumerable<NodeState> GetInitialState();
    }
}
