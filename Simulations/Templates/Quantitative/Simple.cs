using System.Collections.Generic;
using MathNet.Numerics.Distributions;

namespace Simulations.Templates
{
    /// <summary>
    /// QuantitativeTemplates return a Simulation that was configured based on the passed arguments. The Simulation can be run immediate by calling the Simulate method or used as the Simulation argument in a simulation parameter. Essentially, these are shortcuts for creating common simulation scenarios.
    /// </summary>
    public static partial class QuantitativeTemplates
    {
        /// <summary>
        /// Returns a simulation configured to roll x number of n-sided dice. This simulation can be run on its own or used as the Simulation argument in a SimulationParameter.
        /// </summary>
        /// <param name="numberOfSides">Number of sides on every dice.</param>
        /// <param name="numberOfDice">Number of dice to roll.</param>
        /// <returns></returns>
        public static Simulation DiceRoll(int numberOfSides, int numberOfDice = 1)
        {
            // generate expression
            List<string> diceNames = new List<string>();
            for (int i = 0; i < numberOfDice; i++) diceNames.Add($"die{i}");
            string expression = string.Join(" + ", diceNames);

            // create parameters
            IParameter[] parameters = new IParameter[numberOfDice];
            for (int i = 0; i < numberOfDice; i++) parameters[i] = new DistributionParameter(diceNames[i], new DiscreteUniform(1, numberOfSides));

            // return simulation
            return new Simulation(expression, parameters);
        }
    }
}
