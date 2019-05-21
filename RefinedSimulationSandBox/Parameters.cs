using MathNet.Numerics.Distributions;
using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinedSimulationSandBox
{
    /* -----------------------------------------
       parameter names cannot start with numbers
       ----------------------------------------- */

    // TODO:
    //   - Figure out how to handle mix of doubles and ints
    //       > Should everything just be FloatingPoint??

    interface IParameter
    {
        string Name { get; set; }
    }

    class ConditionalOutcome
    {
        public ComparisonOperator ComparisonOperator { get; set; }
        public double ConditionalValue { get; set; }
        public double ReturnValue { get; set; }

        /// <summary>
        /// One of the possible conditions that may be met in a ConditionalParameter. The ConditionalValue will be used as the right value in the comparison operation and the ConditionalParameter's ReferenceValue will be used on the left side.
        /// </summary>
        /// <param name="comparisonOperator">Operator to use when comparing the ConditionalValue to the ReferenceValue.</param>
        /// <param name="comparisonValue">Value that the ConditionalParameter's ReferenceValue will be compared against..</param>
        /// <param name="returnValue">Value that will be returned if the conditional oepration evaluates to true.</param>
        public ConditionalOutcome(ComparisonOperator comparisonOperator, double comparisonValue, double returnValue)
        {
            this.ComparisonOperator = comparisonOperator;
            this.ConditionalValue = comparisonValue;
            this.ReturnValue = returnValue;
        }
    }

    class ConditionalParameter : IParameter
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
        /// <param name="conditionalOutcomes">Array of conditions to be evaluated in order. The first conditional that evalues to true with have its ReturnValue used as the ConditionalParameter value for the simulation.</param>
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
                SimulationParameter convertedParameter = new SimulationParameter(referenceParameter.Name, simulation);
                this.ReferenceParameter = convertedParameter;
            }
        }
    }

    class ConstantParameter : IParameter
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

    class DiscreteOutcome
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

    class DiscreteParameter : IParameter
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

    class DistributionParameter : IParameter
    {
        public string Name { get; set; }
        public IDistribution Distribution { get; set; }

        /// <summary>
        /// A parameter whose value is sampled from the specified distribution.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="distribution">The distribution from which to sample values. Distribution must have a .Samples method that returns an array of doubles or ints.</param>
        public DistributionParameter(string name, IDistribution distribution)
        {
            this.Name = name;
            this.Distribution = distribution;
        }
    }

    class PrecomputedParameter : IParameter
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

    class SimulationParameter : IParameter
    {
        public string Name { get; set; }
        public Simulation Simulation { get; set; }

        /// <summary>
        /// A parameter whose values are determined by running an inner simulation the same number of times as the outer simulation. The total number of inner simulations will be equal to numberOfSimulations ^ #innerSimulations - 1. In other words, you should not see much of a performance impact until you nest simulations two levels deep, as the first level is more of a way to group parameters than to chain simulations.
        /// </summary>
        /// <param name="name">Name of the parameter to be used in the expression whe evaluating the results of the simulation.</param>
        /// <param name="simulation">A complete simulation that could be evaluated on its own.</param>
        public SimulationParameter(string name, Simulation simulation)
        {
            this.Name = name;
            this.Simulation = simulation;
        }
    }



    class QualitativeOutcome
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
}
