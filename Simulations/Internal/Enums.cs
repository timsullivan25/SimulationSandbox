namespace Simulations
{
    public enum BinomialTreeMove
    {
        Root,
        Up,
        Down,
        Converge
    }

    public enum ConfidenceLevel
    {
        _80,
        _85,
        _90,
        _95,
        _99,
        _99_5,
        _99_9
    }   

    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
    }

    public enum ConstraintViolationResolution
    {
        ClosestBound,
        DefaultValue,
        Resimulate
    }

    public enum DistributionFunctionParameterReturnType
    {
        CumulativeDistribution,
        Density,
        DensityLn,
        InverseCumulativeDistribution,
        Probability,
        ProbabilityLn
    }

    public enum OptionExecutionRules
    {
        American,
        Bermudan,
        European
    }

    public enum OptionContractType
    {
        Call,
        Put
    }    

    public enum RandomBagReplacement
    {
        AfterEachPick,
        WhenEmpty,
        Never
    }

    public enum RangeInterpolation
    {
        Linear
    }

    public enum SimulationParameterReturnType
    {
        Results,
        Minimum,
        LowerQuartile,
        Mean,
        Median,
        UpperQuartile,
        Maximum,
        Variance,
        StandardDeviation,
        Kurtosis,
        Skewness
    }

    public enum DependentSimulationParameterReturnType
    {
        Results,
        EndingValue,
        LowestValue,
        HighestValue,
        RangeSize,
        SmallestChange,
        LargestChange,
        AverageChange,
        StandardDeviationOfChanges
    }
}
