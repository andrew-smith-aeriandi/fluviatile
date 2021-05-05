using System;

namespace Fluviatile.Grid
{
    public class Progress
    {
        public Progress(
            long routeCount,
            TimeSpan elapsedTime)
        {
            RouteCount = routeCount;
            ElapsedTime = elapsedTime;
        }

        public long RouteCount { get; }
        public TimeSpan ElapsedTime { get; }
    }
}
