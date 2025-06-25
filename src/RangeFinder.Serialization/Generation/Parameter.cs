namespace RangeFinder.Serialization.Generation;

/// <summary>
/// Range generation parameters that clearly express intent
/// and automatically ensure physical constraints are satisfied.
/// </summary>
public record Parameter
{
    /// <summary>
    /// Number of ranges to generate
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// Average space size per range when the total space is evenly divided by element count.
    /// This determines the total space: TotalSpace = Count × SpacePerRange
    /// Larger values = wider space per range = lower density
    /// Example: 10.0 means each range can use 10 units when space is evenly divided
    /// </summary>
    public double SpacePerRange { get; init; }

    /// <summary>
    /// Average ratio of range length to SpacePerRange
    /// 0.5 = ranges are 50% the length of allocated space
    /// 1.0 = ranges exactly fill the allocated space
    /// 2.0 = ranges are twice the length of allocated space (overlap required)
    /// </summary>
    public double LengthRatio { get; init; }

    /// <summary>
    /// Range length variability coefficient (standard deviation/average)
    /// 0.0 = all same length, 1.0 = high variability
    /// </summary>
    public double LengthVariability { get; init; }

    /// <summary>
    /// Target overlap depth: expected number of overlapping ranges at any point in space
    /// 1.0 = only one range (no overlap), 2.0 = average of two ranges overlap
    /// Actual overlap count varies probabilistically
    /// </summary>
    public double OverlapFactor { get; init; }

    /// <summary>
    /// Clustering tendency: 0.0 = uniform distribution, 1.0 = highly clustered
    /// </summary>
    public double ClusteringFactor { get; init; }

    /// <summary>
    /// Start offset within the total space (0.0 to 1.0)
    /// </summary>
    public double StartOffset { get; init; } = 0.0;

    /// <summary>
    /// Seed value for reproducible results
    /// </summary>
    public int RandomSeed { get; init; } = 42;

    // Calculated properties
    public double TotalSpace => Count * SpacePerRange;
    public double AverageLength => SpacePerRange * LengthRatio;

    /// <summary>
    /// Validates that parameters can generate a valid range dataset
    /// </summary>
    public void Validate()
    {
        // Check for NaN and Infinity values first
        if (double.IsNaN(SpacePerRange) || double.IsInfinity(SpacePerRange))
        {
            throw new ArgumentException("SpacePerRange cannot be NaN or Infinity");
        }

        if (double.IsNaN(LengthRatio) || double.IsInfinity(LengthRatio))
        {
            throw new ArgumentException("LengthRatio cannot be NaN or Infinity");
        }

        if (double.IsNaN(LengthVariability) || double.IsInfinity(LengthVariability))
        {
            throw new ArgumentException("LengthVariability cannot be NaN or Infinity");
        }

        if (double.IsNaN(OverlapFactor) || double.IsInfinity(OverlapFactor))
        {
            throw new ArgumentException("OverlapFactor cannot be NaN or Infinity");
        }

        if (double.IsNaN(ClusteringFactor) || double.IsInfinity(ClusteringFactor))
        {
            throw new ArgumentException("ClusteringFactor cannot be NaN or Infinity");
        }

        if (double.IsNaN(StartOffset) || double.IsInfinity(StartOffset))
        {
            throw new ArgumentException("StartOffset cannot be NaN or Infinity");
        }

        if (TotalSpace <= 0)
        {
            throw new ArgumentException("Calculated TotalSpace must be a positive value");
        }

        if (Count <= 0)
        {
            throw new ArgumentException("Count must be a positive value");
        }

        if (SpacePerRange <= 0)
        {
            throw new ArgumentException("SpacePerRange must be a positive value");
        }

        if (LengthRatio <= 0 || LengthRatio > 5.0)
        {
            throw new ArgumentException("LengthRatio must be between 0 and 5.0");
        }

        if (LengthVariability < 0 || LengthVariability > 2.0)
        {
            throw new ArgumentException("LengthVariability must be between 0 and 2.0");
        }

        if (OverlapFactor <= 0 || OverlapFactor > 100.0)
        {
            throw new ArgumentException("OverlapFactor must be between 0 and 100.0");
        }

        if (ClusteringFactor < 0 || ClusteringFactor > 2.0)
        {
            throw new ArgumentException("ClusteringFactor must be between 0 and 2.0");
        }

        if (StartOffset < 0 || StartOffset > 1.0)
        {
            throw new ArgumentException("StartOffset must be between 0 and 1.0");
        }

        // Check if configuration is physically feasible
        var minSpaceNeeded = Count * AverageLength * (1.0 / Math.Max(OverlapFactor, 1.0));
        if (minSpaceNeeded > TotalSpace * 1.1) // 10% tolerance
        {
            throw new InvalidOperationException(
                $"Configuration requires more space than available. " +
                $"Required: ~{minSpaceNeeded:F1}, Available: {TotalSpace:F1}. " +
                $"Reduce Count or LengthRatio, or increase OverlapFactor.");
        }
    }
}

/// <summary>
/// Convenient factory methods for common range generation scenarios
/// </summary>
public static class RangeParameterFactory
{
    /// <summary>
    /// Dense overlapping ranges - high overlap for stress testing
    /// </summary>
    public static Parameter DenseOverlapping(int count) => new()
    {
        Count = count,
        SpacePerRange = 2.0, // Each range gets 2 units when space is evenly divided
        LengthRatio = 1.5, // Ranges are 150% of allocated space (overlap required)
        LengthVariability = 0.5, // Moderate variability
        OverlapFactor = 3.0, // High overlap
        ClusteringFactor = 0.3 // Light clustering
    };

    /// <summary>
    /// Sparse non-overlapping ranges - minimal overlap for best-case performance
    /// </summary>
    public static Parameter SparseNonOverlapping(int count) => new()
    {
        Count = count,
        SpacePerRange = 10.0, // Each range gets 10 units when space is evenly divided
        LengthRatio = 0.5, // Ranges are 50% of allocated space
        LengthVariability = 0.3, // Low variability
        OverlapFactor = 0.1, // Minimal overlap
        ClusteringFactor = 0.1 // Uniform distribution
    };


    /// <summary>
    /// Clustered ranges - groups of overlapping ranges with gaps
    /// </summary>
    public static Parameter Clustered(int count) => new()
    {
        Count = count,
        SpacePerRange = 5.0, // Each range gets 5 units when space is evenly divided
        LengthRatio = 0.4, // Ranges are 40% of allocated space
        LengthVariability = 0.7, // High variability within clusters
        OverlapFactor = 2.0, // Overlap within clusters
        ClusteringFactor = 1.2 // Strong clustering
    };

    /// <summary>
    /// Uniform distribution - balanced characteristics for baseline testing
    /// </summary>
    public static Parameter Uniform(int count) => new()
    {
        Count = count,
        SpacePerRange = 4.0, // Each range gets 4 units when space is evenly divided
        LengthRatio = 0.4, // Ranges are 40% of allocated space
        LengthVariability = 0.4,
        OverlapFactor = 1.5,
        ClusteringFactor = 0.2 // Nearly uniform
    };

    /// <summary>
    /// Creates a parameter set with custom space allocation and overlap characteristics
    /// </summary>
    public static Parameter Custom(
        int count,
        double spacePerRange,
        double lengthRatio,
        double overlapFactor,
        double lengthVariability = 0.4,
        double clusteringFactor = 0.3) => new()
        {
            Count = count,
            SpacePerRange = spacePerRange,
            LengthRatio = lengthRatio,
            LengthVariability = lengthVariability,
            OverlapFactor = overlapFactor,
            ClusteringFactor = clusteringFactor
        };
}
