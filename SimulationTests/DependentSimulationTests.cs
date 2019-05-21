using Simulations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;

namespace SimulationTests
{
    [TestClass]
    public class DependentSimulationTests
    {
        private static int _numberOfSimulations = 365;

        [TestMethod]
        public void Dependent_StockPriceMovement()
        {
            DistributionParameter normal = new DistributionParameter("dailyChange", new Normal(0.01, 0.0025));
            string expression = "value * (1 + dailyChange)";

            DependentSimulation simulation = new DependentSimulation(100, expression, normal);
            DependentSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Dependent_RecomputeExpression()
        {
            DistributionParameter normal = new DistributionParameter("dailyChange", new Normal(0.01, 0.0025));
            string expression = "value * (1 + dailyChange)";

            DependentSimulation simulation = new DependentSimulation(100, expression, normal);
            DependentSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");

            expression = "value * (2 + dailyChange)";
            results.RecomputeExpression(expression);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Dependent_RegenerateResults()
        {
            DistributionParameter normal = new DistributionParameter("dailyChange", new Normal(0.01, 0.0025));
            string expression = "value * (1 + dailyChange)";

            DependentSimulation simulation = new DependentSimulation(100, expression, normal);
            DependentSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");

            results.Regenerate();
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");

            results.Regenerate(100);
            Assert.AreEqual(100, results.NumberOfPeriods, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Dependent_ReplaceParameter()
        {
            DistributionParameter normal = new DistributionParameter("dailyChange", new Normal(0.01, 0.0025));
            string expression = "value * (1 + dailyChange)";

            DependentSimulation simulation = new DependentSimulation(100, expression, normal);
            DependentSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");

            results.ReplaceParameter(new DistributionParameter("dailyChange", new Normal(0.05, 0.01)));
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");

            results.ReplaceParameter(new DistributionParameter("newNameDailyChange", new Normal(0.05, 0.01)));
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Dependent_ReplaceInitialValue()
        {
            DistributionParameter normal = new DistributionParameter("dailyChange", new Normal(0.01, 0.0025));
            string expression = "value * (1 + dailyChange)";

            DependentSimulation simulation = new DependentSimulation(100, expression, normal);
            DependentSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");

            results.ReplaceInitialValue(1000);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfPeriods, "Incorrect number of simulations was performed.");
        }
    }
}
