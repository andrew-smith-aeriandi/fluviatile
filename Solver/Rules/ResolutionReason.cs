namespace Solver.Rules;

public enum ResolutionReason
{
    Unspecified = 0,
    Housekeeping,
    Hypothetical,
    AisleCount,
    AisleCountWithSingleExit,
    BorderAisleCount,
    CornerTileWithSinglePotentialExit,
    BorderAisleCountWithSinglePotentialExit,
    MeanderRule,
    ExitCount,
    TileEdgesResolution,
    ChannelTileWithResolvedEmptyEdge,
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
