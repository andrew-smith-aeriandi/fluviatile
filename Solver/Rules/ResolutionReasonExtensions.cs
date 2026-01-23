using System.Diagnostics;

namespace Solver.Rules;

public static class ResolutionReasonExtensions
{
    public static double GetResolutionDifficulty(this ResolutionReason reason)
    {
        return reason switch {
            ResolutionReason.Housekeeping => 0.1,
            ResolutionReason.MeanderRule => 1.0,
            ResolutionReason.TileEdgesResolution => 1.0,
            ResolutionReason.AisleCount => 1.0,
            ResolutionReason.AisleCountWithSingleChannel => 1.0,
            ResolutionReason.ThalwegChannelCount => 1.0,
            ResolutionReason.BorderAisleCount => 2.0,
            ResolutionReason.CornerTileWithSinglePotentialExit => 2.0,
            ResolutionReason.MarginAisleResolutionPattern => 2.0,
            ResolutionReason.InternalAisleChannelAdjacency => 2.0,
            ResolutionReason.BorderAisleCountWithSinglePotentialExit => 2.5,
            ResolutionReason.NoClosedLoop => 2.5,
            ResolutionReason.MeanderRuleConstrainedByAisleCounts => 3.0,
            ResolutionReason.ExitCount => 3.0,
            ResolutionReason.AisleCountIntersection => 3.0,
            ResolutionReason.SingleChannel => 3.0,
            ResolutionReason.HypotheticalMeanderRuleConstrainedByAisleCount => 4.0,
            ResolutionReason.MarginAisleResolutionPatternConstrainedByAisleCountIntersection => 4.0,
            ResolutionReason.TarjansAlgorithm => 4.0,
            ResolutionReason.Hypothetical => 5.0,
            _ => throw new UnreachableException()
        };

    }
}
