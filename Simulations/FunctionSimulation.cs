using MathNet.Numerics.Distributions;
using Simulations.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simulations
{
    /// <summary>
    /// Base implementation for FunctionSimulations. Interface is required so that GetParameterValues function can call simulate on any FunctionSimulation to generate values for the current simulation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFunctionSimulation<T>
    {
        T[] Simulate(int numberOfSimulations, bool passthroughIParams = false);
    }

    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T> : IFunctionSimulation<T>
    {
        public Func<T> Function { get; set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        public FunctionSimulation(Func<T> function)
        {
            Function = function;
        }

        /// <summary>
        /// Iterates over the function n number of times.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <param name="passthroughIParams">This has no affect for this version of the simulation, but is necessary to comply with the interface.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        public T[] Simulate(int numberOfSimulations, bool passthroughIParams = false)
        {
            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke();

            return results;
        }
    }

    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T> : IFunctionSimulation<T>
    {
        public Func<T1, T> Function { get; set; }
        public object[] Parameters { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="parameters">Parameters in the order they appear in the function. Any type of object is acceptable so long as it corresponds with the type of the parameter accepted by the function. IParameters, FunctionSimulations, and ListOfInputs have special functionality.</param>
        public FunctionSimulation(Func<T1, T> function, params object[] parameters)
        {
            Function = function;
            Parameters = parameters;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <param name="passthroughIParams">FALSE if IParameters should be evaluated and converted to object arrays. TRUE if IParameters should be passed through as objects to the underlying function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations, bool passthroughIParams = false)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Parameters[0], numberOfSimulations, passthroughIParams);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i]);

            return results;
        }
    }

    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T> Function { get; set; }
        public object[] Parameters { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="parameters">Parameters in the order they appear in the function. Any type of object is acceptable so long as it corresponds with the type of the parameter accepted by the function. IParameters, FunctionSimulations, and ListOfInputs have special functionality.</param>
        public FunctionSimulation(Func<T1, T2, T> function, params object[] parameters)
        {
            Function = function;
            Parameters = parameters;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <param name="passthroughIParams">FALSE if IParameters should be evaluated and converted to object arrays. TRUE if IParameters should be passed through as objects to the underlying function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations, bool passthroughIParams = false)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Parameters[0], numberOfSimulations, passthroughIParams);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Parameters[1], numberOfSimulations, passthroughIParams);          

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i]);

            return results;
        }
    }

    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T> Function { get; set; }
        public object[] Parameters { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="parameters">Parameters in the order they appear in the function. Any type of object is acceptable so long as it corresponds with the type of the parameter accepted by the function. IParameters, FunctionSimulations, and ListOfInputs have special functionality.</param>
        public FunctionSimulation(Func<T1, T2, T3, T> function, params object[] parameters)
        {
            Function = function;
            Parameters = parameters;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <param name="passthroughIParams">FALSE if IParameters should be evaluated and converted to object arrays. TRUE if IParameters should be passed through as objects to the underlying function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations, bool passthroughIParams = false)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Parameters[0], numberOfSimulations, passthroughIParams);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Parameters[1], numberOfSimulations, passthroughIParams);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Parameters[2], numberOfSimulations, passthroughIParams);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i]);

            return results;
        }
    }

    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T> Function { get; set; }
        public object[] Parameters { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="parameters">Parameters in the order they appear in the function. Any type of object is acceptable so long as it corresponds with the type of the parameter accepted by the function. IParameters, FunctionSimulations, and ListOfInputs have special functionality.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T> function, params object[] parameters)
        {
            Function = function;
            Parameters = parameters;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <param name="passthroughIParams">FALSE if IParameters should be evaluated and converted to object arrays. TRUE if IParameters should be passed through as objects to the underlying function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations, bool passthroughIParams = false)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Parameters[0], numberOfSimulations, passthroughIParams);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Parameters[1], numberOfSimulations, passthroughIParams);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Parameters[2], numberOfSimulations, passthroughIParams);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Parameters[3], numberOfSimulations, passthroughIParams);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i]);

            return results;
        }
    }

    /// <summary>
    /// Main and helpers methods for handling different types of simulation inputs.
    /// </summary>
    public static class ParameterParsing
    {
        /// <summary>
        /// Converts any parameter in an array of objects. IParameters will be parsed the same way they are in the StandardSimulation. FunctionSimulations will be simulated the necessary number of times and can be passed without pre-simulating the results. Collections will be passed through as objects unless they are wrapped in a ListOfInputs object, which will instead use each value as an individual input for one simulation.
        /// </summary>
        /// <typeparam name="T">The desired type of the individual parameters. The type of value the function being simulated will accept as input.</typeparam>
        /// <param name="parameter">The parameter from which to generate an array of values.</param>
        /// <param name="numberOfSimulations">The number of values to generate.</param>
        /// <param name="passthroughIParams">Indicates whether IParameters should be converted into arrays of values or passed through as objects to the function.</param>
        /// <returns></returns>
        public static object[] GetParameterValues<T>(object parameter, int numberOfSimulations, bool passthroughIParams = false)
        {
            if (parameter is IParameter iParameter && passthroughIParams == false)
            {
                #region Conditional Parameter

                if (iParameter is ConditionalParameter)
                {
                    Simulation innerSimulation = (iParameter as ConditionalParameter).ReferenceParameter.Simulation;
                    double[] innerResults = innerSimulation.Simulate(numberOfSimulations).Results;
                    // we're saying an integer simulation will be converted to double.. we could just use the math.net floating point numbers to avoid issues here ??
                    // but the conversion doesn't seem to result in floating point comparison issues.. so we might be good?

                    ConditionalOutcome[] conditionalOutcomes = (iParameter as ConditionalParameter).ConditionalOutcomes;
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
                            conditionalResults[s] = (iParameter as ConditionalParameter).DefaultValue;
                    }

                    return UntypeArray(conditionalResults);
                }

                #endregion

                #region Constant Parameter

                else if (iParameter is ConstantParameter)
                {
                    double constant = (iParameter as ConstantParameter).Value;
                    double[] repeatValues = new double[numberOfSimulations];

                    for (int i = 0; i < numberOfSimulations; i++)
                        repeatValues[i] = constant;

                    return UntypeArray(repeatValues);
                }

                #endregion

                #region Dependent Simulation Parameter

                else if (iParameter is DependentSimulationParameter)
                {
                    DependentSimulationParameterReturnType returnType = (iParameter as DependentSimulationParameter).ReturnType;

                    if (returnType == DependentSimulationParameterReturnType.Results)
                    {
                        // if we are looking for results, we only need to run one simulation
                        // constraints cannot apply to the Results return type because each data point is dependent on the last
                        // i.e. we can't change one of them without having to change everything after it...
                        DependentSimulation innerSimulation = (iParameter as DependentSimulationParameter).DependentSimulation;
                        DependentSimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                        return UntypeArray(innerResults.Results);
                    }
                    else
                    {
                        // we need to run a bunch of inner simulations, so use as many threads as possible
                        int summaryRunCount = (iParameter as DependentSimulationParameter).SummaryRunCount;
                        DependentSimulation innerSimulation = (iParameter as DependentSimulationParameter).DependentSimulation;

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
                        if ((iParameter as DependentSimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as DependentSimulationParameter).Constraint;

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
                        return UntypeArray(summaryStatistics);
                    }
                }

                #endregion

                #region Discrete Parameter

                else if (iParameter is DiscreteParameter)
                {
                    // generate array of values between 0 and 1
                    double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new ContinuousUniform(0d, 1d));
                    double[] correspondingValues = new double[numberOfSimulations];

                    // go through array of values and use cumulative probability of outcomes to determine the value for each simulation
                    for (int s = 0; s < numberOfSimulations; s++)
                    {
                        DiscreteOutcome[] possibleOutcomes = (iParameter as DiscreteParameter).PossibleOutcomes;

                        for (int o = 0; o < possibleOutcomes.Length; o++)
                        {
                            if (randomProbabilities[s] <= possibleOutcomes[o]._cumulativeProbability)
                            {
                                correspondingValues[s] = possibleOutcomes[o].Value;
                                break;
                            }
                        }
                    }

                    return UntypeArray(correspondingValues);
                }

                #endregion

                #region Distribution Parameter

                else if (iParameter is DistributionParameter)
                {
                    if ((iParameter as DistributionParameter).Distribution is IDiscreteDistribution)
                    {
                        // must use integers for discrete distributions
                        int[] samples = new int[numberOfSimulations];
                        ((dynamic)(iParameter as DistributionParameter).Distribution).Samples(samples);

                        #region Check Constraints

                        if ((iParameter as DistributionParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as DistributionParameter).Constraint;

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
                                            int newSample = ((dynamic)(iParameter as DistributionParameter).Distribution).Sample();

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

                        return UntypeArray(samples);
                    }
                    else
                    {
                        // use doubles for continuous distribution
                        double[] samples = new double[numberOfSimulations];
                        ((dynamic)(iParameter as DistributionParameter).Distribution).Samples(samples);

                        #region Check Constraints

                        if ((iParameter as DistributionParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as DistributionParameter).Constraint;

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
                                            double newSample = ((dynamic)(iParameter as DistributionParameter).Distribution).Sample();

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

                        return UntypeArray(samples);
                    }
                }

                #endregion

                #region Distribution Function Parameter

                else if (iParameter is DistributionFunctionParameter)
                {
                    try
                    {
                        // compute location values using the underlying location parameter
                        double[] locationValues = new Simulation("location", (iParameter as DistributionFunctionParameter).LocationParameter).Simulate(numberOfSimulations).Results;
                        double[] computeValues = new double[numberOfSimulations];

                        // use distribution, function, and location to compute results                   
                        switch ((iParameter as DistributionFunctionParameter).ReturnType)
                        {
                            case DistributionFunctionParameterReturnType.CumulativeDistribution:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).CumulativeDistribution(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.Density:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).Density(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.DensityLn:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).DensityLn(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.InverseCumulativeDistribution:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).InverseCumulativeDistribution(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.Probability:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).Probability(Convert.ToInt32(locationValues[i]));
                                break;
                            case DistributionFunctionParameterReturnType.ProbabilityLn:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).ProbabilityLn(Convert.ToInt32(locationValues[i]));
                                break;
                            default:
                                break;
                        }

                        return UntypeArray(computeValues);
                    }
                    catch (Exception innerException)
                    {
                        throw new DistributionFunctionFailureException($"Failed to compute {(iParameter as DistributionFunctionParameter).ReturnType} function for {(iParameter as DistributionFunctionParameter).Distribution.GetType()} distribution. This function may not be valid for this type of distribution. Check the MathNet.Numerics.Distributions documentation and the inner exception for more details.", innerException);
                    }
                }

                #endregion

                #region Precomputed Parameter

                else if (iParameter is PrecomputedParameter param)
                {
                    if (param.PrecomputedValues.Length != numberOfSimulations)
                            throw new PrecomputedValueCountException($"{param.Name} has {param.PrecomputedValues.Length} precomputed values but the simulation being run expects {numberOfSimulations} values.");

                    return UntypeArray(param.PrecomputedValues);
                }

                #endregion

                #region Qualitative Interpretation Parameter

                else if (iParameter is QualitativeInterpretationParameter)
                {
                    // simulate the set of qualitative data
                    IQualitativeParameter qualitativeParameter = (iParameter as QualitativeInterpretationParameter).QualitativeParameter;
                    QualitativeSimulation qualitativeSimulation = new QualitativeSimulation(qualitativeParameter);
                    string[] qualitativeOutcomes = qualitativeSimulation.Simulate(numberOfSimulations).Results;

                    // interpret outcomes
                    Dictionary<string, double> interpretationDictionary = (iParameter as QualitativeInterpretationParameter).InterpretationDictionary;
                    double[] interpretedResults = new double[numberOfSimulations];

                    for (int outcome = 0; outcome < numberOfSimulations; outcome++)
                    {
                        if (interpretationDictionary.ContainsKey(qualitativeOutcomes[outcome]))
                        {
                            interpretedResults[outcome] = interpretationDictionary[qualitativeOutcomes[outcome]];
                        }
                        else
                        {
                            interpretedResults[outcome] = (iParameter as QualitativeInterpretationParameter).DefaultValue;
                        }
                    }

                    return UntypeArray(interpretedResults);
                }

                #endregion

                #region Random Bag Parameter

                else if (iParameter is RandomBagParameter)
                {
                    RandomBagParameter randomBag = (RandomBagParameter)iParameter;
                    double[] selections = new double[numberOfSimulations];

                    if (randomBag.IsEmpty)
                        throw new EmptyBagException($"RandomBag parameters must contain at least one object before they can be used in a simulation.");

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

                    return UntypeArray(selections);
                }

                #endregion

                #region Simulation Parameter

                else if (iParameter is SimulationParameter)
                {
                    SimulationParameterReturnType returnType = (iParameter as SimulationParameter).ReturnType;

                    if (returnType == SimulationParameterReturnType.Results)
                    {
                        // if we are looking for results, we only need to run one simulation
                        Simulation innerSimulation = (iParameter as SimulationParameter).Simulation;
                        SimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                        double[] samples = innerResults.Results;

                        #region Check Constraints

                        if ((iParameter as SimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as SimulationParameter).Constraint;

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
                                            double newSample = (iParameter as SimulationParameter).Simulation.Simulate(1).First;

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

                        return UntypeArray(samples);
                    }
                    else
                    {
                        // we need to run a bunch of inner simulations, so use as many threads as possible
                        int summaryRunCount = (iParameter as SimulationParameter).SummaryRunCount;
                        Simulation innerSimulation = (iParameter as SimulationParameter).Simulation;

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

                        if ((iParameter as SimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as SimulationParameter).Constraint;

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
                        return UntypeArray(summaryStatistics);
                    }
                }

                #endregion 

                #region Invalid Parameter

                else
                {
                    throw new InvalidParameterException($"{iParameter.Name} is a {iParameter.GetType()} parameter, which is not one the parameter types that simulations are able to handle. Acceptable parameter types are: Conditional, Constant, Discrete, Distribution, Precomputed, Simulation, DependentSimulation, and QualitativeInterpretation.");
                }

                #endregion
            }
            else if (parameter is ListOfInputs inputs)
            {
                return inputs.Inputs.ToArray();
            }
            else if (parameter is IFunctionSimulation<T> functionSimulation)
            {
                return UntypeArray(functionSimulation.Simulate(numberOfSimulations));
            }
            else 
            {
                return Repeat(parameter, numberOfSimulations);
            }
        }

        /// <summary>
        /// Creates an array with n amount of identical objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="numberOfTimes"></param>
        /// <returns></returns>
        private static T[] Repeat<T>(T obj, int numberOfTimes)
        {
            T[] arr = new T[numberOfTimes];
            for (int i = 0; i < numberOfTimes; i++)
                arr[i] = obj;

            return arr;
        }

        /// <summary>
        /// Converts an array of any type into an array of objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static object[] UntypeArray<T>(T[] arr)
        {
            int length = arr.Length;
            object[] values = new object[length];
            for (int i = 0; i < length; i++)
                values[i] = arr[i];

            return values;
        }
    }  

    /// <summary>
    /// Use this when passing a collection of objects to be used as inputs for each simulation instead of a collection that should be used as an input for every simulation.
    /// </summary>
    public class ListOfInputs
    {
        public ICollection<object> Inputs;

        /// <summary>
        /// Marks a collection of objects as individual inputs to use for each simulation instead of a collection that should be used as an input for every simulation.
        /// </summary>
        /// <param name="inputs"></param>
        public ListOfInputs(ICollection<object> inputs)
        {
            Inputs = inputs;
        }
    }
}
