using System;

namespace Fluviatile.Grid.Serialization
{
    public record Progress
    {
        public Progress()
        {
            RouteCount = 0;
            ElapsedTime = TimeSpan.Zero;
        }

        public long RouteCount { get; init; }

        public TimeSpan ElapsedTime { get; init; }
    }
}
