using System;

namespace Simulations.Templates
{
    /// <summary>
    /// QuantitativeTemplates return a Simulation that was configured based on the passed arguments. The Simulation can be run immediate by calling the Simulate method or used as the Simulation argument in a simulation parameter. Essentially, these are shortcuts for creating common simulation scenarios.
    /// </summary>
    public static partial class QuantitativeTemplates
    {
        /// <summary>
        /// This simulation can be run on its own or used as the Simulation argument in a SimulationParameter.
        /// </summary>
        /// <returns></returns>
        public static Simulation CAPM(IParameter riskFreeRate, IParameter beta, IParameter marketReturn)
        {
            // generate expression
            string expression = $"{riskFreeRate.Name} + ({beta.Name} * ({marketReturn.Name} - {riskFreeRate.Name}))";

            // return simulation
            return new Simulation(expression, riskFreeRate, beta, marketReturn);
        }

        public static Simulation BlackScholes()
        {
            // use DistributionFunctionParameter for computing CDF of normal distribution for d1 and d2

            throw new NotImplementedException();
        }
    }
}
