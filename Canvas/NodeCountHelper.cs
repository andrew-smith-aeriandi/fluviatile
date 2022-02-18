using System.Collections.Generic;

namespace Canvas
{
    public static class NodeCountHelper
    {
        public static IEnumerable<int> MapNodeCounts(IReadOnlyList<int> nodeCounts)
        {
            yield return nodeCounts[0];
            yield return nodeCounts[1];
            yield return nodeCounts[2];
            yield return nodeCounts[11];
            yield return nodeCounts[10];
            yield return nodeCounts[9];
            yield return nodeCounts[6];
            yield return nodeCounts[7];
            yield return nodeCounts[8];
            yield return nodeCounts[17];
            yield return nodeCounts[16];
            yield return nodeCounts[15];
            yield return nodeCounts[12];
            yield return nodeCounts[13];
            yield return nodeCounts[14];
            yield return nodeCounts[5];
            yield return nodeCounts[4];
            yield return nodeCounts[3];
        }
    }
}
