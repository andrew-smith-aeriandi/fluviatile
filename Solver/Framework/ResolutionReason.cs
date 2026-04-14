namespace Solver.Framework;

public enum ResolutionReason
{
    Unspecified = 0,
    Housekeeping,
    Hypothetical,
    AisleCount,
    AisleCountWithSingleChannel,
    BorderAisleCount,
    CornerTileWithSinglePotentialExit,
    BorderAisleCountWithSinglePotentialExit,
    MeanderRule,
    ExitCount,
    TileEdgesResolution,
    MarginAisleResolutionPattern,
    InternalAisleChannelAdjacency,
    MarginAisleResolutionPatternConstrainedByAisleCountIntersection,
    MeanderRuleConstrainedByAisleCounts,
    HypotheticalMeanderRuleConstrainedByAisleCount,
    AisleCountIntersection,
    SingleChannel,
    NoClosedLoop,
    ThalwegChannelCount,
    TarjansAlgorithm
}
