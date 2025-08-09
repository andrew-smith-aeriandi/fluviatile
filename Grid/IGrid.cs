using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public interface IGrid
    {
        int Size { get; }
        void CreateSequence();
        void SetSequence(IEnumerable<(int x, int y)> sequence);
        void SetNodeCounts(IEnumerable<int> nodeCounts);
        IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines();
        IEnumerable<((int x, int y) position, IEnumerable<(float x, float y)> polygon)> GridCells();
        IEnumerable<(int x, int y)> Sequence();
        IEnumerable<(string group, int index, float x, float y, int count, int max)> NodeCounts();
        IEnumerable<IEnumerable<(float x, float y)>> GetMargins();
        IEnumerable<((float x, float y) from, (float x, float y) to)> MarginLines();
        string DisplayText();
    }
}
