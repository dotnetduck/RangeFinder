namespace RangeFinder.IO.Generation;

/// <summary>
/// Enumeration of available dataset characteristics for parameterized benchmarking.
/// </summary>
public enum Characteristic
{
    /// <summary>
    /// Dense overlapping ranges - high overlap probability for stress testing.
    /// Characteristics: ~85% overlap rate, worst-case performance scenarios.
    /// </summary>
    DenseOverlapping,

    /// <summary>
    /// Sparse non-overlapping ranges - minimal overlap for best-case performance.
    /// Characteristics: 0% overlap rate, optimal performance validation.
    /// </summary>
    SparseNonOverlapping,


    /// <summary>
    /// Clustered ranges - groups of overlapping ranges with gaps between clusters.
    /// Characteristics: ~3% overlap rate, real-world clustered data scenarios.
    /// </summary>
    Clustered,

    /// <summary>
    /// Uniform distribution - balanced characteristics for baseline testing.
    /// Characteristics: ~11% overlap rate, general-purpose performance validation.
    /// </summary>
    Uniform
}
