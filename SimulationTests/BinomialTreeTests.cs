using Simulations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    [TestClass]
    public class BinomialTreeTests
    {
        [TestMethod]
        public void Binomial_BasicUpDown()
        {
            BinomialTreeSimulation simulation = new BinomialTreeSimulation(100, new ConstantParameter("volatility", 0.01));
            BinomialTreeSimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingValue, 270.48, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingValue, 36.6, 0.1, "Incorrect minimum value.");
        }

        [TestMethod]
        public void Binomial_CumulativeProbabilities()
        {
            BinomialTreeSimulation simulation = new BinomialTreeSimulation(100, new ConstantParameter("volatility", 0.01));
            BinomialTreeSimulationResults results = simulation.Simulate(2);
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingValue, 102.01, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingValue, 98.01, 0.1, "Incorrect minimum value.");
            Assert.AreEqual(results.LastPeriod[0].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
        }

        [TestMethod]
        public void Binomial_ExpectedValue()
        {
            BinomialTreeSimulation simulation = new BinomialTreeSimulation(100, new ConstantParameter("volatility", 0.01));
            BinomialTreeSimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingValue, 270.48, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingValue, 36.6, 0.1, "Incorrect minimum value.");
            Assert.AreEqual(results.ExpectedValue, 99.99, 0.1, "Incorrect expected value.");

            simulation = new BinomialTreeSimulation(100, new ConstantParameter("volatility", 0.01), 0.6);
            results = simulation.Simulate(100);
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingValue, 270.48, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingValue, 36.6, 0.1, "Incorrect minimum value.");
            Assert.IsTrue(results.ExpectedValue > 99.99, "Incorrect expected value.");

            simulation = new BinomialTreeSimulation(100, new ConstantParameter("volatility", 0.01), 0.4);
            results = simulation.Simulate(100);
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingValue, 270.48, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingValue, 36.6, 0.1, "Incorrect minimum value.");
            Assert.IsTrue(results.ExpectedValue < 99.99, "Incorrect expected value.");
        }

        [TestMethod]
        public void Binomial_VolatilityValue()
        {
            BinomialTreeSimulation simulation = new BinomialTreeSimulation(100, new ConstantParameter("volatility", 0.01));
            BinomialTreeSimulationResults results = simulation.Simulate(2);
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingValue, 102.01, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingValue, 98.01, 0.1, "Incorrect minimum value.");
            Assert.AreEqual(results.LastPeriod[0].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].Volatility, 0.01, 0.001, "Incorrect volatility.");
        }
    }

    [TestClass]
    public class BinomialOptionPricingTreeTests
    {
        [TestMethod]
        public void BinomialOptionPricing_BasicUpDown()
        {
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 100, 0.5, 1, 360);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 105.4118, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 94.866, 0.1, "Incorrect minimum value.");

            simulation.NumberOfPeriods = 360;
            results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 360, "Incorrect number of periods generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 120.8931, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 82.71769, 0.1, "Incorrect minimum value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_CumulativeProbabilities()
        {
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 2, 0.5, 1, 360);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.1055, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.89465, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(results.LastPeriod[0].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
        }

        [TestMethod]
        public void BinomialOptionPricing_ExpectedValue()
        {
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 100, 0.5, 1, 360);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 105.4118, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 94.866, 0.1, "Incorrect minimum value.");
            Assert.AreEqual(results.ExpectedEndingPrice_Underlying, 99.99, 0.1, "Incorrect expected value.");

            simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 100, 0.6, 1, 360);
            results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 105.4118, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 94.866, 0.1, "Incorrect minimum value.");
            Assert.IsTrue(results.ExpectedEndingPrice_Underlying > 99.99, "Incorrect expected value.");

            simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 100, 0.4, 1, 360);
            results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 100, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 5151, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 105.4118, 0.1, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 94.866, 0.1, "Incorrect minimum value.");
            Assert.IsTrue(results.ExpectedEndingPrice_Underlying < 99.99, "Incorrect expected value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_VolatilityUpDownFactors()
        {
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 2, 0.5, 1, 360);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.1055, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.89465, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(results.LastPeriod[0].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].Volatility, 0.00052718, 0.00005, "Incorrect volatility.");
            Assert.AreEqual(results.LastPeriod[2].UpFactor, 1.0005271, 0.001, "Incorrect up factor.");
            Assert.AreEqual(results.LastPeriod[2].DownFactor, 0.999473209, 0.001, "Incorrect down factor.");

            simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 2, 0.5, 30, 360);
            results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.579, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.42431, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(results.LastPeriod[0].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].CumulativeProbability, 0.25, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[2].Volatility, 0.002886751, 0.00005, "Incorrect volatility.");
            Assert.AreEqual(results.LastPeriod[2].UpFactor, 1.002890922, 0.001, "Incorrect up factor.");
            Assert.AreEqual(results.LastPeriod[2].DownFactor, 0.997117411, 0.001, "Incorrect down factor.");

            simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), 2, 0.5, 360, 360);
            simulation.NumberOfPeriods = 1;
            results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 1, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 3, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 101.0050167, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.00498337, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(results.LastPeriod[0].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].CumulativeProbability, 0.50, 0.1, "Incorrect cumulative probability.");
            Assert.AreEqual(results.LastPeriod[1].Volatility, 0.01, 0.00005, "Incorrect volatility.");
            Assert.AreEqual(results.LastPeriod[1].UpFactor, 1.010050167, 0.001, "Incorrect up factor.");
            Assert.AreEqual(results.LastPeriod[1].DownFactor, 0.990049834, 0.001, "Incorrect down factor.");
        }

        [TestMethod]
        public void BinomialOptionPricing_CallOptionPrice_American()
        {
            OptionContract contract = new OptionContract(OptionContractType.Call, 100, 0.02, 14, OptionExecutionRules.American, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), contract, 0.5, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.2793, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.7215, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(0.279276, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(0, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(0.069765, (double)results.FairValueOfOption, 0.01, "Incorrect fair option value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_CallOptionPrice_European()
        {
            OptionContract contract = new OptionContract(OptionContractType.Call, 100, 0.02, 14, OptionExecutionRules.European, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(102, new ConstantParameter("volatility", 0.01), contract, 0.5, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 102.2849, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 101.7159, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(2.284861, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(1.715932, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(1.998643, (double)results.FairValueOfOption, 0.001, "Incorrect fair option value."); // american is exactly 2.0; this precision proves european worked
        }

        [TestMethod]
        public void BinomialOptionPricing_PutOptionPrice_American()
        {
            OptionContract contract = new OptionContract(OptionContractType.Put, 100, 0.02, 14, OptionExecutionRules.American, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), contract, 0.5, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.2793, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.7215, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(0.278498, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(0, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(0.069646, (double)results.FairValueOfOption, 0.01, "Incorrect fair option value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_PutOptionPrice_European()
        {
            OptionContract contract = new OptionContract(OptionContractType.Put, 100, 0.02, 14, OptionExecutionRules.European, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(102, new ConstantParameter("volatility", 0.01), contract, 0.5, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 102.2849, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 101.7159, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(0, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(0, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(0, (double)results.FairValueOfOption, 0.001, "Incorrect fair option value."); // american is exactly 2.0; this precision proves european worked

            simulation = new BinomialOptionPricingSimulation(98, new ConstantParameter("volatility", 0.01), contract, 0.5, 7);
            results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 98.27369, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 97.72707, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(2.272928, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(1.72631, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(1.998255, (double)results.FairValueOfOption, 0.001, "Incorrect fair option value."); // american is exactly 2.0; this precision proves european worked
        }

        [TestMethod]
        public void BinomialOptionPricing_CallOptionPrice_UnevenWeight_American()
        {
            OptionContract contract = new OptionContract(OptionContractType.Call, 100, 0.02, 14, OptionExecutionRules.American, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(102, new ConstantParameter("volatility", 0.01), contract, 0.45, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 102.2849, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 101.7159, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(2.284861, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(1.715932, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(2.000, (double)results.FairValueOfOption, 0.01, "Incorrect fair option value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_CallOptionPrice_UnevenWeight_European()
        {
            OptionContract contract = new OptionContract(OptionContractType.Call, 100, 0.02, 14, OptionExecutionRules.European, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(102, new ConstantParameter("volatility", 0.01), contract, 0.45, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 102.2849, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 101.7159, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(2.284861, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(1.715932, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(1.970221, (double)results.FairValueOfOption, 0.01, "Incorrect fair option value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_PutOptionPrice_UnevenWeight_American()
        {
            OptionContract contract = new OptionContract(OptionContractType.Put, 100, 0.02, 14, OptionExecutionRules.American, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), contract, 0.6, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.2793, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.7215, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(0.278498, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(0, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(0.055717, (double)results.FairValueOfOption, 0.01, "Incorrect fair option value.");
        }

        [TestMethod]
        public void BinomialOptionPricing_PutOptionPrice_UnevenWeight_European()
        {
            OptionContract contract = new OptionContract(OptionContractType.Put, 100, 0.02, 14, OptionExecutionRules.European, 360);
            BinomialOptionPricingSimulation simulation = new BinomialOptionPricingSimulation(100, new ConstantParameter("volatility", 0.01), contract, 0.6, 7);
            BinomialOptionPricingSimulationResults results = simulation.Simulate();
            Assert.AreEqual(results.NumberOfPeriods, 2, "Incorrect number of periods generated.");
            Assert.AreEqual(results.NumberOfNodes, 6, "Incorrect number of nodes generated.");
            Assert.AreEqual(results.MaximumEndingPrice_Underlying, 100.2793, 0.01, "Incorrect maximum value.");
            Assert.AreEqual(results.MinimumEndingPrice_Underlying, 99.7215, 0.01, "Incorrect minimum value.");
            Assert.AreEqual(0.278498, (double)results.MaximumEndingPrice_Option, 0.01, "Incorrect max option value.");
            Assert.AreEqual(0, (double)results.MinimumEndingPrice_Option, 0.01, "Incorrect min option value.");
            Assert.AreEqual(0.044525, (double)results.FairValueOfOption, 0.01, "Incorrect fair option value.");
        }

        // TODO
        // - Trade date tests:
        //      > Number of trading days
        //      > Valuation difference using 1 vs 5 vs 30 days
        //      > Specific start and end date instead of imputed (like everything above)
        //
        // * would it make sense to allow valuation on a specific day of week vs month instead of every x days, where holidays can throw off day pattern ?
    }
}
