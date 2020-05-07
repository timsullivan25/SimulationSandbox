using Simulations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;
using MathNet.Symbolics;

namespace SimulationTests
{
    [TestClass]
    public class SimulationTests
    {
        private static int _numberOfSimulations = 1000;

        [TestMethod]
        public void Simulation_DoubleDiceRoll()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DiscreteParameter("DiceRoll1", new DiscreteOutcome(1, 0.167),
                                                   new DiscreteOutcome(2, 0.167),
                                                   new DiscreteOutcome(3, 0.167),
                                                   new DiscreteOutcome(4, 0.167),
                                                   new DiscreteOutcome(5, 0.167),
                                                   new DiscreteOutcome(6, 0.167)),

                new DiscreteParameter("DiceRoll2", new DiscreteOutcome(1, 0.167),
                                                   new DiscreteOutcome(2, 0.167),
                                                   new DiscreteOutcome(3, 0.167),
                                                   new DiscreteOutcome(4, 0.167),
                                                   new DiscreteOutcome(5, 0.167),
                                                   new DiscreteOutcome(6, 0.167))
            };

            string expression = "DiceRoll1 + DiceRoll2";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_TwentySidedDie()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("twentysided", new DiscreteUniform(1, 20))
            };

            string expression = "twentysided";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_ScaledTenSidedDie()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("scalevalue", 10),
                new SimulationParameter("tensidedsimulation",
                                         new Simulation("dieroll",
                                                        new DistributionParameter("dieroll", new DiscreteUniform(1, 10))),
                                         SimulationParameterReturnType.Results)
            };

            string expression = "scalevalue * tensidedsimulation";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_CAPM()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_ConditionalParameter()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConditionalParameter(name: "Action",
                                         referenceParameter: new DiscreteParameter("CoinFlip",
                                                                                    new DiscreteOutcome(0, 0.50),
                                                                                    new DiscreteOutcome(1, 0.50)),
                                         defaultValue: 0,
                                         conditionalOutcomes: new ConditionalOutcome[] {
                                                                                            new ConditionalOutcome(ComparisonOperator.GreaterThan, 0.50, 10),
                                                                                            new ConditionalOutcome(ComparisonOperator.LessThanOrEqual, 0.50, 5)
                                                                                       })
            };

            string expression = "Action";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_ConditionalIntegerParameter()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConditionalParameter(name: "Action",
                                         referenceParameter: new DiscreteParameter("CoinFlip",
                                                                                    new DiscreteOutcome(1, 0.50),
                                                                                    new DiscreteOutcome(2, 0.50)),
                                         defaultValue: 0,
                                         conditionalOutcomes: new ConditionalOutcome[] {
                                                                                            new ConditionalOutcome(ComparisonOperator.Equal, 1, 10),
                                                                                            new ConditionalOutcome(ComparisonOperator.Equal, 2, 100)
                                                                                       })
            };

            string expression = "Action / 10";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_IterationParameterValidationError()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("C", 10),
                new IterationParameter("I", 10, 100, 10, InterpolationType.Log, false)
            };

            string expression = "C * I";
            Simulation simulation = new Simulation(expression, parameters);

            try
            {
                SimulationResults results = simulation.Simulate(100);
                Assert.Fail("Exception was not thrown.");
            }
            catch (Simulations.Exceptions.IterationStepCountException)
            {
                // passed
            }
            catch
            {
                Assert.Fail("Unexpected exception thrown.");
            }
        }

        [TestMethod]
        public void Simulation_IterationParameter()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("C", 10),
                new IterationParameter("I", 10, 110, 10, InterpolationType.Log, false)
            };

            string expression = "C * I";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(10);
            Assert.AreEqual(10, results.NumberOfSimulations, "Incorrect number of similations run.");
            Assert.AreEqual(883.1, results.Results[5], 1.0, "Incorrect value calculated.");
        }

        [TestMethod]
        public void Simulation_PrecomputedValidationError()
        {
            FloatingPoint[] precomputedValues = new FloatingPoint[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("C", 10),
                new PrecomputedParameter("P", precomputedValues)
            };

            string expression = "C * P";
            Simulation simulation = new Simulation(expression, parameters);

            try
            {
                SimulationResults results = simulation.Simulate(_numberOfSimulations);
                Assert.Fail("Exception was not thrown.");
            }
            catch (Simulations.Exceptions.PrecomputedValueCountException)
            {
                // passed
            }
            catch
            {
                Assert.Fail("Unexpected exception thrown.");
            }
        }

        [TestMethod]
        public void Simulation_PrecomputedTest()
        {
            FloatingPoint[] precomputedValues = new FloatingPoint[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("C", 10),
                new PrecomputedParameter("P", precomputedValues)
            };

            string expression = "C * P";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(10);
            Assert.AreEqual(10, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_Recompute()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            expression = "Rf";
            results = results.RecomputeExpression(expression);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed when recomputing.");

            expression = "2 * (Rf + B * (Rm - Rf))";
            results = results.RecomputeExpression(expression);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed when recomputing.");
        }

        [TestMethod]
        public void Simulation_Regenerate()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            results = results.Regenerate();
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed when regenerating.");

            results = results.Regenerate(10);
            Assert.AreEqual(10, results.NumberOfSimulations, "Incorrect number of simulations was performed when regenerating.");
        }

        [TestMethod]
        public void Simulation_AddAndRemoveParameters()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("a", new Normal(0, 1)),
                new DistributionParameter("b", new Normal(1, 1)),
                new DistributionParameter("c", new Normal(2, 1)),
                new DistributionParameter("d", new Normal(3, 1)),
                new DistributionParameter("e", new Normal(4, 1))
            };

            string expression = "a + b + c";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            try
            {
                results.RemoveParameter(parameters[1]);
                Assert.Fail("Exception was not thrown.");
            }
            catch (Simulations.Exceptions.ParameterInExpressionException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Unexpected exception thrown.");
            }

            results.RemoveParameter(parameters[4]);
            results.AddParameter(new DistributionParameter("f", new Normal(5, 1)));
            results.RecomputeExpression("a + b + c + f");
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed when recomputing after adding parameter.");
        }

        [TestMethod]
        public void Simulation_ReplaceParameter()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("a", 1),
                new ConstantParameter("b", 2),
                new ConstantParameter("c", 3)
            };

            string expression = "a + b + c";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(6, results.Results[0], "Expression value does not match expected output.");

            results.ReplaceParameter(parameters[2], new ConstantParameter("c", 0));
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(3, results.Results[0], "Expression value does not match expected output.");

            results.ReplaceParameter(parameters[2], new ConstantParameter("d", 10));
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(13, results.Results[0], "Expression value does not match expected output.");
        }

        [TestMethod]
        public void Simulation_DependentSimulationParameter_Results()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("scalevalue", 10),
                new DependentSimulationParameter("stockprices", 
                                                  new DependentSimulation(100,
                                                                          "value * dailyincrease",
                                                                          new ConstantParameter("dailyincrease", 1.01)),
                                                  DependentSimulationParameterReturnType.Results)
            };

            string expression = "stockprices * scalevalue";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(1000);
            Assert.AreEqual(1000, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_DependentSimulationParameter_SummaryStatistics()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("scalevalue", 10),
                new DependentSimulationParameter("stockprices",
                                                  new DependentSimulation(100,
                                                                          "value * dailyincrease",
                                                                          new ConstantParameter("dailyincrease", 1.01)),
                                                  DependentSimulationParameterReturnType.AverageChange,
                                                  365)
            };

            string expression = "stockprices * scalevalue";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(10);
            Assert.AreEqual(10, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_SimulationParameter_SummaryStatistics()
        {
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("scalevalue", 10),
                new SimulationParameter("tensidedsimulation",
                                         new Simulation("dieroll",
                                                        new DistributionParameter("dieroll", new DiscreteUniform(1, 10))),
                                         SimulationParameterReturnType.Mean,
                                         1000)
            };

            string expression = "scalevalue * tensidedsimulation";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(10);
            Assert.AreEqual(10, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_AverageAverageValues()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);

            // simulate the simulation
            Simulation outerSimulation = new Simulation("CAPM", new SimulationParameter("CAPM", simulation, SimulationParameterReturnType.Mean, 1000));
            SimulationResults results = outerSimulation.Simulate(10000);
            Assert.AreEqual(10000, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Simulation_ConfidenceInterval()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.25)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");

            ConfidenceInterval confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._80);
            confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._85);
            confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._90);
            confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._95);
            confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._99);
            confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._99_5);
            confidenceInterval = results.ConfidenceInterval(ConfidenceLevel._99_9);
        }

        [TestMethod]
        public void Simulation_DistributionConstraint_Resimulaton_Integer()
        {
            DistributionParameter param = new DistributionParameter("uniform",
                                                                     new DiscreteUniform(0, 1),
                                                                     new ParameterConstraint(null, -1, ConstraintViolationResolution.Resimulate, 10, -99));

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(-99, results.Maximum, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DistributionConstraint_Resimulation_Double()
        {
            DistributionParameter param = new DistributionParameter("uniform",
                                                                     new DiscreteUniform(0, 1),
                                                                     new ParameterConstraint(2, null, ConstraintViolationResolution.Resimulate, 10, 99));

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(99, results.Minimum, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DistributionConstraint_SetToBound_Integer()
        {
            DistributionParameter param = new DistributionParameter("uniform",
                                                                     new DiscreteUniform(0, 1),
                                                                     new ParameterConstraint(null, -1, ConstraintViolationResolution.ClosestBound));

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(-1, results.Maximum, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DistributionConstraint_SetToDefault_Double()
        {
            DistributionParameter param = new DistributionParameter("uniform",
                                                                     new DiscreteUniform(0, 1),
                                                                     new ParameterConstraint(2, null, ConstraintViolationResolution.DefaultValue, defaultValue: 50));

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(50, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_SimulationConstraint_Resimulation_Results()
        {
            SimulationParameter param = new SimulationParameter("uniform",
                                                                new Simulation("diceroll", new DistributionParameter("diceroll", new DiscreteUniform(1, 6))),
                                                                new ParameterConstraint(7, null, ConstraintViolationResolution.Resimulate, 10, 0),
                                                                SimulationParameterReturnType.Results,
                                                                100);
                                                                 
            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(0, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_SimulationConstraint_Resimulation_SummaryStatistic()
        {
            SimulationParameter param = new SimulationParameter("uniform",
                                                                new Simulation("diceroll", new DistributionParameter("diceroll", new DiscreteUniform(1, 6))),
                                                                new ParameterConstraint(null, 0, ConstraintViolationResolution.Resimulate, 10, -10),
                                                                SimulationParameterReturnType.Minimum,
                                                                100);

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(-10, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_SimulationConstraint_SetToBound_Results()
        {
            SimulationParameter param = new SimulationParameter("uniform",
                                                                new Simulation("diceroll", new DistributionParameter("diceroll", new DiscreteUniform(1, 6))),
                                                                new ParameterConstraint(7, null, ConstraintViolationResolution.ClosestBound, 10, 0),
                                                                SimulationParameterReturnType.Results,
                                                                100);

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(7, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_SimulationConstraint_SetToBound_SummaryStatistic()
        {
            SimulationParameter param = new SimulationParameter("uniform",
                                                                new Simulation("diceroll", new DistributionParameter("diceroll", new DiscreteUniform(1, 6))),
                                                                new ParameterConstraint(null, 0, ConstraintViolationResolution.ClosestBound, 10, -10),
                                                                SimulationParameterReturnType.Minimum,
                                                                100);

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(0, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_SimulationConstraint_SetToDefault_Results()
        {
            SimulationParameter param = new SimulationParameter("uniform",
                                                                new Simulation("diceroll", new DistributionParameter("diceroll", new DiscreteUniform(1, 6))),
                                                                new ParameterConstraint(7, null, ConstraintViolationResolution.DefaultValue, 10, 99),
                                                                SimulationParameterReturnType.Results,
                                                                100);

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(99, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_SimulationConstraint_SetToDefault_SummaryStatistic()
        {
            SimulationParameter param = new SimulationParameter("uniform",
                                                                new Simulation("diceroll", new DistributionParameter("diceroll", new DiscreteUniform(1, 6))),
                                                                new ParameterConstraint(null, 0, ConstraintViolationResolution.DefaultValue, 10, -99),
                                                                SimulationParameterReturnType.Minimum,
                                                                100);

            Simulation simulation = new Simulation("uniform", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(-99, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DependentSimulationConstraint_Resimulation_Results()
        {
            DependentSimulationParameter param = new DependentSimulationParameter("simulation", 
                                                                                  new DependentSimulation(100, "value + one", new ConstantParameter("one", 1)),
                                                                                  new ParameterConstraint(null, 100, ConstraintViolationResolution.Resimulate, 10, 0),
                                                                                  DependentSimulationParameterReturnType.Results,
                                                                                  100);

            Simulation simulation = new Simulation("simulation", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreNotEqual(0, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DependentSimulationConstraint_Resimulation_SummaryStatistic()
        {
            DependentSimulationParameter param = new DependentSimulationParameter("simulation",
                                                                                  new DependentSimulation(100, "value + one", new ConstantParameter("one", 1)),
                                                                                  new ParameterConstraint(null, 100, ConstraintViolationResolution.Resimulate, 10, 0),
                                                                                  DependentSimulationParameterReturnType.EndingValue,
                                                                                  100);

            Simulation simulation = new Simulation("simulation", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(0, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DependentSimulationConstraint_SetToBound_SummaryStatistic()
        {
            DependentSimulationParameter param = new DependentSimulationParameter("simulation",
                                                                                  new DependentSimulation(100, "value + one", new ConstantParameter("one", 1)),
                                                                                  new ParameterConstraint(null, 100, ConstraintViolationResolution.ClosestBound, 10, 0),
                                                                                  DependentSimulationParameterReturnType.EndingValue,
                                                                                  100);

            Simulation simulation = new Simulation("simulation", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(100, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_DependentSimulationConstraint_SetToDefault_SummaryStatistic()
        {
            DependentSimulationParameter param = new DependentSimulationParameter("simulation",
                                                                                  new DependentSimulation(100, "value + one", new ConstantParameter("one", 1)),
                                                                                  new ParameterConstraint(5000, null, ConstraintViolationResolution.DefaultValue, 10, 99),
                                                                                  DependentSimulationParameterReturnType.EndingValue,
                                                                                  100);

            Simulation simulation = new Simulation("simulation", param);
            SimulationResults results = simulation.Simulate(100);
            Assert.AreEqual(100, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(99, results.Mean, 0.01, "Incorrect simulation results.");
        }

        [TestMethod]
        public void Simulation_RandomBag_ReplaceAfterEveryPick()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.AfterEachPick);
            bag.Add(1.0, 100);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(1.0, results.Mean, 0.01, "Incorrect mean result value.");
        }

        [TestMethod]
        public void Simulation_RandomBag_ReplaceNever()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.Never);
            bag.Add(1.0, 100);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);

            try
            {
                SimulationResults results = simulation.Simulate(_numberOfSimulations);
            }
            catch (Simulations.Exceptions.RandomBagItemCountException)
            {
                // success
            }
            catch
            {
                Assert.Fail("Caught unexpected error on random bag without enough items to use Never replacement strategy.");
            }

            SimulationResults realResults = simulation.Simulate(100);
            Assert.AreEqual(100, realResults.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(1.0, realResults.Mean, 0.01, "Incorrect mean result value.");
        }

        [TestMethod]
        public void Simulation_RandomBag_ReplaceWhenEmpty()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.WhenEmpty);
            bag.Add(1.0, 100);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(1.0, results.Mean, 0.01, "Incorrect mean result value.");
        }

        [TestMethod]
        public void Simulation_RandomBag_EmptyBagError()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.AfterEachPick);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);

            try
            {
                SimulationResults results = simulation.Simulate(_numberOfSimulations);
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
        public void Simulation_RandomBag_ReplaceNever_MultipleValues()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.Never);
            bag.Add(1.0, 100);
            bag.Add(2.0, 300);
            bag.Add(3.0, 600);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(2.5, results.Mean, 0.01, "Incorrect mean result value.");
        }

        [TestMethod]
        public void Simulation_RandomBag_ReplaceWhenEmpty_MultipleValues()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.WhenEmpty);
            bag.Add(1.0, 10);
            bag.Add(2.0, 30);
            bag.Add(3.0, 60);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(2.5, results.Mean, 0.01, "Incorrect mean result value.");

            bag = new RandomBagParameter("bag", RandomBagReplacement.WhenEmpty);
            bag.Add(1.0, 100);
            bag.Add(2.0, 300);
            bag.Add(3.0, 600);

            parameters = new IParameter[]
            {
                bag
            };

            simulation = new Simulation(expression, parameters);
            results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(2.5, results.Mean, 0.01, "Incorrect mean result value.");
        }

        [TestMethod]
        public void Simulation_RandomBag_EditItems()
        {
            RandomBagParameter bag = new RandomBagParameter("bag", RandomBagReplacement.AfterEachPick);
            bag.Add(1.0, 100);
            bag.Remove(1.0, 50);
            Assert.AreEqual(bag.Contents[1.0], 50, "Removal not successful.");

            bag.Remove(1.0, 100);
            bag.Remove(2.0, 50);
            Assert.IsTrue(bag.IsEmpty, "Bag not empty.");

            bag.Add(3, 50);
            bag.Add(4, 100);
            bag.RemoveAll(5);
            bag.RemoveAll(4);
            Assert.AreEqual(bag.NumberOfItems, 50, "Incorrect number of items.");

            bag.Empty();
            Assert.IsTrue(bag.IsEmpty, "Bag not empty.");

            bag.Add(1.0, 50);
            bag.Add(1.0, 50);

            IParameter[] parameters = new IParameter[]
            {
                bag
            };

            string expression = "bag";
            Simulation simulation = new Simulation(expression, parameters);
            SimulationResults results = simulation.Simulate(_numberOfSimulations);
            Assert.AreEqual(_numberOfSimulations, results.NumberOfSimulations, "Incorrect number of simulations was performed.");
            Assert.AreEqual(1.0, results.Mean, 0.01, "Incorrect mean result value.");
        }
    }
}
