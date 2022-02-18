using System;
using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public record PathFinderJobSpec
    {
        public Tableau Tableau { get; init; }

        public string Name { get; init; }

        public TerminalNode StartPoint { get; init; }

        public List<TerminalNode> EndPoints { get; init; }

        public int ThreadCount { get; init; } = 1;

        public TimeSpan MonitorInterval { get; init; } = TimeSpan.Zero;
    }
}
