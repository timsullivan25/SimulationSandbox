using MathNet.Numerics.Distributions;
using MathNet.Symbolics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefinedSimulationSandBox
{
    class Simulation
    {
        public IParameter[] Parameters { get; private set; }
        public Expression Expression { get; set; }

        /// <summary>
        /// A simulation that will compute an expression using the given/calculated parameter values each time a simulation is computed.
        /// </summary>
        /// <param name="expression">An algebraic expression that uses parameter names as variables.</param>
        /// <param name="parameters">The parameters that contain/will generate values each time the simulation is run so that the expression can be computed to derive a single numerical result for the simulation.</param>
        public Simulation(Expression expression, params IParameter[] parameters)
        {
            this.Parameters = parameters;
            this.Expression = expression;
        }
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

                #region Discrete Parameter

                else if (Parameters[p] is DiscreteParameter)
                {
                    // generate array of values between 0 and 1
                    double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new MathNet.Numerics.Distributions.ContinuousUniform(0d, 1d));
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
                        rawData[p] = samples;
                    }
                    else
                    {
                        // use doubles for continuous distribution
                        double[] samples = new double[numberOfSimulations];
                        ((dynamic)(Parameters[p] as DistributionParameter).Distribution).Samples(samples);
                        rawData[p] = samples;
                    }
                }

                #endregion

                #region Precomputed Parameter

                else if (Parameters[p] is PrecomputedParameter)
                {
                    rawData[p] = (Parameters[p] as PrecomputedParameter).PrecomputedValues;
                }

                #endregion

                #region Simulation Parameter

                else if (Parameters[p] is SimulationParameter)
                {
                    Simulation innerSimulation = (Parameters[p] as SimulationParameter).Simulation;
                    SimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                    rawData[p] = innerResults.Results;
                }

                #endregion
            }

            // return results
            return new SimulationResults(Parameters, rawData, Expression);
        }
    }

    class SimulationResults
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
        public int NumberOfTrials
        {
            get
            {
                return Results.Length;
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
        public SimulationResults RecomputeExpression(Expression expression)
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
        /// Re-runs the simulation used to generate this SimulationResults object with the same number of simulations that was original used. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SimulationResults containing results of new simulation.</returns>
        public SimulationResults Regenerate()
        {
            return Regenerate(((dynamic)_rawData[0]).Length);
        }
        /// <summary>
        /// Re-runs the simulation used to generate this SimulationResults object with the specified number of simulation. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SimulationResults containing results of new simulation.</returns>
        public SimulationResults Regenerate(int numberOfSimulations)
        {
            Simulation newSimulation = new Simulation(_expression, _parameters);
            this.RawData = newSimulation.Simulate(numberOfSimulations).RawData;
            ComputeResults();
            return this;
        }
    }



    class ExhaustiveSensitivitySimulation
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

            return new SensitivitySimulationResults(allSimulationResults);
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

            return new SensitivitySimulationResults(nonConcurrentDictionary);
        }
    }

    class SensitivitySimulation
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

            return new SensitivitySimulationResults(allSimulationResults);
        }
    }

    class SensitivitySimulationResults
    {
        // public properties
        public Dictionary<string, SimulationResults> Results { get; private set; }


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


        // constructor
        public SensitivitySimulationResults(Dictionary<string, SimulationResults> results)
        {
            this.Results = results;
        }


        // methods
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
        public SensitivitySimulationResults RecomputeExpression(Expression expression)
        {
            foreach (var result in Results)
                result.Value.Expression = expression;

            return this; // returns a reference, not a copy
        }

        /// <summary>
        /// Adds a new parameter to the raw data that has already been simulated. Once a new parameter has been added, a new expression that incporates the parameter may be provided.
        /// </summary>
        /// <param name="parameter">New parameter to add to raw data.</param>
        public void AddParameter(IParameter parameter)
        {
            foreach (var result in Results)
                result.Value.AddParameter(parameter);
        }
        /// <summary>
        /// Removes an existing parameter from the raw data that has already been simulated. The existing parameter cannot currently be part of the expression or this will fail.
        /// </summary>
        /// <param name="parameter">Parameter to remove from the existing raw data.</param>
        public void RemoveParameter(IParameter parameter)
        {
            foreach (var result in Results)
                result.Value.RemoveParameter(parameter);
        }

        /// <summary>
        /// Re-runs the simulations used to generate this SensitivitySimulationResults object with the same number of simulations that was original used. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SensitivitySimulationResults containing results of new simulation.</returns>
        public SensitivitySimulationResults Regenerate()
        {
            foreach (var result in Results)
                result.Value.Regenerate();

            return this;
        }
        /// <summary>
        /// Re-runs the simulations used to generate this SensitivitySimulationResults object with the specified number of simulation. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SensitivitySimulationResults containing results of new simulation.</returns>
        public SensitivitySimulationResults Regenerate(int numberOfSimulations)
        {
            foreach (var result in Results)
                result.Value.Regenerate(numberOfSimulations);

            return this;
        }
        /// <summary>
        /// Use multiple threads to re-run the simulations used to generate this SensitivitySimulationResults object with the same number of simulations that was original used. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SensitivitySimulationResults containing results of new simulation.</returns>
        public SensitivitySimulationResults Regenerate_Multithreaded()
        {
            List<Task> simulationTasks = new List<Task>();

            foreach (var result in Results)
            {
                Task t = Task.Run (() =>
                {
                    result.Value.Regenerate();
                });

                simulationTasks.Add(t);
            }

            Task.WaitAll(simulationTasks.ToArray());
            return this;
        }
        /// <summary>
        /// Use multiple threads to re-run the simulations used to generate this SensitivitySimulationResults object with the specified number of simulation. If parameters have been added or removed, the new simulation will account for this.
        /// </summary>
        /// <returns>SensitivitySimulationResults containing results of new simulation.</returns>
        public SensitivitySimulationResults Regenerate_Multithreaded(int numberOfSimulations)
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
            return this;
        }
    }



    class QualitativeSimulation
    {
        public QualitativeOutcome[] PossibleOutcomes { get; set; }
        public QualitativeSimulationResults Results { get; private set; }

        /// <summary>
        /// A simulation that will return the string value corresponding to outcome selected from the exhaustive set of possible outcomes for each simulation.
        /// </summary>
        /// <param name="possibleOutcomes">Exhaustive set of possible outcomes. Cumulative probability must equal 100%.</param>
        public QualitativeSimulation(params QualitativeOutcome[] possibleOutcomes)
        {
            // handle cumulative probability calculations there since there is not a seperate parameter for qualitative simulations 
            possibleOutcomes[0]._cumulativeProbability = possibleOutcomes[0].Probability;

            for (int po = 1; po < possibleOutcomes.Length; po++)
                possibleOutcomes[po]._cumulativeProbability = possibleOutcomes[po].Probability + possibleOutcomes[po - 1]._cumulativeProbability;

            // check total cumulative probabilities for valid distribution
            if (possibleOutcomes.Last()._cumulativeProbability < 0.99d || possibleOutcomes.Last()._cumulativeProbability > 1.01d)
            {
                throw new InvalidProbabilityException($"Total probability of {possibleOutcomes.Last()._cumulativeProbability} does not equal 100%.");
            }
            else
            {
                // force cumulative probability of last outcome to exactly 100% to avoid rounding errors
                possibleOutcomes.Last()._cumulativeProbability = 1d;
            }

            this.PossibleOutcomes = possibleOutcomes;
            this.Results = null;
        }

        /// <summary>
        /// Performs the given number of simulations using the provided parameters and expressions. Results will be an array of strings with one result per simulation.
        /// </summary>
        /// <param name="numberOfSimulations">Number of simulations to perform.</param>
        /// <returns>QualitativeSimulationResults containing an array of strings and summary statistics.</returns>
        public QualitativeSimulationResults Simulate(int numberOfSimulations)
        {
            string[] discreteResults = new string[numberOfSimulations];
                
            // generate array of values between 0 and 1
            double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new MathNet.Numerics.Distributions.ContinuousUniform(0d, 1d));

            // go through array of values and use cumulative probability of outcomes to determine the value for each simulation
            for (int s = 0; s < numberOfSimulations; s++)
            {
                for (int o = 0; o < PossibleOutcomes.Length; o++)
                {
                    if (randomProbabilities[s] <= PossibleOutcomes[o]._cumulativeProbability)
                    {
                        discreteResults[s] = PossibleOutcomes[o].Value;
                        break;
                    }
                }
            }

            // return results
            return new QualitativeSimulationResults(PossibleOutcomes, discreteResults); 
        }
    }

    class QualitativeSimulationResults
    {
        // private results
        private QualitativeOutcome[] _possibleOutcomes;


        // public copy of possible outcomes and results
        public QualitativeOutcome[] PossibleOutcomes
        {
            get
            {
                QualitativeOutcome[] returnArray = new QualitativeOutcome[_possibleOutcomes.Length];
                _possibleOutcomes.CopyTo(returnArray, 0);
                return returnArray;
            }
            set
            {
                _possibleOutcomes = value;
            }
        }
        public string[] Results { get; set; }


        // public summary statistics
        public int NumberOfTrials
        {
            get
            {
                return Results.Length;
            }
        }
        public Dictionary<string, int> IndividualOutcomes
        {
            get
            {
                Dictionary<string, int> countOfOutcomes = new Dictionary<string, int>();

                foreach (var possibleOutcome in _possibleOutcomes)
                {
                    int count = (from outcome in Results
                                 where outcome == possibleOutcome.Value
                                 select outcome).Count();

                    countOfOutcomes.Add(possibleOutcome.Value, count);
                }

                return countOfOutcomes;
            }
        }
        public KeyValuePair<string, int> MostCommonOutcome
        {
            get
            {
                int maxValue = IndividualOutcomes.Max(kv => kv.Value);
                return IndividualOutcomes.Where(kv => kv.Value == maxValue).First();
            }
        }
        public KeyValuePair<string, int> LeastCommonOutcome
        {
            get
            {
                int minValue = IndividualOutcomes.Min(kv => kv.Value);
                return IndividualOutcomes.Where(kv => kv.Value == minValue).First();
            }
        }


        // constructor
        public QualitativeSimulationResults(QualitativeOutcome[] possibleOutcomes, string[] results)
        {
            this.PossibleOutcomes = possibleOutcomes;
            this.Results = results;
        }


        // methods
        /// <summary>
        /// Re-runs the simulation used to generate this QualitativeSimulationResults object with the same number of simulations that was original used.
        /// </summary>
        /// <returns>QualitativeSimulationResults containing results of new simulation.</returns>
        public QualitativeSimulationResults Regenerate()
        {
            return Regenerate(Results.Length);
        }
        /// <summary>
        /// Re-runs the simulation used to generate this QualitativeSimulationResults object with the specified number of simulation.
        /// </summary>
        /// <returns>QualitativeSimulationResults containing results of new simulation.</returns>
        public QualitativeSimulationResults Regenerate(int numberOfSimulations)
        {
            QualitativeSimulation newSimulation = new QualitativeSimulation(_possibleOutcomes);
            this.Results = newSimulation.Simulate(numberOfSimulations).Results;
            return this;
        }
    }
}
