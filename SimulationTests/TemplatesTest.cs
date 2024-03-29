﻿using Simulations;
using Simulations.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;

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

        [TestMethod]
        public void Templates_Qualitative_RestaurantPicker()
        {
            try
            {
                var results = QualitativeTemplates.RestaurantPicker(null);
            }
            catch (Simulations.Exceptions.EmptyBagException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Did not catch empty bag exception.");
            }

            string restaurant = QualitativeTemplates.RestaurantPicker("McDonald's");
            Assert.AreEqual(restaurant, "McDonald's", "Incorrect restaurant chosen.");
        }

        [TestMethod]
        public void Templates_Qualitative_RestaurantVoter()
        {
            try
            {
                var results = QualitativeTemplates.RestaurantVoter(null);
            }
            catch (Simulations.Exceptions.EmptyBagException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Did not catch empty bag exception.");
            }

            string restaurant = QualitativeTemplates.RestaurantVoter(("McDonald's", 3));
            Assert.AreEqual(restaurant, "McDonald's", "Incorrect restaurant chosen.");
        }

        [TestMethod]
        public void Templates_Qualitative_ShuffleItems()
        {
            try
            {
                var results = QualitativeTemplates.ShuffleItems(null);
            }
            catch (Simulations.Exceptions.EmptyBagException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Did not catch empty bag exception.");
            }

            List<string> order = QualitativeTemplates.ShuffleItems("a", "b", "c", "d", "e");
            Assert.AreEqual(order.Count, 5, "Incorrect number of choices.");
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

        [TestMethod]
        public void Templates_Quantitative_BlackScholes_Function()
        {
            double callPrice = QuantitativeTemplates.BlackScholes(OptionContractType.Call, 210.59, 205, 4d/365d, 0.1404, 0.2175);
            double putPrice = QuantitativeTemplates.BlackScholes(OptionContractType.Put, 210.59, 205, 4d/365d, 0.1404, 0.2175);

            Assert.AreEqual(6.104464, callPrice, 0.01, "Incorrect call price.");
            Assert.AreEqual(0.026416, putPrice, 0.01, "Incorrect put price.");
        }

        [TestMethod]
        public void Templates_Quantitative_BlackScholes_Simulation()
        {
            ConstantParameter priceOfUnderlyingAsset = new ConstantParameter("St", 210.59);
            ConstantParameter strikePrice = new ConstantParameter("K", 205);
            ConstantParameter timeToMaturity = new ConstantParameter("t", 4d / 365d);
            ConstantParameter standardDeviationOfStocksReturns = new ConstantParameter("o", 0.1404);
            ConstantParameter riskFreeRate = new ConstantParameter("r", 0.2175);

            Simulation callSimulation = QuantitativeTemplates.BlackScholes(OptionContractType.Call,
                                                                           priceOfUnderlyingAsset,
                                                                           strikePrice,
                                                                           timeToMaturity,
                                                                           standardDeviationOfStocksReturns,
                                                                           riskFreeRate);

            double callSimulationPrice = callSimulation.Simulate(1).Results[0];
            double callActualPrice = QuantitativeTemplates.BlackScholes(OptionContractType.Call, 210.59, 205, 4d / 365d, 0.1404, 0.2175);
            Assert.AreEqual(callActualPrice, callSimulationPrice, 0.01, "Simulation call price does not match function.");

            Simulation putSimulation = QuantitativeTemplates.BlackScholes(OptionContractType.Put,
                                                                          priceOfUnderlyingAsset,
                                                                          strikePrice,
                                                                          timeToMaturity,
                                                                          standardDeviationOfStocksReturns,
                                                                          riskFreeRate);

            double putSimulationPrice = putSimulation.Simulate(1).Results[0];
            double putActualPrice = QuantitativeTemplates.BlackScholes(OptionContractType.Put, 210.59, 205, 4d / 365d, 0.1404, 0.2175);
            Assert.AreEqual(putActualPrice, putSimulationPrice, 0.01, "Simulation put price does not match function.");
        }
    }
}
