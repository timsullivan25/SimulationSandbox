using MathNet.Numerics.Distributions;
using MathNet.Symbolics;
using Simulations.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simulations
{
    #region Standard Simulation

    public class Simulation
    {
        public IParameter[] Parameters { get; private set; }
        public Expression Expression { get; set; }

        /// <summary>
        /// A simulation that will compute an expression using the given/calculated parameter values each time a simulation is computed.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <param name="parameters">The parameters that contain/will generate values each time the simulation is run so that the expression can be computed to derive a single numerical result for the simulation.</param>
        internal Simulation(Expression expression, params IParameter[] parameters)
        {
            this.Parameters = parameters;
            this.Expression = expression;
        } // public constructor forces them to reference MathNet.Symbolics to call either constructor
        /// <summary>
        /// A simulation that will compute an expression using the given/calculated parameter values each time a simulation is computed.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <param name="parameters">The parameters that contain/will generate values each time the simulation is run so that the expression can be computed to derive a single numerical result for the simulation.</param>
        public Simulation(string expression, params IParameter[] parameters)
        {
            this.Parameters = parameters;
            this.Expression = Infix.ParseOrThrow(expression);
        }

        /// <summary>
        /// Performs the given number of simulations using the provided parameters and expressions. Results will be an array of doubles with one number per simulation.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>SimulationResults containing an array of doubles and summary statistics.</returns>
        public SimulationResults Simulate(int numberOfSimulations)
        {
            #region PrecomputedParameter Count Validation

            var precomputedParameters = from param in this.Parameters
                                        where param is PrecomputedParameter
                                        select param;

            foreach (PrecomputedParameter param in precomputedParameters)
                if (param.PrecomputedValues.Length != numberOfSimulations)
                    throw new PrecomputedValueCountException($"{param.Name} has {param.PrecomputedValues.Length} precomputed values but the simulation being run expects {numberOfSimulations} values.");

            #endregion

            #region RandomBagParameter Not Empty Validation

            var randomBagParameters = from param in this.Parameters
                                      where param is RandomBagParameter
                                      select param;

            foreach (RandomBagParameter param in randomBagParameters)
                if (param.IsEmpty)
                    throw new EmptyBagException($"RandomBag parameters must contain at least one object before they can be used in a simulation.");

            #endregion

            // generate a matrix where R:C corresponds to SimulationNumber:ParameterValue
            object[] rawData = new object[Parameters.Length];

            for (int p = 0; p < Parameters.Length; p++)
            {
                #region Conditional Parameter

                if (Parameters[p] is ConditionalParameter)
                {
                    Simulation innerSimulation = (Parameters[p] as ConditionalParameter).ReferenceParameter.Simulation;
                    double[] innerResults = innerSimulation.Simulate(numberOfSimulations).Results;
                    // we're saying an integer simulation will be converted to double.. we could just use the math.net floating point numbers to over issues here ??
                    // but the conversion doesn't seem to result in floating point comparison issues.. so we might be good?

                    ConditionalOutcome[] conditionalOutcomes = (Parameters[p] as ConditionalParameter).ConditionalOutcomes;
                    double[] conditionalResults = new double[numberOfSimulations];

                    for (int s = 0; s < innerResults.Length; s++)
                    {
                        bool valueAssigned = false;

                        for (int c = 0; c < conditionalOutcomes.Length; c++)
                        {
                            switch (conditionalOutcomes[c].ComparisonOperator)
                            {
                                case ComparisonOperator.Equal:
                                    if (innerResults[s] == conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.NotEqual:
                                    if (innerResults[s] != conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.LessThan:
                                    if (innerResults[s] < conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.LessThanOrEqual:
                                    if (innerResults[s] <= conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.GreaterThan:
                                    if (innerResults[s] > conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.GreaterThanOrEqual:
                                    if (innerResults[s] >= conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (valueAssigned)
                                break;
                        }

                        if (!valueAssigned)
                            conditionalResults[s] = (Parameters[p] as ConditionalParameter).DefaultValue;
                    }

                    rawData[p] = conditionalResults;
                }

                #endregion

                #region Constant Parameter

                else if (Parameters[p] is ConstantParameter)
                {
                    double constant = (Parameters[p] as ConstantParameter).Value;
                    double[] repeatValues = new double[numberOfSimulations];

                    for (int i = 0; i < numberOfSimulations; i++)
                        repeatValues[i] = constant;

                    rawData[p] = repeatValues;
                }

                #endregion

                #region Dependent Simulation Parameter

                else if (Parameters[p] is DependentSimulationParameter)
                {
                    DependentSimulationParameterReturnType returnType = (Parameters[p] as DependentSimulationParameter).ReturnType;

                    if (returnType == DependentSimulationParameterReturnType.Results)
                    {
                        // if we are looking for results, we only need to run one simulation
                        // constraints cannot apply to the Results return type because each data point is dependent on the last
                        // i.e. we can't change one of them without having to change everything after it...
                        DependentSimulation innerSimulation = (Parameters[p] as DependentSimulationParameter).DependentSimulation;
                        DependentSimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                        rawData[p] = innerResults.Results;
                    }
                    else
                    {
                        // we need to run a bunch of inner simulations, so use as many threads as possible
                        int summaryRunCount = (Parameters[p] as DependentSimulationParameter).SummaryRunCount;
                        DependentSimulation innerSimulation = (Parameters[p] as DependentSimulationParameter).DependentSimulation;

                        #region Multithread Simulations

                        ConcurrentBag<DependentSimulationResults> innerSimulations = new ConcurrentBag<DependentSimulationResults>();
                        List<Task> innerSimulationTasks = new List<Task>();

                        for (int s = 0; s < numberOfSimulations; s++)
                        {
                            Task t = Task.Run(() =>
                            {
                                innerSimulations.Add(innerSimulation.Simulate(summaryRunCount));
                            });

                            innerSimulationTasks.Add(t);
                        }

                        Task.WaitAll(innerSimulationTasks.ToArray());

                        #endregion

                        #region Unpack Summary Statistics

                        // use an array to hold invidual summary statistics
                        double[] summaryStatistics = new double[numberOfSimulations];
                        int i = 0;

                        foreach (var simulation in innerSimulations)
                        {
                            summaryStatistics[i] = simulation.GetSummaryStatistic(returnType);
                            i++;
                        }

                        #endregion

                        #region Check Constraints

                        // checking constraintas is valid for the summary statistic return type, because they are not dependent on each other
                        if ((Parameters[p] as DependentSimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (Parameters[p] as DependentSimulationParameter).Constraint;

                            for (int s = 0; s < summaryStatistics.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && summaryStatistics[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && summaryStatistics[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        summaryStatistics[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            DependentSimulationResults newResults = innerSimulation.Simulate(summaryRunCount);
                                            double newSample = newResults.GetSummaryStatistic(returnType);

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                summaryStatistics[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            summaryStatistics[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        // assign summary results to output
                        rawData[p] = summaryStatistics;
                    }
                }

                #endregion

                #region Discrete Parameter

                else if (Parameters[p] is DiscreteParameter)
                {
                    // generate array of values between 0 and 1
                    double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new ContinuousUniform(0d, 1d));
                    double[] correspondingValues = new double[numberOfSimulations];

                    // go through array of values and use cumulative probability of outcomes to determine the value for each simulation
                    for (int s = 0; s < numberOfSimulations; s++)
                    {
                        DiscreteOutcome[] possibleOutcomes = (Parameters[p] as DiscreteParameter).PossibleOutcomes;

                        for (int o = 0; o < possibleOutcomes.Length; o++)
                        {
                            if (randomProbabilities[s] <= possibleOutcomes[o]._cumulativeProbability)
                            {
                                correspondingValues[s] = possibleOutcomes[o].Value;
                                break;
                            }
                        }
                    }

                    rawData[p] = correspondingValues;
                }

                #endregion

                #region Distribution Parameter

                else if (Parameters[p] is DistributionParameter)
                {
                    if ((Parameters[p] as DistributionParameter).Distribution is IDiscreteDistribution)
                    {
                        // must use integers for discrete distributions
                        int[] samples = new int[numberOfSimulations];
                        ((dynamic)(Parameters[p] as DistributionParameter).Distribution).Samples(samples);

                        #region Check Constraints

                        if ((Parameters[p] as DistributionParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (Parameters[p] as DistributionParameter).Constraint;

                            for (int s = 0; s < samples.Length; s++)
                            {
                                // this is a special case that requires integer conversion for reassigned values
                                // not sure if we'll run into conversion errors on comparison, or if it will forceable retype ints
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && samples[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = Convert.ToInt32(constraint.LowerBound);
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && samples[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = Convert.ToInt32(constraint.UpperBound);
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        samples[s] = Convert.ToInt32(constraint.DefaultValue);
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            int newSample = ((dynamic)(Parameters[p] as DistributionParameter).Distribution).Sample();
                                                                                        
                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                samples[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {   
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            samples[s] = Convert.ToInt32(constraint.DefaultValue);
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        rawData[p] = samples;                       
                    }
                    else
                    {
                        // use doubles for continuous distribution
                        double[] samples = new double[numberOfSimulations];
                        ((dynamic)(Parameters[p] as DistributionParameter).Distribution).Samples(samples);

                        #region Check Constraints

                        if ((Parameters[p] as DistributionParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (Parameters[p] as DistributionParameter).Constraint;

                            for (int s = 0; s < samples.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && samples[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && samples[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        samples[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            double newSample = ((dynamic)(Parameters[p] as DistributionParameter).Distribution).Sample();

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                samples[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            samples[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        rawData[p] = samples;
                    }
                }

                #endregion

                #region Distribution Function Parameter

                else if (Parameters[p] is DistributionFunctionParameter)
                {
                    try
                    {
                        // compute location values using the underlying location parameter
                        double[] locationValues = new Simulation("location", (Parameters[p] as DistributionFunctionParameter).LocationParameter).Simulate(numberOfSimulations).Results;
                        double[] computeValues = new double[numberOfSimulations];

                        // use distribution, function, and location to compute results                   
                        switch ((Parameters[p] as DistributionFunctionParameter).ReturnType)
                        {
                            case DistributionFunctionParameterReturnType.CumulativeDistribution:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(Parameters[p] as DistributionFunctionParameter).Distribution).CumulativeDistribution(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.Density:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(Parameters[p] as DistributionFunctionParameter).Distribution).Density(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.DensityLn:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(Parameters[p] as DistributionFunctionParameter).Distribution).DensityLn(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.InverseCumulativeDistribution:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(Parameters[p] as DistributionFunctionParameter).Distribution).InverseCumulativeDistribution(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.Probability:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(Parameters[p] as DistributionFunctionParameter).Distribution).Probability(Convert.ToInt32(locationValues[i]));
                                break;
                            case DistributionFunctionParameterReturnType.ProbabilityLn:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(Parameters[p] as DistributionFunctionParameter).Distribution).ProbabilityLn(Convert.ToInt32(locationValues[i]));
                                break;
                            default:
                                break;
                        }

                        rawData[p] = computeValues;
                    }
                    catch (Exception innerException)
                    {
                        throw new DistributionFunctionFailureException($"Failed to compute {(Parameters[p] as DistributionFunctionParameter).ReturnType} function for {(Parameters[p] as DistributionFunctionParameter).Distribution.GetType()} distribution. This function may not be valid for this type of distribution. Check the MathNet.Numerics.Distributions documentation and the inner exception for more details.", innerException);
                    }
                }

                #endregion

                #region Precomputed Parameter

                else if (Parameters[p] is PrecomputedParameter)
                {
                    rawData[p] = (Parameters[p] as PrecomputedParameter).PrecomputedValues;
                }

                #endregion

                #region Qualitative Interpretation Parameter

                else if (Parameters[p] is QualitativeInterpretationParameter)
                {
                    // simulate the set of qualitative data
                    IQualitativeParameter qualitativeParameter = (Parameters[p] as QualitativeInterpretationParameter).QualitativeParameter;
                    QualitativeSimulation qualitativeSimulation = new QualitativeSimulation(qualitativeParameter);
                    string[] qualitativeOutcomes = qualitativeSimulation.Simulate(numberOfSimulations).Results;

                    // interpret outcomes
                    Dictionary<string, double> interpretationDictionary = (Parameters[p] as QualitativeInterpretationParameter).InterpretationDictionary;
                    double[] interpretedResults = new double[numberOfSimulations];

                    for (int outcome = 0; outcome < numberOfSimulations; outcome++)
                    {
                        if (interpretationDictionary.ContainsKey(qualitativeOutcomes[outcome]))
                        {
                            interpretedResults[outcome] = interpretationDictionary[qualitativeOutcomes[outcome]];
                        }
                        else
                        {
                            interpretedResults[outcome] = (Parameters[p] as QualitativeInterpretationParameter).DefaultValue;
                        }
                    }

                    rawData[p] = interpretedResults;
                }

                #endregion

                #region Random Bag Parameter

                else if (Parameters[p] is RandomBagParameter)
                {
                    RandomBagParameter randomBag = (RandomBagParameter)Parameters[p];
                    double[] selections = new double[numberOfSimulations];

                    if (randomBag.ReplacementRule == RandomBagReplacement.AfterEachPick)
                    {
                        double[] contents = randomBag.ContentsToArray();
                        int[] selectedIndices = new int[numberOfSimulations];
                        DiscreteUniform distribution = new DiscreteUniform(0, randomBag.NumberOfItems - 1);
                        distribution.Samples(selectedIndices);

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectedIndices[i]];
                    }
                    else if (randomBag.ReplacementRule == RandomBagReplacement.Never)
                    {
                        // validate we can choose the correct number of items
                        if (randomBag.NumberOfItems < numberOfSimulations)
                            throw new RandomBagItemCountException($"{numberOfSimulations} selections were requested from a bag containing {randomBag.NumberOfItems} items. This is not possible when the replacement rule is set to never.");

                        // choose without replacement
                        double[] contents = randomBag.ContentsToArray();
                        int[] selectionOrder = Enumerable.Range(0, numberOfSimulations).ToArray();
                        selectionOrder.Shuffle();

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectionOrder[i]];
                    }
                    else if (randomBag.ReplacementRule == RandomBagReplacement.WhenEmpty)
                    {
                        double[] contents = randomBag.ContentsToArray();
                        int numberOfRefills = (numberOfSimulations / randomBag.NumberOfItems) - (numberOfSimulations % randomBag.NumberOfItems == 0 ? 1 : 0);
                        int[] selectionOrder = new int[(numberOfRefills + 1) * numberOfSimulations];

                        // simulate refilling the bag to get enough indices to satisfy the number of simulations
                        for (int i = 0; i < numberOfRefills + 1; i++)
                        {
                            int[] roundSelectionOrder = Enumerable.Range(0, randomBag.NumberOfItems).ToArray();
                            roundSelectionOrder.Shuffle();

                            for (int j = 0; j < roundSelectionOrder.Length; j++)
                                selectionOrder[j + (i * randomBag.NumberOfItems)] = roundSelectionOrder[j];
                        }

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectionOrder[i]];
                    }
                    else
                    {
                        throw new RandomBagReplacementRuleException($"{randomBag.ReplacementRule} is not a valid replacement rule.");
                    }

                    rawData[p] = selections;
                }

                #endregion

                #region Simulation Parameter

                else if (Parameters[p] is SimulationParameter)
                {
                    SimulationParameterReturnType returnType = (Parameters[p] as SimulationParameter).ReturnType;

                    if (returnType == SimulationParameterReturnType.Results)
                    {
                        // if we are looking for results, we only need to run one simulation
                        Simulation innerSimulation = (Parameters[p] as SimulationParameter).Simulation;
                        SimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                        double[] samples = innerResults.Results;

                        #region Check Constraints

                        if ((Parameters[p] as SimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (Parameters[p] as SimulationParameter).Constraint;

                            for (int s = 0; s < samples.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && samples[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && samples[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        samples[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            // standard simulation only requires one data point
                                            // might be better to generate more than one sample to avoid overhead of parameter validation each time?
                                            double newSample = (Parameters[p] as SimulationParameter).Simulation.Simulate(1).First; 

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                samples[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            samples[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        rawData[p] = samples;
                    }
                    else
                    {
                        // we need to run a bunch of inner simulations, so use as many threads as possible
                        int summaryRunCount = (Parameters[p] as SimulationParameter).SummaryRunCount;
                        Simulation innerSimulation = (Parameters[p] as SimulationParameter).Simulation;

                        #region Multithread Simulations

                        ConcurrentBag<SimulationResults> innerSimulations = new ConcurrentBag<SimulationResults>();
                        List<Task> innerSimulationTasks = new List<Task>();

                        for (int s = 0; s < numberOfSimulations; s++)
                        {
                            Task t = Task.Run(() =>
                            {
                                innerSimulations.Add(innerSimulation.Simulate(summaryRunCount));
                            });

                            innerSimulationTasks.Add(t);
                        }

                        Task.WaitAll(innerSimulationTasks.ToArray());

                        #endregion

                        #region Unpack Summary Statistic

                        // use an array to hold invidual summary statistics
                        double[] summaryStatistics = new double[numberOfSimulations];
                        int i = 0;

                        foreach (var simulation in innerSimulations)
                        {
                            summaryStatistics[i] = simulation.GetSummaryStatistic(returnType);
                            i++;
                        }

                        #endregion

                        #region Check Constraints

                        if ((Parameters[p] as SimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (Parameters[p] as SimulationParameter).Constraint;

                            for (int s = 0; s < summaryStatistics.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && summaryStatistics[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && summaryStatistics[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        summaryStatistics[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            SimulationResults newResults = innerSimulation.Simulate(summaryRunCount);
                                            double newSample = newResults.GetSummaryStatistic(returnType);

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                summaryStatistics[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            summaryStatistics[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        // assign summary results to output
                        rawData[p] = summaryStatistics;
                    }
                }

                #endregion 

                #region Invalid Parameter

                else
                {
                    throw new InvalidParameterException($"{Parameters[p].Name} is a {Parameters[p].GetType()} parameter, which is not one the parameter types that simulations are able to handle. Acceptable parameter types are: Conditional, Constant, Discrete, Distribution, Precomputed, Simulation, DependentSimulation, and QualitativeInterpretation.");
                }

                #endregion
            }

            // return results
            return new SimulationResults(Parameters, rawData, Expression);
        }
    }

    public class SimulationResults
    {
        // private results
        private IParameter[] _parameters;
        private object[] _rawData;
        private Expression _expression;
        private double[] _results;
        private double[] _copyOfResults;


        // public copy of results and raw data
        public IParameter[] Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }
        public object[] RawData
        {
            get
            {
                // return a copy to prevent overriding data
                object[] returnArray = new object[_rawData.Length];
                _rawData.CopyTo(returnArray, 0); // not sure this copies correctly because of nesting
                return returnArray;
            }
            private set
            {
                // create an internal copy to use for summary statistics
                _rawData = value;
            }
        }
        public Expression Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                _expression = value;
                ComputeResults();
            }
        }
        public double[] Results
        {
            get
            {
                // return a copy to prevent overriding data
                double[] returnArray = new double[_results.Length];
                _results.CopyTo(returnArray, 0);
                return returnArray;
            }
            private set
            {
                // create an internal copy to use for summary statistics
                _results = value;
                _copyOfResults = new double[_results.Length];
                _results.CopyTo(_copyOfResults, 0);
            }
        }


        // public summary statistics
        public int NumberOfSimulations
        {
            get
            {
                return Results.Length;
            }
        }
        public double First
        {
            get
            {
                return _results.First();
            }
        }
        public double Last
        {
            get
            {
                return _results.Last();
            }
        }
        public double Minimum
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Minimum(_copyOfResults);
            }
        }
        public double LowerQuartile
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.LowerQuartileInplace(_copyOfResults);
            }
        }
        public double Mean
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Mean(_copyOfResults);
            }
        }
        public double Median
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.MedianInplace(_copyOfResults);
            }
        }
        public double UpperQuartile
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.UpperQuartileInplace(_copyOfResults);
            }
        }
        public double Maximum
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Maximum(_copyOfResults);
            }
        }
        public double Variance
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Variance(_copyOfResults);
            }
        }
        public double StandardDeviation
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.StandardDeviation(_copyOfResults);
            }
        }
        public double Kurtosis
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Kurtosis(_copyOfResults);
            }
        }
        public double Skewness
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Skewness(_copyOfResults);
            }
        }


        // summary methods
        /// <summary>
        /// Returns the confidence interval of the results assuming that they are normally distributed.
        /// </summary>
        /// <param name="confidenceLevel">Confidence level to use when generating the confidence interval.</param>
        /// <returns></returns>
        public ConfidenceInterval ConfidenceInterval(ConfidenceLevel confidenceLevel)
        {
            double z = confidenceLevel.ZScore();
            double sqrtOfNumberOfSimulations = Math.Sqrt(NumberOfSimulations);
            double standardDeviation = StandardDeviation;
            double upperBound = Mean + z * (standardDeviation / sqrtOfNumberOfSimulations);
            double lowerBound = Mean - z * (standardDeviation / sqrtOfNumberOfSimulations);
            return new ConfidenceInterval(confidenceLevel, lowerBound, upperBound);
        }
        /// <summary>
        /// Returns the summary statistic associated with the passed enum. Can only return summary statistics that are of type double.
        /// </summary>
        /// <param name="summaryStatistic">Enum corresponding to the desired summary statistic.</param>
        /// <returns></returns>
        public double GetSummaryStatistic(SimulationParameterReturnType summaryStatistic)
        {
            switch (summaryStatistic)
            {
                case SimulationParameterReturnType.Minimum:
                    return Minimum;
                case SimulationParameterReturnType.LowerQuartile:
                    return LowerQuartile;
                case SimulationParameterReturnType.Mean:
                    return Mean;
                case SimulationParameterReturnType.Median:
                    return Median;
                case SimulationParameterReturnType.UpperQuartile:
                    return UpperQuartile;
                case SimulationParameterReturnType.Maximum:
                    return Maximum;
                case SimulationParameterReturnType.Variance:
                    return Variance;
                case SimulationParameterReturnType.StandardDeviation:
                    return StandardDeviation;
                case SimulationParameterReturnType.Kurtosis:
                    return Kurtosis;
                case SimulationParameterReturnType.Skewness:
                    return Skewness;
                default:
                    throw new InvalidSummaryStatisticException($"Cannot return {summaryStatistic}. Only statistics that return a single number can be accessed using this function.");
            }
        }


        // constructor
        public SimulationResults(IParameter[] parameters, object[] rawData, Expression expression)
        {
            this.Parameters = parameters;
            this.RawData = rawData;
            this.Expression = expression;
        }


        // methods
        private void ComputeResults()
        {
            // perform operation chain on each row of existing matrix to generate one column of results
            int numberOfSimulations = ((dynamic)_rawData[0]).Length; // we know it is an array but have no idea what type of array it is... which seems like a design flaw
            double[] simulationResults = new double[numberOfSimulations];

            for (int s = 0; s < numberOfSimulations; s++)
            {
                Dictionary<string, FloatingPoint> symbols = new Dictionary<string, FloatingPoint>();

                for (int p = 0; p < _parameters.Length; p++)
                {
                    if (_parameters[p] is DistributionParameter && (_parameters[p] as DistributionParameter).Distribution is IDiscreteDistribution)
                    {
                        // results are stored as integers
                        symbols.Add(_parameters[p].Name, ((int[])_rawData[p])[s]);
                    }
                    else if (_parameters[p] is PrecomputedParameter)
                    {
                        // results are stored as FloatingPoint
                        symbols.Add(_parameters[p].Name, ((FloatingPoint[])_rawData[p])[s]);
                    }
                    else
                    {
                        // results are stored as doubles
                        symbols.Add(_parameters[p].Name, ((double[])_rawData[p])[s]);
                    }
                }

                simulationResults[s] = Evaluate.Evaluate(symbols, _expression).RealValue;
            }

            // return results
            this.Results = simulationResults;
        }

        /// <summary>
        /// Computes a new expression using the data generated by the previously run simulation. This function can be used to do limited sensitivity analysis on the output without the overhead of recomputing all parameters.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <returns>SimulationResults containing an array of doubles and summary statistics.</returns>
        public SimulationResults RecomputeExpression(string expression)
        {
            return RecomputeExpression(Infix.ParseOrThrow(expression)); // returns a reference, not a copy
        }
        /// <summary>
        /// Computes a new expression using the data generated by the previously run simulation. This function can be used to do limited sensitivity analysis on the output without the overhead of recomputing all parameters.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <returns>SimulationResults containing an array of doubles and summary statistics.</returns>
        internal SimulationResults RecomputeExpression(Expression expression)
        {
            this.Expression = expression;
            return this; // returns a reference, not a copy
        }

        /// <summary>
        /// Adds a new parameter to the raw data that has already been simulated. Once a new parameter has been added, a new expression that incporates the parameter may be provided.
        /// </summary>
        /// <param name="parameter">New parameter to add to raw data.</param>
        public void AddParameter(IParameter parameter)
        {
            // simulate data for just the new variable
            Simulation additionalParameter = new Simulation(parameter.Name, parameter);
            SimulationResults additionalResults = additionalParameter.Simulate(((dynamic)_rawData[0]).Length);

            // add new parameter
            IParameter[] newParameters = new IParameter[_parameters.Length + 1];
            _parameters.CopyTo(newParameters, 0);
            newParameters[newParameters.GetUpperBound(0)] = parameter;
            Parameters = newParameters;

            // add new data
            object[] newRawData = new object[_parameters.Length];
            _rawData.CopyTo(newRawData, 0);
            newRawData[newRawData.GetUpperBound(0)] = additionalResults.RawData[0];
            RawData = newRawData;
        }
        /// <summary>
        /// Removes an existing parameter from the raw data that has already been simulated. The existing parameter cannot currently be part of the expression or this will fail.
        /// </summary>
        /// <param name="parameter">Parameter to remove from the existing raw data.</param>
        public void RemoveParameter(IParameter parameter)
        {
            // validation
            if (_parameters.Contains(parameter) == false)
                throw new InvalidParameterException($"{parameter.Name} was not used as a parameter in this simulation.");

            if (Infix.Format(_expression).Contains(parameter.Name))
                throw new ParameterInExpressionException($"Cannot remove {parameter.Name} because it is currently being used in the expression. Provide a new expression that does not use parameter before trying to remove it.");

            // remove parameter and raw data
            IParameter[] newParameters = new IParameter[_parameters.Length - 1];
            object[] newRawData = new object[_parameters.Length - 1];
            bool skipped = false;

            for (int i = 0; i < _parameters.Length; i++)
            {
                if (_parameters[i].Name == parameter.Name)
                {
                    skipped = true;
                    continue;
                }
                else
                {
                    newParameters[skipped ? i - 1 : i] = _parameters[i];
                    newRawData[skipped ? i - 1 : i] = _rawData[i];
                }
            }

            Parameters = newParameters;
            RawData = newRawData;
        }
        /// <summary>
        /// Replaces the old parameter with a new parameter, regenerates data for only the changed parameter, and recomputes the results using the rest of the existing data.
        /// </summary>
        /// <param name="oldParameter">Old parameter that should be repalced.</param>
        /// <param name="newParameter">New parameter that should be used in place of the old parameter. If the name does not match the name of the old parameter, the expression will be updated with the new parameter name.</param>
        /// <returns>DependentSimulationResults containing results of the new simulation.</returns>
        public SimulationResults ReplaceParameter(IParameter oldParameter, IParameter newParameter)
        {
            bool notFound = true;

            for (int p = 0; p < _parameters.Length; p++)
            {
                if (_parameters[p] == oldParameter)
                {
                    // replace the parameter
                    notFound = false;
                    _parameters[p] = newParameter;

                    // simulate data for just the new variable
                    Simulation additionalParameter = new Simulation(newParameter.Name, newParameter);
                    SimulationResults additionalResults = additionalParameter.Simulate(((dynamic)_rawData[0]).Length);

                    // replace the stored parameter data
                    _rawData[p] = additionalResults._rawData[0];              

                    break;
                }
            }

            if (notFound)
            {
                throw new InvalidParameterException($"{oldParameter.Name} was not used as a parameter in this simulation.");
            }
            else
            {
                // update expression if parameter name has changed
                if (Infix.Format(_expression).Contains(newParameter.Name) == false)
                {
                    string oldExpression = Infix.Format(_expression);
                    oldExpression = oldExpression.Replace(oldParameter.Name, newParameter.Name);
                    this.Expression = Infix.ParseOrThrow(oldExpression);
                }

                ComputeResults();
                return this;
            }
        }

        /// <summary>
        /// Re-runs the simulation used to generate this SimulationResults object with the same number of simulations that was original used. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SimulationResults containing results of the new simulation.</returns>
        public SimulationResults Regenerate()
        {
            return Regenerate(((dynamic)_rawData[0]).Length);
        }
        /// <summary>
        /// Re-runs the simulation used to generate this SimulationResults object with the specified number of simulation. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SimulationResults containing results of the new simulation.</returns>
        public SimulationResults Regenerate(int numberOfSimulations)
        {
            Simulation newSimulation = new Simulation(_expression, _parameters);
            this.RawData = newSimulation.Simulate(numberOfSimulations).RawData;
            ComputeResults();
            return this;
        }
    }

    #endregion

    #region Dependent

    public class DependentSimulation
    {
        public double StartValue { get; set; }
        public Expression Expression { get; set; }
        public IParameter ChangeParameter { get; set; }

        /// <summary>
        /// A simulation where each value is the previous simulation's value modified by an algebraic expression that utilizes the change parameter.
        /// </summary>
        /// <param name="startValue">Value to be used as the previous value when running the first simulation.</param>
        /// <param name="expression">An expression that uses 'value' to refer to the previous simulation's value and references the change parameter by its name.</param>
        /// <param name="changeParameter">A parameter used to generate a value to use when computing each simulation's expression.</param>
        internal DependentSimulation(double startValue, Expression expression, IParameter changeParameter)
        {
            this.StartValue = startValue;
            this.Expression = expression;
            this.ChangeParameter = changeParameter;
        }
        /// <summary>
        /// A simulation where each value is the previous simulation's value modified by an algebraic expression that utilizes the change parameter.
        /// </summary>
        /// <param name="startValue">Value to be used as the previous value when running the first simulation.</param>
        /// <param name="expression">An expression that uses 'value' to refer to the previous simulation's value and references the change parameter by its name.</param>
        /// <param name="changeParameter">A parameter used to generate a value to use when computing each simulation's expression.</param>
        public DependentSimulation(double startValue, string expression, IParameter changeParameter)
        {
            this.StartValue = startValue;
            this.Expression = Infix.ParseOrThrow(expression);
            this.ChangeParameter = changeParameter;
        }

        /// <summary>
        /// Performs the given number of depdent simulations using the provided parameters and expressions. Results will be an array of doubles with one number per simulation.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>DependentSimulationResults containing an array of doubles and summary statistics.</returns>
        public DependentSimulationResults Simulate(int numberOfSimulations)
        {
            double[] dependentResults = new double[numberOfSimulations];

            // precompute all parameter values
            Simulation parameterSimulation = new Simulation(ChangeParameter.Name, ChangeParameter);
            double[] parameterValues = parameterSimulation.Simulate(numberOfSimulations).Results;

            // create dictionary once to increase run speed
            Dictionary<string, FloatingPoint> symbols = new Dictionary<string, FloatingPoint>();
            symbols.Add("value", StartValue);
            symbols.Add(ChangeParameter.Name, 0);

            for (int s = 0; s < numberOfSimulations; s++)
            {
                symbols[ChangeParameter.Name] = parameterValues[s]; // set to current parameter value                
                dependentResults[s] = Evaluate.Evaluate(symbols, Expression).RealValue;
                symbols["value"] = dependentResults[s]; // set to value calculated above since this should lag s by 1
            }

            // uses specialized results.. but this somehow needs to be compatible with simulation parameter
            return new DependentSimulationResults(StartValue, Expression, ChangeParameter, dependentResults, parameterValues);
        }
    }

    public class DependentSimulationResults
    {
        // private results
        private double _initialValue;     
        private Expression _expression;
        private IParameter _changeParameter;
        private double[] _results;
        private double[] _changeValues;


        // public copy of results and raw data
        public double InitialValue
        {
            get
            {
                return _initialValue;
            }
            set
            {
                _initialValue = value;
            }
        }
        public Expression Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                _expression = value;
            }
        }
        public IParameter ChangeParameter
        {
            get
            {
                return _changeParameter;
            }
            set
            {
                _changeParameter = value;
            }
        }        
        public double[] Results
        {
            get
            {
                // return a copy to prevent overriding data
                double[] returnArray = new double[_results.Length];
                _results.CopyTo(returnArray, 0); // not sure this copies correctly because of nesting
                return returnArray;
            }
            private set
            {
                // create an internal copy to use for summary statistics
                _results = value;
            }
        }
        public double[] ChangeValues
        {
            get
            {
                // return a copy to prevent overriding data
                double[] returnArray = new double[_results.Length];
                _results.CopyTo(returnArray, 0); // not sure this copies correctly because of nesting
                return returnArray;
            }
            private set
            {
                // create an internal copy to use for summary statistics
                _changeValues = value;
            }
        }


        // public summary statistics
        public int NumberOfPeriods
        {
            get
            {
                return _results.Length;
            }
        }
        public double EndingValue
        {
            get
            {
                return _results.Last();
            }
        }
        public double LowestValue
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Minimum(_results);
            }
        }
        public double HighestValue
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Maximum(_results);
            }
        }
        public string ValueRange
        {
            get
            {
                return $"{LowestValue} - {HighestValue}";
            }
        }
        public double RangeSize
        {
            get
            {
                return HighestValue - LowestValue;
            }
        }
        public double SmallestChange
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Minimum(_changeValues);
            }
        }
        public double LargestChange
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Maximum(_changeValues);
            }
        }
        public double AverageChange
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Mean(_changeValues);
            }
        }
        public double StandardDeviationOfChanges
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.StandardDeviation(_changeValues);
            }
        }


        // summary methods
        /// <summary>
        /// Returns the summary statistic associated with the passed enum. Can only return summary statistics that are of type double.
        /// </summary>
        /// <param name="summaryStatistic">Enum corresponding to the desired summary statistic.</param>
        /// <returns></returns>
        public double GetSummaryStatistic(DependentSimulationParameterReturnType summaryStatistic)
        {
            switch (summaryStatistic)
            {
                case DependentSimulationParameterReturnType.EndingValue:
                    return EndingValue;
                case DependentSimulationParameterReturnType.LowestValue:
                    return LowestValue;
                case DependentSimulationParameterReturnType.HighestValue:
                    return HighestValue;
                case DependentSimulationParameterReturnType.RangeSize:
                    return RangeSize;
                case DependentSimulationParameterReturnType.SmallestChange:
                    return SmallestChange;
                case DependentSimulationParameterReturnType.LargestChange:
                    return LargestChange;
                case DependentSimulationParameterReturnType.AverageChange:
                    return AverageChange;
                case DependentSimulationParameterReturnType.StandardDeviationOfChanges:
                    return StandardDeviationOfChanges;
                default:
                    throw new InvalidSummaryStatisticException($"Cannot return {summaryStatistic}. Only statistics that return a single number can be accessed using this function."); ;
            }
        }


        // constructor
        public DependentSimulationResults(double initialValue, Expression expression, IParameter changeParameter, double[] results, double[] changeValues)
        {
            this.InitialValue = initialValue;
            this.Expression = expression;
            this.ChangeParameter = changeParameter;
            this.Results = results;
            this.ChangeValues = changeValues;
        }


        // methods
        private void Resimulate(int numberOfSimulations)
        {
            DependentSimulation newSimulation = new DependentSimulation(_initialValue, _expression, _changeParameter);
            DependentSimulationResults newResults = newSimulation.Simulate(numberOfSimulations);
            this.Results = newResults.Results;
            this.ChangeValues = newResults.ChangeValues;
        }
        private void Recompute()
        {
            double[] recomputedResults = new double[_results.Length];

            Dictionary<string, FloatingPoint> symbols = new Dictionary<string, FloatingPoint>();
            symbols.Add("value", _initialValue);
            symbols.Add(ChangeParameter.Name, 0);

            for (int s = 0; s < _results.Length; s++)
            {
                symbols[ChangeParameter.Name] = _changeValues[s]; // set to current parameter value                
                recomputedResults[s] = Evaluate.Evaluate(symbols, _expression).RealValue;
                symbols["value"] = recomputedResults[s]; // set to value calculated above since this should lag s by 1
            }

            this.Results = recomputedResults;
        }
       
        /// <summary>
        /// Computes a new expression using the change data generated by the previously run simulation. This function can be used to do limited sensitivity analysis on the output without the overhead of recomputing changes.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses value to refer the initial value and the value of the previous simulation, and the parameter name as variables.</param>
        /// <returns>DependentSimulationResults containing an array of doubles and summary statistics.</returns>
        public DependentSimulationResults RecomputeExpression(string expression)
        {
            return RecomputeExpression(Infix.ParseOrThrow(expression)); // returns a reference, not a copy
        }
        /// <summary>
        /// Computes a new expression using the change data generated by the previously run simulation. This function can be used to do limited sensitivity analysis on the output without the overhead of recomputing changes.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses value to refer the initial value and the value of the previous simulation, and the parameter name as variables.</param>
        /// <returns>DependentSimulationResults containing an array of doubles and summary statistics.</returns>
        internal DependentSimulationResults RecomputeExpression(Expression expression)
        {
            this.Expression = expression;
            Recompute(); 
            return this;
        }

        /// <summary>
        /// Re-runs the simulation used to generate this DependentSimulationResults object with the same number of simulations that was original used.
        /// </summary>
        /// <returns>DependentSimulationResults containing results of the new simulation.</returns>
        public DependentSimulationResults Regenerate()
        {
            Resimulate(_results.Length);
            return this;
        }
        /// <summary>
        /// Re-runs the simulation used to generate this DependentSimulationResults object with the specified number of simulation.
        /// </summary>
        /// <returns>DependentSimulationResults containing results of the new simulation.</returns>
        public DependentSimulationResults Regenerate(int numberOfSimulations)
        {
            Resimulate(numberOfSimulations);
            return this;
        }

        /// <summary>
        /// Replaces the change parameter with a new parameter and re-runs the simulation.
        /// </summary>
        /// <param name="changeParameter">New parameter that should be used to generate changes. If the name does not match the name of the old parameter, the expression will be updated with the new parameter name.</param>
        /// <returns>DependentSimulationResults containing results of the new simulation.</returns>
        public DependentSimulationResults ReplaceParameter(IParameter changeParameter)
        {
            // update expression if parameter name has changed
            if (Infix.Format(_expression).Contains(changeParameter.Name) == false)
            {
                string oldExpression = Infix.Format(_expression);
                oldExpression = oldExpression.Replace(_changeParameter.Name, changeParameter.Name);
                this.Expression = Infix.ParseOrThrow(oldExpression);
            }

            this.ChangeParameter = changeParameter;
            Resimulate(_results.Length);
            return this;
        }
        /// <summary>
        /// Replaces the initial value with a new initial value and recomputes the results using the existing change data.
        /// </summary>
        /// <param name="initialValue">New initla value to use when computing the first simulations result.</param>
        /// <returns>DependentSimulationResults containing results of the new simulation.</returns>
        public DependentSimulationResults ReplaceInitialValue(double initialValue)
        {
            this.InitialValue = initialValue;
            Recompute();
            return this;
        }
    }

    #endregion

    #region Sensitivity

    public class ExhaustiveSensitivitySimulation
    {
        private List<string> _precomputedParameterSetNames = new List<string>();
        private List<FloatingPoint[]> _precomputedParameterSets = new List<FloatingPoint[]>();

        public Simulation Simulation { get; set; }

        /// <summary>
        /// A simulation that will return a dictionary of combinations and simulation results for each exhaustive combination of PrecomputedParameter values. Exhaustive combination means all possible combinations that result from taking one precomputed value from each of the PrecomputedParameters.
        /// </summary>
        /// <param name="simulation">The simulation that was built with one or more PrecomputedValueParameters. The simulation will be resimulated the desired number of times for each exhaustive combination of values of the PrecomputedValueParameters.</param>
        public ExhaustiveSensitivitySimulation(Simulation simulation)
        {
            // validate simulation contains at least one precomputer parameter
            int precomputedParameters = (from param in simulation.Parameters
                                            where param is PrecomputedParameter
                                            select param).Count();

            if (precomputedParameters == 0)
            {
                throw new MissingPrecomputedParameterException("The simulation must contain at least one PrecomputeParameter in order to create an ExhaustiveSensitivitySimulation. However, there is no benefit to be gained from using the exhaustive version unless there are two or more precomputed parameters.");
            }
            else
            {
                var parameters = (from p in simulation.Parameters
                                    where p is PrecomputedParameter
                                    select p).ToArray();

                for (int i = 0; i < parameters.Length; i++)
                {
                    _precomputedParameterSetNames.Add((parameters[i] as PrecomputedParameter).Name);
                    _precomputedParameterSets.Add((parameters[i] as PrecomputedParameter).PrecomputedValues);
                }

            }

            this.Simulation = simulation;
        }

        /// <summary>
        /// Performs the given number of simulations using the provided parameters and expressions. Results will be dictionary where each key is a combination of PrecomputedParameters and the value is the SimulationResults for that set of parameters.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>SensitivitySimulationResults containing a dictionary of combinations and results and summary statistics.</returns>
        public SensitivitySimulationResults Simulate(int numberOfSimulations)
        {
            Dictionary<string, SimulationResults> allSimulationResults = new Dictionary<string, SimulationResults>();

            // run simulations for each sensitivity factor combination, one at a time
            if (_precomputedParameterSets.Count == 1)
            {
                for (int f = 0; f < _precomputedParameterSets[0].Length; f++)
                {
                    List<string> precomputedParameters = new List<string>();

                    // convert precomputed parameters into constants for the current factor
                    IParameter[] convertedParameters = new IParameter[Simulation.Parameters.Length];

                    for (int p = 0; p < Simulation.Parameters.Length; p++)
                    {
                        if (Simulation.Parameters[p] is PrecomputedParameter)
                        {
                            // had to convert from floating point again here...
                            convertedParameters[p] = new ConstantParameter(Simulation.Parameters[p].Name, (Simulation.Parameters[p] as PrecomputedParameter).PrecomputedValues[f].RealValue);
                            precomputedParameters.Add($"{convertedParameters[p].Name} = {(convertedParameters[p] as ConstantParameter).Value}");
                        }
                        else
                        {
                            convertedParameters[p] = Simulation.Parameters[p];
                        }
                    }

                    // run all simulations for the current factor
                    allSimulationResults.Add(string.Join("; ", precomputedParameters), new Simulation(Simulation.Expression, convertedParameters).Simulate(numberOfSimulations));
                }
            }
            else
            {
                // take the cartesian product of all possible combinations
                var exhaustiveCombinations = _precomputedParameterSets.CartesianProduct();

                foreach (var combination in exhaustiveCombinations)
                {
                    int precomputedFactorIndex = 0;
                    List<string> precomputedParameters = new List<string>();

                    // convert precomputed parameters into constants for the current factor
                    IParameter[] convertedParameters = new IParameter[Simulation.Parameters.Length];

                    for (int p = 0; p < Simulation.Parameters.Length; p++)
                    {
                        if (Simulation.Parameters[p] is PrecomputedParameter)
                        {
                            convertedParameters[p] = new ConstantParameter(_precomputedParameterSetNames[precomputedFactorIndex], combination.ElementAt(precomputedFactorIndex).RealValue);
                            precomputedParameters.Add($"{convertedParameters[p].Name} = {(convertedParameters[p] as ConstantParameter).Value}");
                            precomputedFactorIndex++;
                        }
                        else
                        {
                            convertedParameters[p] = Simulation.Parameters[p];
                        }
                    }

                    // run all simulations for the current factor
                    allSimulationResults.Add(string.Join("; ", precomputedParameters), new Simulation(Simulation.Expression, convertedParameters).Simulate(numberOfSimulations));

                }
            }

            return new SensitivitySimulationResults(allSimulationResults, Simulation.Parameters, Simulation.Expression, numberOfSimulations, true, false);
        }

        /// <summary>
        /// Uses multithreading to performs the given number of simulations using the provided parameters and expressions. Results will be dictionary where each key is a combination of PrecomputedParameters and the value is the SimulationResults for that set of parameters. This method should be used when computations are expected to be intensive and you need the speed boost. For small calculations, this will provided limited/possible negative speed increases, so don't use it.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>SensitivitySimulationResults containing a dictionary of combinations and results and summary statistics.</returns>
        public SensitivitySimulationResults Simulate_Multithreaded(int numberOfSimulations)
        {
            ConcurrentDictionary<string, SimulationResults> allSimulationResults = new ConcurrentDictionary<string, SimulationResults>();

            // run simulations for each sensitivity factor combination, one at a time
            if (_precomputedParameterSets.Count == 1)
            {
                for (int f = 0; f < _precomputedParameterSets[0].Length; f++)
                {
                    List<string> precomputedParameters = new List<string>();

                    // convert precomputed parameters into constants for the current factor
                    IParameter[] convertedParameters = new IParameter[Simulation.Parameters.Length];

                    for (int p = 0; p < Simulation.Parameters.Length; p++)
                    {
                        if (Simulation.Parameters[p] is PrecomputedParameter)
                        {
                            // had to convert from floating point again here...
                            convertedParameters[p] = new ConstantParameter(Simulation.Parameters[p].Name, (Simulation.Parameters[p] as PrecomputedParameter).PrecomputedValues[f].RealValue);
                            precomputedParameters.Add($"{convertedParameters[p].Name} = {(convertedParameters[p] as ConstantParameter).Value}");
                        }
                        else
                        {
                            convertedParameters[p] = Simulation.Parameters[p];
                        }
                    }

                    // run all simulations for the current factor
                    allSimulationResults.TryAdd(string.Join("; ", precomputedParameters), new Simulation(Simulation.Expression, convertedParameters).Simulate(numberOfSimulations));
                }
            }
            else
            {
                // take the cartesian product of all possible combinations
                var exhaustiveCombinations = _precomputedParameterSets.CartesianProduct();
                List<Task> simulationTasks = new List<Task>();

                foreach (var combination in exhaustiveCombinations)
                {
                    // need to store local copies of numberOfSimulations, parameters, and parameter names to avoid issues with concurrent access
                    string[] localParameterSetNames = new string[_precomputedParameterSetNames.Count];
                    _precomputedParameterSetNames.CopyTo(localParameterSetNames);

                    IParameter[] localParameters = new IParameter[Simulation.Parameters.Length];
                    Simulation.Parameters.CopyTo(localParameters, 0);

                    int localNumberOfSimulations = numberOfSimulations;

                    // start task
                    Task t = Task.Run(() =>
                    {
                        int precomputedFactorIndex = 0;
                        List<string> precomputedParameters = new List<string>();

                        // convert precomputed parameters into constants for the current factor
                        IParameter[] convertedParameters = new IParameter[localParameters.Length];

                        for (int p = 0; p < localParameters.Length; p++)
                        {
                            if (localParameters[p] is PrecomputedParameter)
                            {
                                convertedParameters[p] = new ConstantParameter(localParameterSetNames[precomputedFactorIndex], combination.ElementAt(precomputedFactorIndex).RealValue);
                                precomputedParameters.Add($"{convertedParameters[p].Name} = {(convertedParameters[p] as ConstantParameter).Value}");
                                precomputedFactorIndex++;
                            }
                            else
                            {
                                convertedParameters[p] = localParameters[p];
                            }
                        }

                        // run all simulations for the current factor
                        allSimulationResults.TryAdd(string.Join("; ", precomputedParameters), new Simulation(Simulation.Expression, convertedParameters).Simulate(localNumberOfSimulations));
                    });

                    simulationTasks.Add(t);
                }

                Task.WaitAll(simulationTasks.ToArray());
            }

            // extract and sort keys before returning results
            Dictionary<string, SimulationResults> nonConcurrentDictionary = new Dictionary<string, SimulationResults>();
            var kvPairs = from kv in allSimulationResults
                            orderby kv.Key // this sort of works.. but doesn't really give me the order i'd expect...
                            select kv;

            foreach (var kv in kvPairs)
                nonConcurrentDictionary.Add(kv.Key, kv.Value);

            return new SensitivitySimulationResults(nonConcurrentDictionary, Simulation.Parameters, Simulation.Expression, numberOfSimulations, true, true);
        }
    }

    public class SensitivitySimulation
    {
        private int _numberOfFactors;

        public Simulation Simulation { get; set; }

        /// <summary>
        /// A simulation that will return a dictionary of combinations and simulation results for each set of PrecomputedParameter values.
        /// </summary>
        /// <param name="simulation">The simulation that was built with one or more PrecomputedValueParameters. The simulation will be resimulated the desired number of times for each set of values of the PrecomputedValueParameters.</param>
        public SensitivitySimulation(Simulation simulation)
        {
            // validate simulation contains at least one precomputer parameter
            int precomputedParameters = (from param in simulation.Parameters
                                            where param is PrecomputedParameter
                                            select param).Count();

            if (precomputedParameters == 0)
            {
                throw new MissingPrecomputedParameterException("The simulation must contain at least one PrecomputeParameter in order to create a SensitivitySimulation.");
            }
            else if (precomputedParameters == 1)
            {
                _numberOfFactors = ((from p in simulation.Parameters
                                        where p is PrecomputedParameter
                                        select p).First() as PrecomputedParameter).PrecomputedValues.Length;
            }
            else if (precomputedParameters > 1)
            {
                var parameters = (from p in simulation.Parameters
                                    where p is PrecomputedParameter
                                    select p).ToArray();

                int length = (parameters[0] as PrecomputedParameter).PrecomputedValues.Length;

                for (int i = 1; i < parameters.Count(); i++)
                {
                    if ((parameters[i] as PrecomputedParameter).PrecomputedValues.Length != length)
                        throw new PrecomputedValueCountException($"{(parameters[i] as PrecomputedParameter).Name} has {(parameters[i] as PrecomputedParameter).PrecomputedValues.Length} precomputed values but {(parameters[0] as PrecomputedParameter).Name} has {(parameters[0] as PrecomputedParameter).PrecomputedValues.Length} precomputed values. If a simulation contains multiple sets of precomputed values, each set must have the same number of values.");
                }

                _numberOfFactors = length;
            }

            this.Simulation = simulation;
        }

        /// <summary>
        /// Performs the given number of simulations using the provided parameters and expressions. Results will be dictionary where each key is a combination of PrecomputedParameters and the value is the SimulationResults for that set of parameters.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>SensitivitySimulationResults containing a dictionary of combinations and results and summary statistics.</returns>
        public SensitivitySimulationResults Simulate(int numberOfSimulations)
        {
            Dictionary<string, SimulationResults> allSimulationResults = new Dictionary<string, SimulationResults>();

            // run simulations for each sensitivity factor, one at a time
            for (int f = 0; f < _numberOfFactors; f++)
            {
                List<string> precomputedParameters = new List<string>();

                // convert precomputed parameters into constants for the current factor
                IParameter[] convertedParameters = new IParameter[Simulation.Parameters.Length];

                for (int p = 0; p < Simulation.Parameters.Length; p++)
                {
                    if (Simulation.Parameters[p] is PrecomputedParameter)
                    {
                        // had to convert from floating point again here...
                        convertedParameters[p] = new ConstantParameter(Simulation.Parameters[p].Name, (Simulation.Parameters[p] as PrecomputedParameter).PrecomputedValues[f].RealValue);
                        precomputedParameters.Add($"{convertedParameters[p].Name} = {(convertedParameters[p] as ConstantParameter).Value}");
                    }
                    else
                    {
                        convertedParameters[p] = Simulation.Parameters[p];
                    }
                }

                // run all simulations for the current factor
                allSimulationResults.Add(string.Join("; ", precomputedParameters), new Simulation(Simulation.Expression, convertedParameters).Simulate(numberOfSimulations));
            }

            return new SensitivitySimulationResults(allSimulationResults, Simulation.Parameters, Simulation.Expression, numberOfSimulations, false, false);
        }
    }

    public class SensitivitySimulationResults
    {
        // private backing variables
        private IParameter[] _parameters;
        private Expression _expression;
        private int _numberOfSimulations;
        private bool _exhaustive;       
        private bool _multithreaded;


        // public properties
        public Dictionary<string, SimulationResults> Results { get; private set; }
        public IParameter[] Parameters
        {
            get
            {
                IParameter[] returnArray = new IParameter[_parameters.Length];
                _parameters.CopyTo(returnArray, 0);
                return returnArray;
            }
            private set
            {
                _parameters = value;
            }
        }
        public Expression Expression
        {
            get
            {
                return _expression;
            }
            private set
            {
                _expression = value;
            }
        }
        public int NumberOfSimulations
        {
            get
            {
                return _numberOfSimulations;
            }
            private set
            {
                _numberOfSimulations = value;
            }
        }
        public bool Exhaustive
        {
            get
            {
                return _exhaustive;
            }
            private set
            {
                _exhaustive = value;
            }
        }
        public bool MultiThreaded
        {
            get
            {
                return _multithreaded;
            }
            private set
            {
                _multithreaded = value;
            }
        }        


        // summary statistics
        public KeyValuePair<string, SimulationResults> HighestMean
        {
            get
            {
                double maxValue = Results.Max(kv => kv.Value.Mean);
                return Results.Where(kv => kv.Value.Mean == maxValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> HighestMedian
        {
            get
            {
                double maxValue = Results.Max(kv => kv.Value.Median);
                return Results.Where(kv => kv.Value.Median == maxValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> HighestStandardDeviation
        {
            get
            {
                double maxValue = Results.Max(kv => kv.Value.StandardDeviation);
                return Results.Where(kv => kv.Value.StandardDeviation == maxValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> LowestMean
        {
            get
            {
                double minValue = Results.Min(kv => kv.Value.Mean);
                return Results.Where(kv => kv.Value.Mean == minValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> LowestMedian
        {
            get
            {
                double minValue = Results.Min(kv => kv.Value.Median);
                return Results.Where(kv => kv.Value.Median == minValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> LowestStandardDeviation
        {
            get
            {
                double minValue = Results.Min(kv => kv.Value.StandardDeviation);
                return Results.Where(kv => kv.Value.StandardDeviation == minValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> BestPossibleOutcome
        {
            get
            {
                double maxValue = Results.Max(kv => kv.Value.Maximum);
                return Results.Where(kv => kv.Value.Maximum == maxValue).First();
            }
        }
        public KeyValuePair<string, SimulationResults> WorstPossibleOutcome
        {
            get
            {
                double minValue = Results.Min(kv => kv.Value.Minimum);
                return Results.Where(kv => kv.Value.Minimum == minValue).First();
            }
        }


        // should we have enum and function for returning a specific summary statistic?


        // constructor
        public SensitivitySimulationResults(Dictionary<string, SimulationResults> results, IParameter[] parameters, Expression expression, int numberOfSimulations, bool exhaustive, bool multithreaded)
        {
            this.Results = results;
            this.Parameters = parameters;
            this.Expression = expression;
            this.NumberOfSimulations = numberOfSimulations;
            this.Exhaustive = exhaustive;
            this.MultiThreaded = multithreaded;
        }


        // methods
        private void Resimulate()
        {
            // need to redo the entire simulation when number of results could change
            Simulation newBaseSimulation = new Simulation(_expression, _parameters);

            if (_exhaustive)
            {
                ExhaustiveSensitivitySimulation newSensitivitySimulation = new ExhaustiveSensitivitySimulation(newBaseSimulation);

                if (_multithreaded)
                {
                    SensitivitySimulationResults newResults = newSensitivitySimulation.Simulate_Multithreaded(_numberOfSimulations);
                    this.Results = newResults.Results;
                }
                else // use single thread
                {
                    SensitivitySimulationResults newResults = newSensitivitySimulation.Simulate(_numberOfSimulations);
                    this.Results = newResults.Results;
                }
            }
            else // not exhaustive... uses set approach
            {
                SensitivitySimulation newSensitivitySimulation = new SensitivitySimulation(newBaseSimulation);
                SensitivitySimulationResults newResults = newSensitivitySimulation.Simulate(_numberOfSimulations);
                this.Results = newResults.Results;
            }
        }

        /// <summary>
        /// Computes a new expression using the data generated by the previously run simulation. This function can be used to do limited sensitivity analysis on the output without the overhead of recomputing all parameters.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <returns>SimulationResults containing an array of doubles and summary statistics.</returns>
        public SensitivitySimulationResults RecomputeExpression(string expression)
        {
            return RecomputeExpression(Infix.ParseOrThrow(expression)); // returns a reference, not a copy
        }
        /// <summary>
        /// Computes a new expression using the data generated by the previously run simulation. This function can be used to do limited sensitivity analysis on the output without the overhead of recomputing all parameters.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <returns>SimulationResults containing an array of doubles and summary statistics.</returns>
        internal SensitivitySimulationResults RecomputeExpression(Expression expression)
        {
            foreach (var result in Results)
                result.Value.Expression = expression;

            this.Expression = expression;
            return this; // returns a reference, not a copy
        }

        /// <summary>
        /// Adds a new parameter to the raw data that has already been simulated. Once a new parameter has been added, a new expression that incporates the parameter may be provided.
        /// </summary>
        /// <param name="parameter">New parameter to add to raw data.</param>
        public void AddParameter(IParameter parameter)
        {
            // add parameter to parent simulation (this)
            IParameter[] newParameters = new IParameter[_parameters.Length + 1];
            _parameters.CopyTo(newParameters, 0);
            newParameters[newParameters.GetUpperBound(0)] = parameter;
            this.Parameters = newParameters;

            // add parameter to all children or resimulate, if necessary
            if (parameter is PrecomputedParameter)
            {
                Resimulate();
            }
            else
            {
                foreach (var result in Results)
                    result.Value.AddParameter(parameter);
            }
        }
        /// <summary>
        /// Removes an existing parameter from the raw data that has already been simulated. The existing parameter cannot currently be part of the expression or this will fail.
        /// </summary>
        /// <param name="parameter">Parameter to remove from the existing raw data.</param>
        public void RemoveParameter(IParameter parameter)
        {
            // validation -- needs to occur here even though it happens at lower level, otherwise we could have an unexpected failure when Resimulating here
            if (_parameters.Contains(parameter) == false)
                throw new InvalidParameterException($"{parameter.Name} was not used as a parameter in this simulation.");

            if (Infix.Format(_expression).Contains(parameter.Name))
                throw new ParameterInExpressionException($"Cannot remove {parameter.Name} because it is currently being used in the expression. Provide a new expression that does not use parameter before trying to remove it.");

            // remove parameter from parent results (this)
            IParameter[] newParameters = new IParameter[_parameters.Length - 1];
            bool skipped = false;

            for (int i = 0; i < _parameters.Length; i++)
            {
                if (_parameters[i].Name == parameter.Name)
                {
                    skipped = true;
                    continue;
                }
                else
                {
                    newParameters[skipped ? i - 1 : i] = _parameters[i];
                }
            }

            this.Parameters = newParameters;

            // remove from all children or resimulate, if necessary
            if (parameter is PrecomputedParameter)
            {
                Resimulate();
            }
            else
            {
                foreach (var result in Results)
                    result.Value.RemoveParameter(parameter);
            }

        }
        /// <summary>
        /// Replaces the old parameter with a new parameter, regenerates data for only the changed parameter, and recomputes the results using the rest of the existing data.
        /// </summary>
        /// <param name="oldParameter">Old parameter that should be repalced.</param>
        /// <param name="newParameter">New parameter that should be used in place of the old parameter. If the name does not match the name of the old parameter, the expression will be updated with the new parameter name.</param>
        /// <returns>DependentSimulationResults containing results of the new simulation.</returns>
        public SensitivitySimulationResults ReplaceParameter(IParameter oldParameter, IParameter newParameter)
        {
            // validate that the parameter exists
            if (oldParameter is PrecomputedParameter || newParameter is PrecomputedParameter)
            {
                bool notFound = true;

                for (int p = 0; p < _parameters.Length; p++)
                {
                    if (_parameters[p] == oldParameter)
                    {
                        // replace the parameter
                        notFound = false;
                        _parameters[p] = newParameter;

                        // update the expression stored here, because that is what will be used by Resimulate
                        if (Infix.Format(_expression).Contains(newParameter.Name) == false)
                        {
                            string oldExpression = Infix.Format(_expression);
                            oldExpression = oldExpression.Replace(oldParameter.Name, newParameter.Name);
                            this.Expression = Infix.ParseOrThrow(oldExpression);
                        }

                        break;
                    }
                }

                if (notFound)
                {
                    throw new InvalidParameterException($"{oldParameter.Name} was not used as a parameter in this simulation.");
                }
                else
                {
                    // need to redo the entire simulation because number of results could change
                    Resimulate();
                }
            }
            else
            {
                // new parameter will not affect which scenarios are computed
                foreach (var result in Results)
                    result.Value.ReplaceParameter(oldParameter, newParameter);
            }

            return this;
        }

        /// <summary>
        /// Re-runs the simulations used to generate this SensitivitySimulationResults object with the same number of simulations that was original used. If parameters have been added or removed, the new simulation will account for this. If the original simulation was multithreaded, this method will be as well.
        /// </summary>
        /// <returns>SensitivitySimulationResults containing results of new simulation.</returns>
        public SensitivitySimulationResults Regenerate()
        {
            if (_multithreaded)
            {
                List<Task> simulationTasks = new List<Task>();

                foreach (var result in Results)
                {
                    Task t = Task.Run(() =>
                    {
                        result.Value.Regenerate();
                    });

                    simulationTasks.Add(t);
                }

                Task.WaitAll(simulationTasks.ToArray());
            }
            else
            {
                foreach (var result in Results)
                    result.Value.Regenerate();
            }

            return this;
        }
        /// <summary>
        /// Re-runs the simulations used to generate this SensitivitySimulationResults object with the specified number of simulation. If parameters have been added or removed, the new simulation will account for this. If the original simulation was multithreaded, this method will be as well.
        /// </summary>
        /// <returns>SensitivitySimulationResults containing results of new simulation.</returns>
        public SensitivitySimulationResults Regenerate(int numberOfSimulations)
        {
            if (_multithreaded)
            {
                List<Task> simulationTasks = new List<Task>();

                foreach (var result in Results)
                {
                    int localNumberSimulations = numberOfSimulations;

                    Task t = Task.Run(() =>
                    {
                        result.Value.Regenerate(localNumberSimulations);
                    });

                    simulationTasks.Add(t);
                }

                Task.WaitAll(simulationTasks.ToArray());
            }
            else
            {
                foreach (var result in Results)
                    result.Value.Regenerate(numberOfSimulations);
            }

            return this;
        }
    }

    #endregion

    #region Qualitative

    public class QualitativeSimulation
    {
        public IQualitativeParameter QualitativeParameter { get; set; }

        /// <summary>
        /// A simulation that will return the string value corresponding to the outcome selected from the exhaustive set of possible outcomes for each simulation or from the interpretation of the outcomes of the underlying quantitative simulation.
        /// </summary>
        /// <param name="qualitativeParameter">A single parameter that determines how the quantitative simulation should select an outcome.</param>
        public QualitativeSimulation(IQualitativeParameter qualitativeParameter)
        {
            this.QualitativeParameter = qualitativeParameter;
        }
        /// <summary>
        /// A simulation that will return the string value corresponding to the outcome selected from the exhaustive set of possible outcomes for each simulation.
        /// </summary>
        /// <param name="possibleOutcomes">The exhaustive set of possible outcomes. Cumulative probability must equal 100%.</param>
        public QualitativeSimulation(QualitativeOutcome[] possibleOutcomes)
        {
            this.QualitativeParameter = new QualitativeParameter("DiscreteOutcomes", possibleOutcomes);
        }

        /// <summary>
        /// Performs the given number of simulations using the provided parameters and expressions. Results will be an array of strings with one result per simulation.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>QualitativeSimulationResults containing an array of strings and summary statistics.</returns>
        public QualitativeSimulationResults Simulate(int numberOfSimulations)
        {
            string[] qualitativeResults = new string[numberOfSimulations];

            #region Qualitative Parameter (exhaustive outcomes)

            if (QualitativeParameter is QualitativeParameter)
            {
                // generate array of values between 0 and 1
                double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new ContinuousUniform(0d, 1d));

                // go through array of values and use cumulative probability of outcomes to determine the value for each simulation
                for (int s = 0; s < numberOfSimulations; s++)
                {
                    QualitativeOutcome[] possibleOutcomes = (QualitativeParameter as QualitativeParameter).PossibleOutcomes;

                    for (int o = 0; o < possibleOutcomes.Length; o++)
                    {
                        if (randomProbabilities[s] <= possibleOutcomes[o]._cumulativeProbability)
                        {
                            qualitativeResults[s] = possibleOutcomes[o].Value;
                            break;
                        }
                    }
                }
            }

            #endregion

            #region Conditional Parameter (based on underlying simulation)

            else if (QualitativeParameter is QualitativeConditionalParameter)
            {
                // get results of underlying simulation
                Simulation underlyingSimulation = new Simulation(expression: (QualitativeParameter as QualitativeConditionalParameter).ReferenceParameter.Name, 
                                                                 parameters: (QualitativeParameter as QualitativeConditionalParameter).ReferenceParameter);

                double[] underlyingResults = underlyingSimulation.Simulate(numberOfSimulations).Results;

                // interpret results
                QualitativeConditionalOutcome[] conditionalOutcomes = (QualitativeParameter as QualitativeConditionalParameter).ConditionalOutcomes;

                for (int s = 0; s < underlyingResults.Length; s++)
                {
                    bool valueAssigned = false;

                    for (int c = 0; c < conditionalOutcomes.Length; c++)
                    {
                        switch (conditionalOutcomes[c].ComparisonOperator)
                        {
                            case ComparisonOperator.Equal:
                                if (underlyingResults[s] == conditionalOutcomes[c].ConditionalValue)
                                {
                                    qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                    valueAssigned = true;
                                }
                                break;
                            case ComparisonOperator.NotEqual:
                                if (underlyingResults[s] != conditionalOutcomes[c].ConditionalValue)
                                {
                                    qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                    valueAssigned = true;
                                }
                                break;
                            case ComparisonOperator.LessThan:
                                if (underlyingResults[s] < conditionalOutcomes[c].ConditionalValue)
                                {
                                    qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                    valueAssigned = true;
                                }
                                break;
                            case ComparisonOperator.LessThanOrEqual:
                                if (underlyingResults[s] <= conditionalOutcomes[c].ConditionalValue)
                                {
                                    qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                    valueAssigned = true;
                                }
                                break;
                            case ComparisonOperator.GreaterThan:
                                if (underlyingResults[s] > conditionalOutcomes[c].ConditionalValue)
                                {
                                    qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                    valueAssigned = true;
                                }
                                break;
                            case ComparisonOperator.GreaterThanOrEqual:
                                if (underlyingResults[s] >= conditionalOutcomes[c].ConditionalValue)
                                {
                                    qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                    valueAssigned = true;
                                }
                                break;
                            default:
                                break;
                        }

                        if (valueAssigned)
                            break;
                    }

                    if (!valueAssigned)
                        qualitativeResults[s] = (QualitativeParameter as QualitativeConditionalParameter).DefaultValue;
                }
            }

            #endregion

            #region Random Bag Parameter

            else if (QualitativeParameter is QualitativeRandomBagParameter)
            {
                QualitativeRandomBagParameter randomBag = (QualitativeRandomBagParameter)QualitativeParameter;
                if (randomBag.IsEmpty) throw new EmptyBagException($"QualitativeRandomBag parameters must contain at least one object before they can be used in a simulation.");
                string[] selections = new string[numberOfSimulations];

                if (randomBag.ReplacementRule == RandomBagReplacement.AfterEachPick)
                {
                    string[] contents = randomBag.ContentsToArray();
                    int[] selectedIndices = new int[numberOfSimulations];
                    DiscreteUniform distribution = new DiscreteUniform(0, randomBag.NumberOfItems - 1);
                    distribution.Samples(selectedIndices);

                    for (int i = 0; i < numberOfSimulations; i++)
                        selections[i] = contents[selectedIndices[i]];
                }
                else if (randomBag.ReplacementRule == RandomBagReplacement.Never)
                {
                    // validate we can choose the correct number of items
                    if (randomBag.NumberOfItems < numberOfSimulations)
                        throw new RandomBagItemCountException($"{numberOfSimulations} selections were requested from a bag containing {randomBag.NumberOfItems} items. This is not possible when the replacement rule is set to never.");

                    // choose without replacement
                    string[] contents = randomBag.ContentsToArray();
                    int[] selectionOrder = Enumerable.Range(0, randomBag.NumberOfItems).ToArray();
                    selectionOrder.Shuffle();

                    for (int i = 0; i < numberOfSimulations; i++)
                        selections[i] = contents[selectionOrder[i]];
                }
                else if (randomBag.ReplacementRule == RandomBagReplacement.WhenEmpty)
                {
                    string[] contents = randomBag.ContentsToArray();
                    int numberOfRefills = (numberOfSimulations / randomBag.NumberOfItems) - (numberOfSimulations % randomBag.NumberOfItems == 0 ? 1 : 0);
                    int[] selectionOrder = new int[(numberOfRefills + 1) * numberOfSimulations];

                    // simulate refilling the bag to get enough indices to satisfy the number of simulations
                    for (int i = 0; i < numberOfRefills + 1; i++)
                    {
                        int[] roundSelectionOrder = Enumerable.Range(0, randomBag.NumberOfItems).ToArray();
                        roundSelectionOrder.Shuffle();

                        for (int j = 0; j < roundSelectionOrder.Length; j++)
                            selectionOrder[j + (i * randomBag.NumberOfItems)] = roundSelectionOrder[j];
                    }

                    for (int i = 0; i < numberOfSimulations; i++)
                        selections[i] = contents[selectionOrder[i]];
                }
                else
                {
                    throw new RandomBagReplacementRuleException($"{randomBag.ReplacementRule} is not a valid replacement rule.");
                }

                qualitativeResults = selections;
            }

            #endregion

            #region Invalid Parameter

            else
            {
                throw new InvalidParameterException($"{QualitativeParameter.Name} is not a valid qualitative parameter.");
            }

            #endregion

            // return results
            return new QualitativeSimulationResults(QualitativeParameter, qualitativeResults);
        }
    }

    public class QualitativeSimulationResults
    {
        // public backing variables
        private string[] _results;


        // public copy of possible outcomes and results
        public IQualitativeParameter QualitativeParameter { get; private set; }
        public string[] Results
        {
            get
            {
                string[] returnArray = new string[_results.Length];
                _results.CopyTo(returnArray, 0);
                return returnArray;
            }
            private set
            {
                _results = value;
            }
        }


        // public summary statistics
        public int NumberOfSimulations
        {
            get
            {
                return Results.Length;
            }
        }
        public QualitativeSimulationOutcome[] IndividualOutcomes
        {
            get
            {
                #region Qualitative Parameter

                if (QualitativeParameter is QualitativeParameter)
                {
                    // all possible results are available because PossibleOutcomes is an exhaustive collection
                    string[] possibleResults = (from QualitativeOutcome po in (QualitativeParameter as QualitativeParameter).PossibleOutcomes
                                                select po.Value).OrderBy(po => po).ToArray();

                    QualitativeSimulationOutcome[] individualOutcomes = new QualitativeSimulationOutcome[possibleResults.Length];

                    for (int i = 0; i < possibleResults.Length; i++)
                    {
                        int count = (from r in Results
                                     where r == possibleResults[i]
                                     select r).Count();

                        double percentage = Convert.ToDouble(count) / Convert.ToDouble(Results.Length);

                        individualOutcomes[i] = new QualitativeSimulationOutcome(possibleResults[i], count, percentage);
                    }

                    return individualOutcomes;
                }

                #endregion

                #region Qualitative Conditional Parameter

                else if (QualitativeParameter is QualitativeConditionalParameter)
                {
                    // conditional outcomes is not necessarily exhaustive
                    string[] possibleResults_WithoutDefault = (from QualitativeConditionalOutcome co in (QualitativeParameter as QualitativeConditionalParameter).ConditionalOutcomes
                                                               select co.ReturnValue).OrderBy(po => po).ToArray();

                    // must add the default value to list of possible results to make it exhaustive
                    string[] possibleResults = new string[possibleResults_WithoutDefault.Length + 1];
                    possibleResults_WithoutDefault.CopyTo(possibleResults, 0);
                    possibleResults[possibleResults.GetUpperBound(0)] = (QualitativeParameter as QualitativeConditionalParameter).DefaultValue;
                    QualitativeSimulationOutcome[] individualOutcomes = new QualitativeSimulationOutcome[possibleResults.Length];

                    for (int i = 0; i < possibleResults.Length; i++)
                    {
                        int count = (from r in Results
                                     where r == possibleResults[i]
                                     select r).Count();

                        double percentage = Convert.ToDouble(count) / Convert.ToDouble(Results.Length);

                        individualOutcomes[i] = new QualitativeSimulationOutcome(possibleResults[i], count, percentage);
                    }

                    return individualOutcomes;
                }

                #endregion

                #region Qualitative Random Bag Parameter

                else if (QualitativeParameter is QualitativeRandomBagParameter)
                {
                    // all possible results are available because dictionary keys is an exhaustive collection
                    string[] possibleResults = (from kv in (QualitativeParameter as QualitativeRandomBagParameter).Contents
                                                select kv.Key).OrderBy(k => k).ToArray();

                    QualitativeSimulationOutcome[] individualOutcomes = new QualitativeSimulationOutcome[possibleResults.Length];

                    for (int i = 0; i < possibleResults.Length; i++)
                    {
                        int count = (from r in Results
                                     where r == possibleResults[i]
                                     select r).Count();

                        double percentage = Convert.ToDouble(count) / Convert.ToDouble(Results.Length);

                        individualOutcomes[i] = new QualitativeSimulationOutcome(possibleResults[i], count, percentage);
                    }

                    return individualOutcomes;
                }

                #endregion

                #region Invalid Parameter

                else
                {
                    throw new InvalidParameterException($"{QualitativeParameter.Name} is not a valid IQualitativeParameter. Valid types are QualitativeParameter and QualitativeConditionalParameter.");
                }

                #endregion
            }
        }
        public QualitativeSimulationOutcome MostCommonOutcome
        {
            get
            {
                int maxValue = IndividualOutcomes.Max(o => o.Count);
                return IndividualOutcomes.Where(o => o.Count == maxValue).First();
            }
        }
        public QualitativeSimulationOutcome LeastCommonOutcome
        {
            get
            {
                int minValue = IndividualOutcomes.Min(o => o.Count);
                return IndividualOutcomes.Where(o => o.Count == minValue).First();
            }
        }


        // summary methods
        /// <summary>
        /// Returns the QualitativeSimulationOutcome object associated with one of the possible outcomes of the simulation that generated these results.
        /// </summary>
        /// <param name="outcome">Name of the outcome for which to return information.</param>
        /// <returns></returns>
        public QualitativeSimulationOutcome GetOutcome(string outcome)
        {
            // try catch is faster than validation in successful cases if we're just going to throw the error anyways
            try { return IndividualOutcomes.Where(o => o.Outcome == outcome).First(); }
            catch { throw new NonExistentOutcomeException($"{outcome} is not one the possible outcomes in this results set."); }
        }


        // constructor
        public QualitativeSimulationResults(IQualitativeParameter qualitativeParameter, string[] results)
        {
            this.QualitativeParameter = qualitativeParameter;
            this.Results = results;
        }


        // methods
        /// <summary>
        /// Re-runs the simulation used to generate this QualitativeSimulationResults object with the same number of simulations that was original used.
        /// </summary>
        /// <returns>QualitativeSimulationResults containing results of new simulation.</returns>
        public QualitativeSimulationResults Regenerate()
        {
            return Regenerate(NumberOfSimulations);
        }
        /// <summary>
        /// Re-runs the simulation used to generate this QualitativeSimulationResults object with the specified number of simulation.
        /// </summary>
        /// <returns>QualitativeSimulationResults containing results of new simulation.</returns>
        public QualitativeSimulationResults Regenerate(int numberOfSimulations)
        {
            QualitativeSimulation newSimulation = new QualitativeSimulation(QualitativeParameter);
            this.Results = newSimulation.Simulate(numberOfSimulations).Results;
            return this;
        }
        /// <summary>
        /// Replaces the old parameter with a new parameter and then regenerates the entire simulation.
        /// </summary>
        /// <param name="qualitativeParameter">The new parameter to use for the simulation..</param>
        /// <returns>QualitativeSimulationResults containing results of the new simulation.</returns>
        public QualitativeSimulationResults ReplaceParameter(IQualitativeParameter qualitativeParameter)
        {
            this.QualitativeParameter = qualitativeParameter;
            QualitativeSimulation newSimulation = new QualitativeSimulation(qualitativeParameter);
            this.Results = newSimulation.Simulate(NumberOfSimulations).Results;
            return this;
        }
    }

    #endregion

    #region Binomial Tree

    public class BinomialTreeSimulation
    {
        public double StartValue { get; set; }
        public IParameter VolatilityParameter { get; set; }
        public double UpProbability { get; set; }

        /// <summary>
        /// A simulation that will create a node tree where the nodes in each period are increased and decreased by the volatility parameter to generate the nodes for the next period.
        /// </summary>
        /// <param name="startValue">The value of the root node (period 0) before the volatility parameter has been applied.</param>
        /// <param name="volatilityParameter">The parameter that determines the volatility used to generate the next period of nodes. Generally a constant value, but it is possible to use a parameter that causes volatility to change over time.</param>
        /// <param name="upProbability">The probability of the next move being in an upward direction (value * [1 + volatility]) in each period.</param>
        public BinomialTreeSimulation(double startValue, IParameter volatilityParameter, double upProbability = 0.50d)
        {
            if (upProbability < 0.0d || upProbability > 1.0d)
                throw new InvalidProbabilityException($"Up probability of {upProbability} is not between 0 and 100%.");

            this.StartValue = startValue;
            this.VolatilityParameter = volatilityParameter;
            this.UpProbability = upProbability;
        }

        /// <summary>
        /// Generates a tree with the given number of periods using the StartValue and VolatilityParameter. NOTE: If volatility parameter is not constant, this will result in 2^period nodes being generated each period and may cause memory overflows with large period numbers.
        /// </summary>
        /// <param name="numberOfPeriods">Number of periods for which to generate node tree.</param>
        /// <returns>BinomialTreeSimulationResults containing the node tree and summary statistics.</returns>
        public BinomialTreeSimulationResults Simulate(int numberOfPeriods)
        {
            double[] volatility = new Simulation(VolatilityParameter.Name, VolatilityParameter).Simulate(numberOfPeriods + 1).Results;
            List<BinomialTreeNode> flattenedNodeTree = new List<BinomialTreeNode>();
            List<BinomialTreeNode> previousGeneration = new List<BinomialTreeNode>();
            flattenedNodeTree.Add(new BinomialTreeNode(StartValue, this.UpProbability, volatility[0], BinomialTreeMove.Root));
            previousGeneration.Add(flattenedNodeTree[0]);           

            for (int p = 0; p < numberOfPeriods; p++)
            {
                // create new generation
                List<BinomialTreeNode> newGeneration = new List<BinomialTreeNode>();

                foreach (BinomialTreeNode node in previousGeneration)
                {
                    // up node
                    BinomialTreeNode upNode = new BinomialTreeNode(node.Value * (1 + volatility[p]), this.UpProbability, volatility[p + 1], BinomialTreeMove.Up);
                    
                    if (newGeneration.Count > 0)
                    {
                        if (newGeneration.Last().Value < upNode.Value - 0.0001 || newGeneration.Last().Value > upNode.Value + 0.0001)
                        {
                            // if this node doesn't exist, add it
                            newGeneration.Add(upNode);
                            node.AddChild(upNode);
                        }
                        else
                        {
                            // up and down movements can converage and overlap for adjacent nodes whe volatility is the same as the previous period
                            // in these cases, nodes should have two parents
                            // there is a slight phenomena where value will trend slightly down over time (100 * 1.01 * 0.99 != 100)
                            newGeneration.Last().AddParent(node);
                        }
                    }
                    else
                    {
                        newGeneration.Add(upNode);
                        node.AddChild(upNode);
                    }

                    // down node
                    BinomialTreeNode downNode = new BinomialTreeNode(node.Value * (1 - volatility[p]), this.UpProbability, volatility[p + 1], BinomialTreeMove.Down);
                    newGeneration.Add(downNode);
                    node.AddChild(downNode);
                }

                // merge generations
                foreach (var newNode in newGeneration)
                    flattenedNodeTree.Add(newNode);

                previousGeneration = newGeneration;
            }

            return new BinomialTreeSimulationResults(flattenedNodeTree[0], flattenedNodeTree, numberOfPeriods);
        }
    }

    public class BinomialTreeSimulationResults
    {
        // properties
        public BinomialTreeNode RootNode { get; private set; }
        public List<BinomialTreeNode> FlattenedNodeTree { get; set; }


        // summary statistics
        public int NumberOfPeriods { get; private set; }
        public int NumberOfNodes
        {
            get
            {
                return FlattenedNodeTree.Count;
            }
        }
        public double MaximumEndingValue
        {
            get
            {
                return this.GetPeriod(NumberOfPeriods).Max(n => n.Value);
            }
        }
        public double MinimumEndingValue
        {
            get
            {
                return this.GetPeriod(NumberOfPeriods).Min(n => n.Value);
            }
        }
        public List<BinomialTreeNode> LastPeriod
        {
            get
            {
                return this.GetPeriod(NumberOfPeriods);
            }
        }
        public double ExpectedValue
        {
            get
            {
                double expectedValue = 0.0d;

                foreach (var node in this.LastPeriod)
                    expectedValue += node.Value * node.CumulativeProbability;

                return expectedValue;
            }
        }


        // summary methods
        /// <summary>
        /// Returns a list of nodes from the node tree that comprise the entirety of possible nodes for that period.
        /// </summary>
        /// <param name="period">Period for which to return list of nodes.</param>
        /// <returns></returns>
        public List<BinomialTreeNode> GetPeriod(int period)
        {
            return (from node in FlattenedNodeTree
                    where node.Level == period
                    select node).ToList();
        }
        

        // constructor
        public BinomialTreeSimulationResults(BinomialTreeNode rootNode, List<BinomialTreeNode> flattedNodeTree, int numberOfPeriods)
        {
            this.RootNode = rootNode;
            this.FlattenedNodeTree = flattedNodeTree;
            this.NumberOfPeriods = numberOfPeriods;
        }


        // methods
    }

    #endregion

    #region Binomial Option Pricing

    public class BinomialOptionPricingSimulation
    {
        // private back variables
        private int _dayCountConvention = 360;
        private int _numberOfPeriods = 0;


        // properties
        public double StartValueOfUnderlying { get; set; }
        public IParameter ImpliedVolatility { get; set; }
        public double UpProbability { get; set; }
        public int DaysPerPeriod { get; set; }
        public int DayCountConvention
        {
            get
            {
                if (this.OptionContract != null)
                {
                    return this.OptionContract.DayCountConvention;
                }
                else
                {
                    return _dayCountConvention;
                }
            }
            set
            {
                if (this.OptionContract != null)
                {
                    this.OptionContract.DayCountConvention = value; ;
                }
                else
                {
                    _dayCountConvention = value; ;
                }
            }
        }
        public int NumberOfPeriods
        {
            get
            {
                if (this.OptionContract != null)
                {
                    return (OptionContract.TradingDaysRemaining / DaysPerPeriod) + (OptionContract.TradingDaysRemaining % DaysPerPeriod == 0 ? 0 : 1);
                }
                else
                {
                    return _numberOfPeriods;
                }
            }
            set
            {
                _numberOfPeriods = value;
            }
        }
        public OptionContract OptionContract { get; set; }


        // constructors
        /// <summary>
        /// A simulation that will create a node tree of the price of the underlying instrument at the end of each period using the CRR method. The nodes will not contain option pricing information until an option is added to the simulation/results.
        /// </summary>
        /// <param name="startValueOfUnderlying">The value of the underlying at time 0, before any moves have been made.</param>
        /// <param name="impliedVolatility">The parameter describing the implied volatility of the underlying. Implied volatility should be given on a yearly-basis (using the day count convention) but will be reassessed in each period (where the reassessed value is still a yearly volatility) using the volatility parameter.</param>
        /// <param name="upProbability">The probability of the next move being in an upward direction (value * [1 + volatility]) in each period.</param>
        /// <param name="daysPerPeriod">The number of days that should pass between each period of nodes being calculated.</param>
        /// <param name="dayCountConvention">The day count convention of the underlying instrument.</param>
        public BinomialOptionPricingSimulation(double startValueOfUnderlying, IParameter impliedVolatility, int numberOfPeriods, double upProbability = 0.50, int daysPerPeriod = 1, int dayCountConvention = 360)
        {
            if (upProbability < 0.0d || upProbability > 1.0d)
                throw new InvalidProbabilityException($"Up probability of {upProbability} is not between 0 and 100%.");

            this.StartValueOfUnderlying = startValueOfUnderlying;
            this.ImpliedVolatility = impliedVolatility;
            this.NumberOfPeriods = numberOfPeriods;
            this.UpProbability = upProbability;
            this.DaysPerPeriod = daysPerPeriod;
            this.DayCountConvention = dayCountConvention;
            this.OptionContract = null;
        }
        /// <summary>
        /// A simulation that will create a node tree of the price of the option at the end of each period using the CRR method.
        /// </summary>
        /// <param name="startValueOfUnderlying">The value of the underlying at time 0, before any moves have been made.</param>
        /// <param name="impliedVolatility">The parameter describing the implied volatility of the underlying. Implied volatility should be given on a yearly-basis (using the day count convention) but will be reassessed in each period (where the reassessed value is still a yearly volatility) using the volatility parameter.</param>
        /// <param name="upProbability">The probability of the next move being in an upward direction (value * [1 + volatility]) in each period.</param>
        /// <param name="daysPerPeriod">The number of days that should pass between each period of nodes being calculated.</param>
        /// <param name="dayCountConvention">The day count convention of the underlying instrument.</param>
        public BinomialOptionPricingSimulation(double startValueOfUnderlying, IParameter impliedVolatility, OptionContract optionContract, double upProbability = 0.50, int daysBetweenTrades = 1)
        {
            if (upProbability < 0.0d || upProbability > 1.0d)
                throw new InvalidProbabilityException($"Up probability of {upProbability} is not between 0 and 100%.");

            this.StartValueOfUnderlying = startValueOfUnderlying;
            this.ImpliedVolatility = impliedVolatility;           
            this.UpProbability = upProbability;
            this.DaysPerPeriod = daysBetweenTrades;
            this.OptionContract = optionContract;
        }


        // methods
        /// <summary>
        /// Performs a number of simulations equal to the NumberOfPeriods parameter. Results will be a node tree with summary statistics. Underlying price movement will always be simulated. If an OptionsContract is provided, the contract price will be simulated as well.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>BinomialOptionPricingSimulationResults containing a node tree and summary statistics.</returns>
        public BinomialOptionPricingSimulationResults Simulate()
        {
            // makes the most sense to simulate weekly price movement to avoid issues with trading days
            int numberOfPeriods = this.NumberOfPeriods;

            // simulate the price movement of the underlying, and if we have an options contract, then calculate its values as well
            double[] volatility = new Simulation(ImpliedVolatility.Name, ImpliedVolatility).Simulate(numberOfPeriods + 1).Results;

            // movement factors must be calculated differently than normal binomial node tree
            // u = e ^ (yearlyVolatility * sqrt(daysInPeriod / dayCountConvention))
            // d = 1 / u
            double yearFrac = Math.Sqrt(Convert.ToDouble(this.DaysPerPeriod) / Convert.ToDouble(this.DayCountConvention));
            double periodicVolatility = volatility[0] * yearFrac;
            double upFactor = Math.Pow(Math.E, periodicVolatility);
            double downFactor = 1 / upFactor;

            List<OptionPricingTreeNode> flattenedNodeTree = new List<OptionPricingTreeNode>();
            List<OptionPricingTreeNode> previousGeneration = new List<OptionPricingTreeNode>();
            flattenedNodeTree.Add(new OptionPricingTreeNode(DaysPerPeriod, StartValueOfUnderlying, this.UpProbability, periodicVolatility, upFactor, downFactor, BinomialTreeMove.Root, this.OptionContract));
            previousGeneration.Add(flattenedNodeTree[0]);

            for (int p = 0; p < numberOfPeriods; p++)
            {
                // create new generation
                List<OptionPricingTreeNode> newGeneration = new List<OptionPricingTreeNode>();

                foreach (OptionPricingTreeNode node in previousGeneration)
                {
                    // calculate next factors so we can store in node, assign at end
                    double nextPeriodicVolatility = volatility[p + 1] * yearFrac;
                    double nextUpFactor = Math.Pow(Math.E, nextPeriodicVolatility);
                    double nextDownFactor = 1 / nextUpFactor;

                    // up node
                    OptionPricingTreeNode upNode = new OptionPricingTreeNode(DaysPerPeriod, node.UnderlyingPrice * upFactor, this.UpProbability, nextPeriodicVolatility, nextUpFactor, nextDownFactor, BinomialTreeMove.Up, this.OptionContract);

                    if (newGeneration.Count > 0)
                    {
                        if (newGeneration.Last().UnderlyingPrice < upNode.UnderlyingPrice - 0.0001 || newGeneration.Last().UnderlyingPrice > upNode.UnderlyingPrice + 0.0001)
                        {
                            // if this node doesn't exist, add it
                            newGeneration.Add(upNode);
                            node.AddChild(upNode);
                        }
                        else
                        {
                            // up and down movements can converage and overlap for adjacent nodes whe volatility is the same as the previous period
                            // in these cases, nodes should have two parents
                            newGeneration.Last().AddParent(node);
                        }
                    }
                    else
                    {
                        newGeneration.Add(upNode);
                        node.AddChild(upNode);
                    }

                    // down node
                    OptionPricingTreeNode downNode = new OptionPricingTreeNode(DaysPerPeriod, node.UnderlyingPrice * downFactor, this.UpProbability, nextPeriodicVolatility, nextUpFactor, nextDownFactor, BinomialTreeMove.Down, this.OptionContract);
                    newGeneration.Add(downNode);
                    node.AddChild(downNode);

                    // store next up/down factor
                    periodicVolatility = nextPeriodicVolatility;
                    upFactor = nextUpFactor;
                    downFactor = nextDownFactor;
                }

                // merge generations
                foreach (var newNode in newGeneration)
                    flattenedNodeTree.Add(newNode);

                previousGeneration = newGeneration;
            }

            return new BinomialOptionPricingSimulationResults(flattenedNodeTree[0], flattenedNodeTree, numberOfPeriods, this.OptionContract);
        }
    }

    public class BinomialOptionPricingSimulationResults
    {
        // properties
        public OptionPricingTreeNode RootNode { get; private set; }
        public List<OptionPricingTreeNode> FlattenedNodeTree { get; set; }
        public OptionContract OptionContract { get; set; }


        // summary statistics       
        public int NumberOfPeriods { get; private set; }
        public int NumberOfNodes
        {
            get
            {
                return FlattenedNodeTree.Count;
            }
        }
        public List<OptionPricingTreeNode> LastPeriod
        {
            get
            {
                return this.GetPeriod(NumberOfPeriods);
            }
        }
        public double? MaximumEndingPrice_Option
        {
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    return this.GetPeriod(NumberOfPeriods).Max(n => n.IntrinsicOptionPrice);
                }
            }
        }
        public double MaximumEndingPrice_Underlying
        {
            get
            {
                return this.GetPeriod(NumberOfPeriods).Max(n => n.UnderlyingPrice);
            }
        }
        public double? MinimumEndingPrice_Option
        {
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    return this.GetPeriod(NumberOfPeriods).Min(n => n.IntrinsicOptionPrice);
                }
            }
        }
        public double MinimumEndingPrice_Underlying
        {
            get
            {
                return this.GetPeriod(NumberOfPeriods).Min(n => n.UnderlyingPrice);
            }
        }
        public double? ExpectedEndingPrice_Option
        {
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    double expectedValue = 0.0d;

                    foreach (var node in this.LastPeriod)
                        expectedValue += (double)node.IntrinsicOptionPrice * node.CumulativeProbability;

                    return expectedValue;
                }
            }
        }
        public double ExpectedEndingPrice_Underlying
        {
            get
            {
                double expectedValue = 0.0d;

                foreach (var node in this.LastPeriod)
                    expectedValue += node.UnderlyingPrice * node.CumulativeProbability;

                return expectedValue;
            }
        }
        public double? FairValueOfOption
        {
            get
            {
                if (this.OptionContract == null)
                {
                    return null;
                }
                else
                {
                    return this.RootNode.BinomialValue;
                }
            }
        }


        // summary methods
        /// <summary>
        /// Returns a list of nodes from the node tree that comprise the entirety of possible nodes for that period.
        /// </summary>
        /// <param name="period">Period for which to return list of nodes.</param>
        /// <returns></returns>
        public List<OptionPricingTreeNode> GetPeriod(int period)
        {
            return (from node in FlattenedNodeTree
                    where node.Level == period
                    select node).ToList();
        }


        // constructor
        public BinomialOptionPricingSimulationResults(OptionPricingTreeNode rootNode, List<OptionPricingTreeNode> flattedNodeTree, int numberOfPeriods, OptionContract optionContract)
        {
            this.RootNode = rootNode;
            this.FlattenedNodeTree = flattedNodeTree;
            this.NumberOfPeriods = numberOfPeriods;
            this.OptionContract = optionContract;
        }


        // methods
        // replace contract
        // replace violatility parameter
    }

    #endregion
}