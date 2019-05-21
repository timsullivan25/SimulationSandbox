using Simulations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;

namespace SimulationTests
{
    [TestClass]
    public class QualitativeTests
    {
        private static int _numberOfSimulations = 1000;

        [TestMethod]
        public void Qualitative_DiceRoll()
        {
            QualitativeOutcome[] possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.167),
                new QualitativeOutcome("2", 0.167),
                new QualitativeOutcome("3", 0.167),
                new QualitativeOutcome("4", 0.167),
                new QualitativeOutcome("5", 0.167),
                new QualitativeOutcome("6", 0.167)
            };

            QualitativeSimulation simulation = new QualitativeSimulation(possibleOutcomes);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Qualitative_DiceRollWithRegeneration()
        {
            QualitativeOutcome[] possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.167),
                new QualitativeOutcome("2", 0.167),
                new QualitativeOutcome("3", 0.167),
                new QualitativeOutcome("4", 0.167),
                new QualitativeOutcome("5", 0.167),
                new QualitativeOutcome("6", 0.167)
            };

            QualitativeSimulation simulation = new QualitativeSimulation(new QualitativeParameter("diceroll", possibleOutcomes));
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            results = results.Regenerate();
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed when using standard regeneration.");

            results = results.Regenerate(10);
            Assert.AreEqual(10, results.NumberOfSimulations, "Incorrect number of simulations was performed when using new number of simulations in regeneration.");
        }
       
        [TestMethod]
        public void Qualitative_ReplaceParameter()
        {
            QualitativeOutcome[] possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.167),
                new QualitativeOutcome("2", 0.167),
                new QualitativeOutcome("3", 0.167),
                new QualitativeOutcome("4", 0.167),
                new QualitativeOutcome("5", 0.167),
                new QualitativeOutcome("6", 0.167)
            };

            QualitativeSimulation simulation = new QualitativeSimulation(new QualitativeParameter("diceroll", possibleOutcomes));
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            QualitativeParameter param = new QualitativeParameter("arrowkey",
                                                                  new QualitativeOutcome("left", 0.25),
                                                                  new QualitativeOutcome("right", 0.25),
                                                                  new QualitativeOutcome("up", 0.25),
                                                                  new QualitativeOutcome("down", 0.25));

            results.ReplaceParameter(param);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Qualitative_Conditional()
        {
            QualitativeConditionalParameter param = new QualitativeConditionalParameter("upordown",
                                                                                        new DistributionParameter("normal", new Normal(1, 0.1)),
                                                                                        "flat",
                                                                                        new QualitativeConditionalOutcome(ComparisonOperator.GreaterThan, 1, "up"),
                                                                                        new QualitativeConditionalOutcome(ComparisonOperator.LessThan, 1, "down"));

            QualitativeSimulation simulation = new QualitativeSimulation(param);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Qualitative_Interpretation()
        {
            QualitativeConditionalParameter qualitativeParam = new QualitativeConditionalParameter("upordown",
                                                                                                    new DistributionParameter("normal", new Normal(1, 0.1)),
                                                                                                    "flat",
                                                                                                    new QualitativeConditionalOutcome(ComparisonOperator.GreaterThan, 1, "up"),
                                                                                                    new QualitativeConditionalOutcome(ComparisonOperator.LessThan, 1, "down"));
           
            Dictionary<string, double> interpretationDictionary = new Dictionary<string, double>();
            interpretationDictionary.Add("up", 10);
            interpretationDictionary.Add("down", -10);

            QualitativeInterpretationParameter interpretationParam = new QualitativeInterpretationParameter("scaledmovement",
                                                                                                            qualitativeParam,
                                                                                                            interpretationDictionary,
                                                                                                            0);

            Simulation simulation = new Simulation("scaledmovement", interpretationParam);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Qualitative_Discrete_QueryOutcomeWithZeroResults()
        {
            QualitativeOutcome[] possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.000),
                new QualitativeOutcome("2", 0.200),
                new QualitativeOutcome("3", 0.200),
                new QualitativeOutcome("4", 0.200),
                new QualitativeOutcome("5", 0.200),
                new QualitativeOutcome("6", 0.200)
            };

            QualitativeSimulation simulation = new QualitativeSimulation(possibleOutcomes);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            QualitativeSimulationOutcome outcome = results.GetOutcome("1");
            Assert.AreEqual(outcome.Count, 0, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Qualitative_Discrete_QueryNonExistentOutcome()
        {
            QualitativeOutcome[] possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.000),
                new QualitativeOutcome("2", 0.200),
                new QualitativeOutcome("3", 0.200),
                new QualitativeOutcome("4", 0.200),
                new QualitativeOutcome("5", 0.200),
                new QualitativeOutcome("6", 0.200)
            };

            QualitativeSimulation simulation = new QualitativeSimulation(possibleOutcomes);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            try
            {
                QualitativeSimulationOutcome outcome = results.GetOutcome("7");
            }
            catch (Simulations.Exceptions.NonExistentOutcomeException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Invalid exception type thrown.");
            }           
        }

        [TestMethod]
        public void Qualitative_Conditional_QueryOutcomeWithZeroResults()
        {
            QualitativeConditionalParameter param = new QualitativeConditionalParameter("upordown",
                                                                                        new DistributionParameter("normal", new Normal(1, 0.1)),
                                                                                        "flat",
                                                                                        new QualitativeConditionalOutcome(ComparisonOperator.GreaterThan, 1, "up"),
                                                                                        new QualitativeConditionalOutcome(ComparisonOperator.LessThanOrEqual, 1, "down"));

            QualitativeSimulation simulation = new QualitativeSimulation(param);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            QualitativeSimulationOutcome outcome = results.GetOutcome("flat");
            Assert.AreEqual(outcome.Count, 0, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Qualitative_Conditional_QueryNonExistentOutcome()
        {
            QualitativeConditionalParameter param = new QualitativeConditionalParameter("upordown",
                                                                                        new DistributionParameter("normal", new Normal(1, 0.1)),
                                                                                        "flat",
                                                                                        new QualitativeConditionalOutcome(ComparisonOperator.GreaterThan, 1, "up"),
                                                                                        new QualitativeConditionalOutcome(ComparisonOperator.LessThanOrEqual, 1, "down"));

            QualitativeSimulation simulation = new QualitativeSimulation(param);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            try
            {
                QualitativeSimulationOutcome outcome = results.GetOutcome("7");
            }
            catch (Simulations.Exceptions.NonExistentOutcomeException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Invalid exception type thrown.");
            }
        }

        [TestMethod]
        public void QualitativeSimulation_RandomBag_ReplaceAfterEveryPick()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.AfterEachPick);
            bag.Add("1", 100);

            QualitativeSimulation simulation = new QualitativeSimulation(bag);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(results.MostCommonOutcome.Outcome, "1", "Incorrect most common outcome.");
        }

        [TestMethod]
        public void Qualitative_RandomBag_ReplaceNever()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.AfterEachPick);
            bag.Add("1", 100);

            QualitativeSimulation simulation = new QualitativeSimulation(bag);

            try
            {
                QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            }
            catch (Simulations.Exceptions.RandomBagItemCountException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Caught unexpected error on random bag without enough items to use Never replacement strategy.");
            }

            QualitativeSimulationResults realResults = simulation.Simulate(100);
            Assert.AreEqual(100, realResults.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(realResults.MostCommonOutcome.Outcome, "1", "Incorrect most common outcome.");
        }

        [TestMethod]
        public void Qualitative_RandomBag_ReplaceWhenEmpty()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.WhenEmpty);
            bag.Add("1", 100);

            QualitativeSimulation simulation = new QualitativeSimulation(bag);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(results.MostCommonOutcome.Outcome, "1", "Incorrect most common outcome.");
        }

        [TestMethod]
        public void Qualitative_RandomBag_EmptyBagError()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.AfterEachPick);
            QualitativeSimulation simulation = new QualitativeSimulation(bag);

            try
            {
                QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            }
            catch (Simulations.Exceptions.EmptyBagException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Caught unexpected error on random bag without any items.");
            }
        }

        [TestMethod]
        public void Qualitative_RandomBag_ReplaceNever_MultipleValues()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.Never);
            bag.Add("1", 100);
            bag.Add("2", 300);
            bag.Add("3", 600);

            QualitativeSimulation simulation = new QualitativeSimulation(bag);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(results.MostCommonOutcome.Outcome, "3", "Incorrect most common outcome.");
            Assert.AreEqual(results.LeastCommonOutcome.Outcome, "1", "Incorrect least common outcome.");
            Assert.AreEqual(results.IndividualOutcomes[1].Probability, 0.30, 0.01, "Incorrect probability of middle outcome.");
        }

        [TestMethod]
        public void Qualitative_RandomBag_ReplaceWhenEmpty_MultipleValues()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.WhenEmpty);
            bag.Add("1", 100);
            bag.Add("2", 300);
            bag.Add("3", 600);

            QualitativeSimulation simulation = new QualitativeSimulation(bag);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(results.MostCommonOutcome.Outcome, "3", "Incorrect most common outcome.");
            Assert.AreEqual(results.LeastCommonOutcome.Outcome, "1", "Incorrect least common outcome.");
            Assert.AreEqual(results.IndividualOutcomes[1].Probability, 0.30, 0.01, "Incorrect probability of middle outcome.");

            bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.WhenEmpty);
            bag.Add("1", 10);
            bag.Add("2", 30);
            bag.Add("3", 60);

            simulation = new QualitativeSimulation(bag);
            results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(results.MostCommonOutcome.Outcome, "3", "Incorrect most common outcome.");
            Assert.AreEqual(results.LeastCommonOutcome.Outcome, "1", "Incorrect least common outcome.");
            Assert.AreEqual(results.IndividualOutcomes[1].Probability, 0.30, 0.01, "Incorrect probability of middle outcome.");
        }

        [TestMethod]
        public void Qualitative_RandomBag_EditItems()
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("bag", RandomBagReplacement.AfterEachPick);
            bag.Add("1", 100);
            bag.Remove("1", 50);
            Assert.AreEqual(bag.Contents["1"], 50, "Removal not successful.");

            bag.Remove("1", 100);
            bag.Remove("2", 50);
            Assert.IsTrue(bag.IsEmpty, "Bag not empty.");

            bag.Add("3", 50);
            bag.Add("4", 100);
            bag.RemoveAll("5");
            bag.RemoveAll("4");
            Assert.AreEqual(bag.NumberOfItems, 50, "Incorrect number of items.");

            bag.Empty();
            Assert.IsTrue(bag.IsEmpty, "Bag not empty.");

            bag.Add("1", 50);
            bag.Add("1", 50);

            QualitativeSimulation simulation = new QualitativeSimulation(bag);
            QualitativeSimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(results.MostCommonOutcome.Outcome, "1", "Incorrect most common outcome.");
        }
    }
}
