using Simulations;
using Simulations.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;

namespace SimulationTests
{
    [TestClass]
    public class QualitativeTemplateTests
    {
        [TestMethod]
        public void Templates_Qualitative_CoinFlip()
        {
            var results = QualitativeTemplates.CoinFlip(1000);
            Assert.AreEqual(1000, results.NumberOfSimulations, "Incorrect number of simulations performed.");
            Assert.AreEqual(500, results.GetOutcome("Heads").Count, 100, "Incorrect number of heads.");
        }

        [TestMethod]
        public void Templates_Qualitative_DiceRoll()
        {
            var results = QualitativeTemplates.DiceRoll(6, 1000);
            Assert.AreEqual(1000, results.NumberOfSimulations, "Incorrect number of simulations performed.");
        }

        [TestMethod]
        public void Templates_Qualitative_DeckOfCards()
        {
            try
            {
                var results = QualitativeTemplates.DeckOfCards(53);
            }
            catch (Simulations.Exceptions.RandomBagItemCountException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Did not catch too many cards drawn exception.");
            }

            var cards = QualitativeTemplates.DeckOfCards(5);
            Assert.AreEqual(5, cards.NumberOfSimulations, "Incorrect number of cards drawn.");
        }
    }

    [TestClass]
    public class QuantitativeTemplateTests
    {
        [TestMethod]
        public void Templates_Quantitative_CAPM()
        {
            var results = QuantitativeTemplates.CAPM(new ConstantParameter("rf", 0.02), 
                                                     new ConstantParameter("b", 1.00), 
                                                     new DistributionParameter("rm", new Normal(0.08, 0.025)))
                                                     .Simulate(1000);

            Assert.AreEqual(1000, results.NumberOfSimulations, "Incorrect number of simulations performed.");
            Assert.AreEqual(0.08, results.Mean, 0.01, "Incorrect mean result.");
        }

        [TestMethod]
        public void Templates_Quantitative_DiceRoll()
        {
            var results = QuantitativeTemplates.DiceRoll(6, 2).Simulate(1000);
            Assert.AreEqual(1000, results.NumberOfSimulations, "Incorrect number of simulations performed.");
            Assert.AreEqual(7.0, results.Mean, 0.5, "Incorrect mean results.");

            Simulation simulation = new Simulation("die1 + die2",
                                                   new SimulationParameter("die1", QuantitativeTemplates.DiceRoll(6, 1)),
                                                   new SimulationParameter("die2", QuantitativeTemplates.DiceRoll(6, 1)));

            results = simulation.Simulate(1000);
            Assert.AreEqual(1000, results.NumberOfSimulations, "Incorrect number of simulations performed.");
            Assert.AreEqual(7.0, results.Mean, 0.5, "Incorrect mean results.");
        }
    }
}
