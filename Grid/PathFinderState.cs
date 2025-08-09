using Fluviatile.Grid.Serialization;
using System.Collections.Generic;

namespace Fluviatile.Grid
{
    public record PathFinderState
    {
        public string Name { get; init; }

        public IList<Step> Steps { get; init; }

        public Progress Progress { get; init; }
    }
}
