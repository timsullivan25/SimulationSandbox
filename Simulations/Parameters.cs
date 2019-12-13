using MathNet.Numerics.Distributions;
using MathNet.Symbolics;
using Simulations.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Simulations
{
    #region Interfaces

    public interface IParameter
    {
        string Name { get; set; }
    }

    public interface IQualitativeParameter
    {
        string Name { get; set; }
    }

    #endregion

    #region Quantitative Parameters

    public class ConditionalOutcome
    {
        public ComparisonOperator ComparisonOperator { get; set; }
        public double ConditionalValue { get; set; }
        public double ReturnValue { get; set; }
        public double Tolerance { get; set; }

        /// <summary>
        /// One of the possible conditions that may be met in a ConditionalParameter. The ConditionalValue will be used as the right value in the comparison operation and the ConditionalParameter's ReferenceValue will be used on the left side.
        /// </summary>
        /// <param name="comparisonOperator">Operator to use when comparing the ConditionalValue to the ReferenceValue.</param>
        /// <param name="comparisonValue">Value that the ConditionalParameter's ReferenceValue will be compared against.</param>
        /// <param name="returnValue">Value that will be returned if the conditional operation evaluates to true.</param>
        /// <param name="tolerance">Used when checking equal and not equal operators to overcome issues with floating point number comparisons.</param>
        public ConditionalOutcome(ComparisonOperator comparisonOperator, double comparisonValue, double returnValue, double tolerance = 0.1d)
        {
            this.ComparisonOperator = comparisonOperator;
            this.ConditionalValue = comparisonValue;
            this.ReturnValue = returnValue;
            this.Tolerance = tolerance;
        }
    }

    public class ConditionalParameter : IParameter
    {
        public string Name { get; set; }
        public SimulationParameter ReferenceParameter { get; set; }
        public ConditionalOutcome[] ConditionalOutcomes { get; set; }
        public double DefaultValue { get; set; }

        /// <summary>
        /// A parameter whose value will take the value of the first ConditionalOutcome that evalutes as true, or the default value if none of the outcomes evaluate to true.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="referenceParameter">The parameter whose value will be used on the left side of the comparison operation.</param>
        /// <param name="defaultValue">The value to return if none of the ConditionalOutcomes evaluate to true.</param>
        /// <param name="conditionalOutcomes">Array of conditions to be evaluated in order. The first conditional that evaluates to true with have its ReturnValue used as the ConditionalParameter value for the simulation.</param>
        public ConditionalParameter(string name, IParameter referenceParameter, double defaultValue, params ConditionalOutcome[] conditionalOutcomes)
        {
            this.Name = name;
            this.ConditionalOutcomes = conditionalOutcomes;
            this.DefaultValue = defaultValue;

            // accept any type of reference parameter but force it to become a simulation parameter in order to use recursion inside simulation
            if (referenceParameter is SimulationParameter)
            {
                this.ReferenceParameter = (SimulationParameter)referenceParameter;
            }
            else
            {
                Simulation simulation = new Simulation(referenceParameter.Name, referenceParameter);
                SimulationParameter convertedParameter = new SimulationParameter(referenceParameter.Name, simulation, SimulationParameterReturnType.Results);
                this.ReferenceParameter = convertedParameter;
            }
        }
    }

    public class ConstantParameter : IParameter
    {
        public string Name { get; set; }
        public double Value { get; set; }

        /// <summary>
        /// A parameter whose value will be the same in every simulation.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="value">Value to use for every simulation.</param>
        public ConstantParameter(string name, double value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class DependentSimulationParameter : IParameter
    {
        public string Name { get; set; }
        public DependentSimulation DependentSimulation { get; set; }
        public DependentSimulationParameterReturnType ReturnType { get; set; }
        public int SummaryRunCount { get; set; }
        public ParameterConstraint Constraint { get; set; }

        /// <summary>
        /// A parameter whose values are determined by running an inner simulation the same number of times as the outer simulation. The total number of inner simulations will be equal to numberOfSimulations ^ (#innerSimulations - 1) if you return the results of the simulation. If you choose to return a summary statistics instead, the number of inner simulations will be equal to (numberOfSimulations * innerSimulationRunCount) ^ (#innerSimulations - 1).
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="dependentSimulation">A complete depedent simulation that could be evaluated on its own.</param>
        /// <param name="returnType">The data point that should be returned from each underlying simulation.</param>
        /// <param name="summaryRunCount">The number of times the simulation should be run to generate the summary statistic. Has no affect when return type is set to Results.</param>
        public DependentSimulationParameter(string name, DependentSimulation dependentSimulation, DependentSimulationParameterReturnType returnType = DependentSimulationParameterReturnType.Results, int summaryRunCount = 1000)
        {
            this.Name = name;
            this.DependentSimulation = dependentSimulation;
            this.ReturnType = returnType;
            this.SummaryRunCount = summaryRunCount;
            this.Constraint = null;
        }
        /// <summary>
        /// A parameter whose values are determined by running an inner simulation the same number of times as the outer simulation. The total number of inner simulations will be equal to numberOfSimulations ^ (#innerSimulations - 1) if you return the results of the simulation. If you choose to return a summary statistics instead, the number of inner simulations will be equal to (numberOfSimulations * innerSimulationRunCount) ^ (#innerSimulations - 1).
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="dependentSimulation">A complete depedent simulation that could be evaluated on its own.</param>
        /// <param name="constraint">A ParameterConstraint object containing information about how the simulated values of this parameted should be constrained. This constraint will only be applied if the return type is not Results, because Results samples as dependent upon the previous sample and cannot be changed without regenerating all remaining samples.</param>
        /// <param name="returnType">The data point that should be returned from each underlying simulation.</param>
        /// <param name="summaryRunCount">The number of times the simulation should be run to generate the summary statistic. Has no affect when return type is set to Results.</param>
        public DependentSimulationParameter(string name, DependentSimulation dependentSimulation, ParameterConstraint constraint, DependentSimulationParameterReturnType returnType = DependentSimulationParameterReturnType.EndingValue, int summaryRunCount = 1000)
        {
            this.Name = name;
            this.DependentSimulation = dependentSimulation;
            this.ReturnType = returnType;
            this.SummaryRunCount = summaryRunCount;
            this.Constraint = constraint;
        }
    }

    public class DiscreteOutcome
    {
        internal double _cumulativeProbability; // used to increase speed of simulation by precomputing the comparison values
        private double _probability;

        public double Value { get; set; }
        public double Probability
        {
            get
            {
                return _probability;
            }
            set
            {
                if (value < 0d || value > 1d)
                    throw new InvalidProbabilityException($"Probability of {value} is not between 0 and 1.");

                _probability = value;
            }
        }

        /// <summary>
        /// One of the possible outcomes in a discrete parameter. A discrete outcome must have a probability between 0.0 and 1.0. The sum of the probability of all discrete outcomes must equal 1.0.
        /// </summary>
        /// <param name="value">Value of the discrete parameter when this outcome is selected.</param>
        /// <param name="probability">Probability of this outcome being selected.</param>
        public DiscreteOutcome(double value, double probability)
        {
            this.Value = value;
            this.Probability = probability;
            _cumulativeProbability = _probability; // this value should be reset later on to prevent errors when something reuses these objects
        }
    }

    public class DiscreteParameter : IParameter
    {
        private DiscreteOutcome[] _possibleOutcomes;

        public string Name { get; set; }
        public DiscreteOutcome[] PossibleOutcomes
        {
            get
            {
                return _possibleOutcomes;
            }
            set
            {
                double sumOfProbabilities = 0d;

                for (int outcome = 0; outcome < value.Length; outcome++)
                    sumOfProbabilities += value[outcome].Probability;

                if (sumOfProbabilities < 0.99d || sumOfProbabilities > 1.01d)
                    throw new InvalidProbabilityException($"Total probability of {sumOfProbabilities} does not equal 100%.");

                _possibleOutcomes = value;
            }
        }

        /// <summary>
        /// A parameter where there are a predetermined number of possible outcomes whose probabilities sum to 100%.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="possibleOutcomes">An array of possible outcomes, where each outcome has a probability between 0 and 1 and the cumulative probability of all outcomes is 100%.</param>
        public DiscreteParameter(string name, params DiscreteOutcome[] possibleOutcomes)
        {
            this.Name = name;
            this.PossibleOutcomes = possibleOutcomes;

            // set all cumulative probabilities to make matching the outcome faster duration the simulation      
            _possibleOutcomes[0]._cumulativeProbability = _possibleOutcomes[0].Probability;

            for (int po = 1; po < possibleOutcomes.Length; po++)
                _possibleOutcomes[po]._cumulativeProbability = _possibleOutcomes[po].Probability + _possibleOutcomes[po - 1]._cumulativeProbability;

            // force cumulative probability of last outcome to exactly 100% to avoid rounding errors
            possibleOutcomes.Last()._cumulativeProbability = 1d;
        }
    }

    public class DistributionParameter : IParameter
    {
        public string Name { get; set; }
        public IDistribution Distribution { get; set; }
        public ParameterConstraint Constraint { get; set; }

        /// <summary>
        /// A parameter whose value is sampled from the specified distribution.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="distribution">The distribution from which to sample values. Distribution must have a .Samples method that returns an array of doubles or ints.</param>
        public DistributionParameter(string name, IDistribution distribution)
        {
            this.Name = name;
            this.Distribution = distribution;
            this.Constraint = null;
        }
        /// <summary>
        /// A parameter whose value is sampled from the specified distribution.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="distribution">The distribution from which to sample values. Distribution must have a .Samples method that returns an array of doubles or ints.</param>
        /// <param name="constraint">A ParameterConstraint object containing information about how the simulated values of this parameted should be constrained.</param>
        public DistributionParameter(string name, IDistribution distribution, ParameterConstraint constraint)
        {
            this.Name = name;
            this.Distribution = distribution;
            this.Constraint = constraint;
        }
    }

    public class DistributionFunctionParameter : IParameter
    {
        public string Name { get; set; }
        public IDistribution Distribution { get; set; }
        public DistributionFunctionParameterReturnType ReturnType { get; set; }
        public IParameter LocationParameter { get; set; }

        /// <summary>
        /// A parameter whose value is determined by computing a CDF or PDF function using the location in the distribution generated by the location parameter.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="distribution">The distribution from which to compute the specified function. Distribution must have a corresponding method for the return type.</param>
        /// <param name="returnType">The function that will be computed using the distribution and the location parameter.</param>
        /// <param name="locationParameter">Parameter that will be used to determine the location in the distribution for which to compute the specified function.</param>
        public DistributionFunctionParameter(string name, IDistribution distribution, DistributionFunctionParameterReturnType returnType, IParameter locationParameter)
        {
            this.Name = name;
            this.Distribution = distribution;
            this.ReturnType = returnType;
            this.LocationParameter = locationParameter;
        }
    }

    public class PrecomputedParameter : IParameter
    {
        public string Name { get; set; }
        public FloatingPoint[] PrecomputedValues { get; set; }

        /// <summary>
        /// A parameter whose values have already been computed for every simulation. A simulation using a PrecomputedParameter must run exactly as many simulations as there are PrecomputedValues or it will result in an error.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="precomputedValues">The precomputd values to use for the parameter. There must be one value per simulation and exactly as many values are there are simulations being run.</param>
        public PrecomputedParameter(string name, FloatingPoint[] precomputedValues)
        {
            this.Name = name;
            this.PrecomputedValues = precomputedValues;
        }
    }

    public class QualitativeInterpretationParameter : IParameter
    {
        public string Name { get; set; }
        public IQualitativeParameter QualitativeParameter { get; set; }
        public Dictionary<string, double> InterpretationDictionary { get; set; }
        public double DefaultValue { get; set; }

        /// <summary>
        /// A parameter whose value will be determined by looking up a qualitative value in its InterpretationDictionary. If there are no keys that match the qualitative outcome, the DefaultValue will be return instead.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>'
        /// <param name="qualitativeParameter">A parameter that can be used to create and run a QualitativeSimulation to produce the qualitative values that will be interpretted by the dictionary.</param>
        /// <param name="interpretationDictionary">Dictionary that tells the parameter how to interpret the qualitative values it receives.</param>
        /// <param name="defaultValue">Value to return if there are no matching keys in the dictionary.</param>
        public QualitativeInterpretationParameter(string name, IQualitativeParameter qualitativeParameter, Dictionary<string, double> interpretationDictionary, double defaultValue = 0)
        {
            this.Name = name;
            this.QualitativeParameter = qualitativeParameter;
            this.InterpretationDictionary = interpretationDictionary;
            this.DefaultValue = defaultValue;
        }
        /// <summary>
        /// A parameter whose value will be determined by looking up a qualitative value in its InterpretationDictionary. If there are no keys that match the qualitative outcome, the DefaultValue will be return instead.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>'
        /// <param name="possibleOutcomes">The exhaustive set of possible outcomes. Cumulative probability must equal 100%.</param>
        /// <param name="interpretationDictionary">Dictionary that tells the parameter how to interpret the qualitative values it receives.</param>
        /// <param name="defaultValue">Value to return if there are no matching keys in the dictionary.</param>
        public QualitativeInterpretationParameter(string name, QualitativeOutcome[] possibleOutcomes, Dictionary<string, double> interpretationDictionary, double defaultValue = 0)
        {
            this.Name = name;
            this.QualitativeParameter = new QualitativeParameter("DiscreteOutcomes", possibleOutcomes);
            this.InterpretationDictionary = interpretationDictionary;
            this.DefaultValue = defaultValue;
        }
    }

    public class RandomBagParameter : IParameter
    {
        // properties
        public string Name { get; set; }
        public Dictionary<double, int> Contents { get; set; }
        public RandomBagReplacement ReplacementRule { get; set; }


        // summary properties
        public int NumberOfItems
        {
            get
            {
                if (Contents.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return Contents.Sum(kv => kv.Value);
                }
            }
        }
        public bool IsEmpty
        {
            get
            {
                return this.NumberOfItems == 0;
            }
        }


        // constructors
        /// <summary>
        /// A parameter that conceptually resembles a bag containing objects that correspond to all possible outcomes. Each outcome's probability is represented by the proportionate number objects in the bag.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression when evaluating the results of the simulation.</param>
        /// <param name="replacementRule">If set to AfterEveryPick, this parameter will be converted to a DiscreteParameter during the simulation. If set to never, there is the potential for failure if there are not enough objects in the bag to satisfy the number of picks being made.</param>
        public RandomBagParameter(string name, RandomBagReplacement replacementRule = RandomBagReplacement.AfterEachPick)
        {
            this.Name = name;
            this.Contents = new Dictionary<double, int>();
            this.ReplacementRule = replacementRule;
        }
        /// <summary>
        /// A parameter that conceptually resembles a bag containing objects that correspond to all possible outcomes. Each outcome's probability is represented by the proportionate number objects in the bag.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression when evaluating the results of the simulation.</param>
        /// <param name="replacementRule">If set to AfterEveryPick, this parameter will be converted to a DiscreteParameter during the simulation. If set to never, there is the potential for failure if there are not enough objects in the bag to satisfy the number of picks being made.</param>
        public RandomBagParameter(string name, Dictionary<double, int> contents, RandomBagReplacement replacementRule = RandomBagReplacement.AfterEachPick)
        {
            this.Name = name;
            this.Contents = contents;
            this.ReplacementRule = replacementRule;
        }


        // methods
        /// <summary>
        /// Adds the specified number of outcome objects to the bag.
        /// </summary>
        /// <param name="outcome">Value that should be returned when the outcome object is selected from the bag.</param>
        /// <param name="numberOfObjects">Number of outcome objects that should be added to the bag.</param>
        public void Add(double outcome, int numberOfObjects = 1)
        {
            if (Contents.ContainsKey(outcome))
            {
                Contents[outcome] += numberOfObjects;
            }
            else
            {
                Contents.Add(outcome, numberOfObjects);
            }
        }
        /// <summary>
        /// Removes the specified number of outcome objects from the bag. If the number provided is greater than the number of outcome objects, all objects will be removed. If the object does not exist in the bag, this will not throw an error.
        /// </summary>
        /// <param name="outcome">Value that should be returned when the outcome object is selected from the bag.</param>
        /// <param name="numberOfObjects">Number of outcome objects that should be added to the bag.</param>
        public void Remove(double outcome, int numberOfObjects = 1)
        {
            if (Contents.ContainsKey(outcome))
            {
                Contents[outcome] -= numberOfObjects;

                if (Contents[outcome] <= 0)
                    Contents.Remove(outcome);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Removes all instances of the outcome objects from the bag. If the object does not exist in the bag, this will not throw an error.
        /// </summary>
        /// <param name="outcome">Value that should be returned when the outcome object is selected from the bag.</param>
        public void RemoveAll(double outcome)
        {
            if (Contents.ContainsKey(outcome))
            {
                Contents.Remove(outcome);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Removes all outcome objects from the bag. This is a full reset that will require new objects to be added in order for this to be a valid parameter.
        /// </summary>
        public void Empty()
        {
            Contents = new Dictionary<double, int>();
        }
        /// <summary>
        /// Returns a list containing all objects in the bag with the amount of each object in the list corresponding to the number of those objects in the bag.
        /// </summary>
        /// <returns></returns>
        public double[] ContentsToArray()
        {
            double[] contents = new double[NumberOfItems];
            int index = 0;

            foreach (var kv in this.Contents)
            {
                for (int i = 0; i < kv.Value; i++)
                {
                    contents[index] = kv.Key;
                    index++;
                }
            }

            return contents;
        }
    }

    public class SimulationParameter : IParameter
    {
        public string Name { get; set; }
        public Simulation Simulation { get; set; }
        public SimulationParameterReturnType ReturnType { get; set; }
        public int SummaryRunCount { get; set; }
        public ParameterConstraint Constraint { get; set; }

        /// <summary>
        /// A parameter whose values are determined by running an inner simulation the same number of times as the outer simulation. The total number of inner simulations will be equal to numberOfSimulations ^ (#innerSimulations - 1) if you return the results of the simulation. If you choose to return a summary statistics instead, the number of inner simulations will be equal to (numberOfSimulations * innerSimulationRunCount) ^ (#innerSimulations - 1).
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="simulation">A complete simulation that could be evaluated on its own.</param>
        /// <param name="returnType">The data point that should be returned from each underlying simulation.</param>
        /// <param name="summaryRunCount">The number of times the simulation should be run to generate the summary statistic. Has no affect when return type is set to Results.</param>
        public SimulationParameter(string name, Simulation simulation, SimulationParameterReturnType returnType = SimulationParameterReturnType.Results, int summaryRunCount = 10000)
        {
            this.Name = name;
            this.Simulation = simulation;
            this.ReturnType = returnType;
            this.SummaryRunCount = summaryRunCount;
            this.Constraint = null;
        }
        /// <summary>
        /// A parameter whose values are determined by running an inner simulation the same number of times as the outer simulation. The total number of inner simulations will be equal to numberOfSimulations ^ (#innerSimulations - 1) if you return the results of the simulation. If you choose to return a summary statistics instead, the number of inner simulations will be equal to (numberOfSimulations * innerSimulationRunCount) ^ (#innerSimulations - 1).
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="simulation">A complete simulation that could be evaluated on its own.</param>
        /// <param name="constraint">A ParameterConstraint object containing information about how the simulated values of this parameted should be constrained.</param>
        /// <param name="returnType">The data point that should be returned from each underlying simulation.</param>
        /// <param name="summaryRunCount">The number of times the simulation should be run to generate the summary statistic. Has no affect when return type is set to Results.</param>
        public SimulationParameter(string name, Simulation simulation, ParameterConstraint constraint, SimulationParameterReturnType returnType = SimulationParameterReturnType.Results, int summaryRunCount = 10000)
        {
            this.Name = name;
            this.Simulation = simulation;
            this.ReturnType = returnType;
            this.SummaryRunCount = summaryRunCount;
            this.Constraint = constraint;
        }
    }

    #endregion

    #region Qualitative Parameters

    public class QualitativeOutcome
    {
        internal double _cumulativeProbability; // used to increase speed of simulation by precomputing the comparison values
        private double _probability;

        public string Value { get; set; }
        public double Probability
        {
            get
            {
                return _probability;
            }
            set
            {
                if (value < 0d || value > 1d)
                    throw new InvalidProbabilityException($"Probability of {value} is not between 0 and 1.");

                _probability = value;
            }
        }

        /// <summary>
        /// One of the possible outcomes in a discrete parameter. A discrete outcome must have a probability between 0.0 and 1.0. The sum of the probability of all discrete outcomes must equal 1.0.
        /// </summary>
        /// <param name="value">Value of the discrete parameter when this outcome is selected.</param>
        /// <param name="probability">Probability of this outcome being selected.</param>
        public QualitativeOutcome(string value, double probability)
        {
            this.Value = value;
            this.Probability = probability;
            _cumulativeProbability = _probability;
        }
    }

    public class QualitativeParameter : IQualitativeParameter
    {
        private QualitativeOutcome[] _possibleOutcomes;

        public string Name { get; set; }
        public QualitativeOutcome[] PossibleOutcomes
        {
            get
            {
                return _possibleOutcomes;
            }
            set
            {
                double sumOfProbabilities = 0d;

                for (int outcome = 0; outcome < value.Length; outcome++)
                    sumOfProbabilities += value[outcome].Probability;

                if (sumOfProbabilities < 0.99d || sumOfProbabilities > 1.01d)
                    throw new InvalidProbabilityException($"Total probability of {sumOfProbabilities} does not equal 100%.");

                _possibleOutcomes = value;
            }
        }

        /// <summary>
        /// A parameter where there are a predetermined number of possible outcomes whose probabilities sum to 100%.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="possibleOutcomes">An array of possible outcomes, where each outcome has a probability between 0 and 1 and the cumulative probability of all outcomes is 100%.</param>
        public QualitativeParameter(string name, params QualitativeOutcome[] possibleOutcomes)
        {
            this.Name = name;
            this.PossibleOutcomes = possibleOutcomes;

            // set all cumulative probabilities to make matching the outcome faster duration the simulation      
            _possibleOutcomes[0]._cumulativeProbability = _possibleOutcomes[0].Probability;

            for (int po = 1; po < possibleOutcomes.Length; po++)
                _possibleOutcomes[po]._cumulativeProbability = _possibleOutcomes[po].Probability + _possibleOutcomes[po - 1]._cumulativeProbability;

            // force cumulative probability of last outcome to exactly 100% to avoid rounding errors
            possibleOutcomes.Last()._cumulativeProbability = 1d;
        }
    }

    public class QualitativeConditionalOutcome
    {
        public ComparisonOperator ComparisonOperator { get; set; }
        public double ConditionalValue { get; set; }
        public string ReturnValue { get; set; }

        /// <summary>
        /// One of the possible conditions that may be met in a QualitativeConditionalParameter. The ConditionalValue will be used as the right value in the comparison operation and the QualitativeConditionalParameter's ReferenceValue will be used on the left side.
        /// </summary>
        /// <param name="comparisonOperator">Operator to use when comparing the ConditionalValue to the ReferenceValue.</param>
        /// <param name="comparisonValue">Value that the QualitativeConditionalParameter's ReferenceValue will be compared against.</param>
        /// <param name="returnValue">Value that will be returned if the conditional operation evaluates to true.</param>
        public QualitativeConditionalOutcome(ComparisonOperator comparisonOperator, double comparisonValue, string returnValue)
        {
            this.ComparisonOperator = comparisonOperator;
            this.ConditionalValue = comparisonValue;
            this.ReturnValue = returnValue;
        }
    }

    public class QualitativeConditionalParameter : IQualitativeParameter
    {
        public string Name { get; set; }
        public SimulationParameter ReferenceParameter { get; set; }
        public QualitativeConditionalOutcome[] ConditionalOutcomes { get; set; }
        public string DefaultValue { get; set; }

        /// <summary>
        /// A parameter whose value will take the value of the first QualitativeConditionalOutcome that evalutes as true, or the default value if none of the outcomes evaluate to true.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="referenceParameter">The parameter whose value will be used on the left side of the comparison operation.</param>
        /// <param name="defaultValue">The value to return if none of the QualitativeConditionalOutcomes evaluate to true.</param>
        /// <param name="qualitativeConditionalOutcomes">Array of conditions to be evaluated in order. The first conditional that evaluates to true with have its ReturnValue used as the ConditionalParameter value for the simulation.</param>
        public QualitativeConditionalParameter(string name, IParameter referenceParameter, string defaultValue, params QualitativeConditionalOutcome[] qualitativeConditionalOutcomes)
        {
            this.Name = name;
            this.ConditionalOutcomes = qualitativeConditionalOutcomes;
            this.DefaultValue = defaultValue;

            // accept any type of reference parameter but force it to become a simulation parameter in order to use recursion inside simulation
            if (referenceParameter is SimulationParameter)
            {
                this.ReferenceParameter = (SimulationParameter)referenceParameter;
            }
            else
            {
                Simulation simulation = new Simulation(referenceParameter.Name, referenceParameter);
                SimulationParameter convertedParameter = new SimulationParameter(referenceParameter.Name, simulation, SimulationParameterReturnType.Results);
                this.ReferenceParameter = convertedParameter;
            }
        }
    }

    public class QualitativeRandomBagParameter : IQualitativeParameter
    {
        // properties
        public string Name { get; set; }
        public Dictionary<string, int> Contents { get; set; }
        public RandomBagReplacement ReplacementRule { get; set; }


        // summary properties
        public int NumberOfItems
        {
            get
            {
                if (Contents.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return Contents.Sum(kv => kv.Value);
                }
            }
        }
        public bool IsEmpty
        {
            get
            {
                return this.NumberOfItems == 0;
            }
        }


        // constructors
        /// <summary>
        /// A parameter that conceptually resembles a bag containing objects that correspond to all possible outcomes. Each outcome's probability is represented by the proportionate number objects in the bag.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression when evaluating the results of the simulation.</param>
        /// <param name="replacementRule">If set to AfterEveryPick, this parameter will be converted to a DiscreteParameter during the simulation. If set to never, there is the potential for failure if there are not enough objects in the bag to satisfy the number of picks being made.</param>
        public QualitativeRandomBagParameter(string name, RandomBagReplacement replacementRule = RandomBagReplacement.AfterEachPick)
        {
            this.Name = name;
            this.Contents = new Dictionary<string, int>();
            this.ReplacementRule = replacementRule;
        }
        /// <summary>
        /// A parameter that conceptually resembles a bag containing objects that correspond to all possible outcomes. Each outcome's probability is represented by the proportionate number objects in the bag.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression when evaluating the results of the simulation.</param>
        /// <param name="replacementRule">If set to AfterEveryPick, this parameter will be converted to a DiscreteParameter during the simulation. If set to never, there is the potential for failure if there are not enough objects in the bag to satisfy the number of picks being made.</param>
        public QualitativeRandomBagParameter(string name, Dictionary<string, int> contents, RandomBagReplacement replacementRule = RandomBagReplacement.AfterEachPick)
        {
            this.Name = name;
            this.Contents = contents;
            this.ReplacementRule = replacementRule;
        }


        // methods
        /// <summary>
        /// Adds the specified number of outcome objects to the bag.
        /// </summary>
        /// <param name="outcome">Value that should be returned when the outcome object is selected from the bag.</param>
        /// <param name="numberOfObjects">Number of outcome objects that should be added to the bag.</param>
        public void Add(string outcome, int numberOfObjects = 1)
        {
            if (Contents.ContainsKey(outcome))
            {
                Contents[outcome] += numberOfObjects;
            }
            else
            {
                Contents.Add(outcome, numberOfObjects);
            }
        }
        /// <summary>
        /// Removes the specified number of outcome objects from the bag. If the number provided is greater than the number of outcome objects, all objects will be removed. If the object does not exist in the bag, this will not throw an error.
        /// </summary>
        /// <param name="outcome">Value that should be returned when the outcome object is selected from the bag.</param>
        /// <param name="numberOfObjects">Number of outcome objects that should be added to the bag.</param>
        public void Remove(string outcome, int numberOfObjects = 1)
        {
            if (Contents.ContainsKey(outcome))
            {
                Contents[outcome] -= numberOfObjects;

                if (Contents[outcome] <= 0)
                    Contents.Remove(outcome);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Removes all instances of the outcome objects from the bag. If the object does not exist in the bag, this will not throw an error.
        /// </summary>
        /// <param name="outcome">Value that should be returned when the outcome object is selected from the bag.</param>
        public void RemoveAll(string outcome)
        {
            if (Contents.ContainsKey(outcome))
            {
                Contents.Remove(outcome);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// Removes all outcome objects from the bag. This is a full reset that will require new objects to be added in order for this to be a valid parameter.
        /// </summary>
        public void Empty()
        {
            Contents = new Dictionary<string, int>();
        }
        /// <summary>
        /// Returns a list containing all objects in the bag with the amount of each object in the list corresponding to the number of those objects in the bag.
        /// </summary>
        /// <returns></returns>
        public string[] ContentsToArray()
        {
            string[] contents = new string[NumberOfItems];
            int index = 0;

            foreach (var kv in this.Contents)
            {
                for (int i = 0; i < kv.Value; i++)
                {
                    contents[index] = kv.Key;
                    index++;
                }
            }

            return contents;
        }
    }

    #endregion
}