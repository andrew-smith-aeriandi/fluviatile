using System.Collections.Generic;

namespace Canvas
{
    public interface IGrid
    {
        int Size { get; }
        void CreateSequence();
        void SetSequence(IEnumerable<(int x, int y)> sequence);
        void SetNodeCounts(IEnumerable<int> nodeCounts);
        IEnumerable<((float x, float y) from, (float x, float y) to)> GridLines();
        IEnumerable<(int x, int y)> Sequence();
        IEnumerable<(float x, float y, int count)> NodeCounts();
        IEnumerable<IEnumerable<(float x, float y)>> GetMargins();
        IEnumerable<((float x, float y) from, (float x, float y) to)> MarginLines();
        string DisplayText();
    }
}
