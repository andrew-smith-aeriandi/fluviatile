using System.Collections.Generic;

namespace Fluviatile.Grid.Serialization
{
    public class RouteLog
    {
        public string Shape { get; set; }
        public int Size { get; set; }
        public Progress Progress { get; set; }
        public List<int> TerminalNodes { get; set; }
        public List<Footprint> Steps { get; set; }
    }
}
