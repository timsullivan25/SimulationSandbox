using Simulations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Symbolics;
using MathNet.Numerics.Distributions;

namespace SimulationTests
{
    [TestClass]
    public class SensitivitySimulationTests
    {
        private static int _numberOfSimulations = 1000;

        [TestMethod]
        public void Sensitivity_CAPM()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025 })
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);
            SensitivitySimulation sensitivity = new SensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(3, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_CAPM_Exhaustive()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025 })
            };

            string expression = "Rf + B * (Rm - Rf)";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(9, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_CAPM_VeryExhaustive()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            string expression = "Rf + B * (Rm - Rf) + a";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_Recompute()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            string expression = "Rf + B * (Rm - Rf) + a";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            expression = "Rf + B * (Rm - Rf) + 3 * a";
            results = results.RecomputeExpression(expression);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_Regenerate()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            string expression = "Rf + B * (Rm - Rf) + a";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            results = results.Regenerate();
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            results = results.Regenerate(100);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(100, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_AddAndRemoveParameters()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { 1, 2, 3}),
                new DistributionParameter("b", new Normal(1, 1)),
                new DistributionParameter("c", new Normal(2, 1)),
                new PrecomputedParameter("d", new FloatingPoint[] { 10, 20, 30}),
                new DistributionParameter("e", new Normal(4, 1))
            };

            string expression = "a + b + c";
            Simulation simulation = new Simulation(expression, parameters);
            SensitivitySimulation sensitivity = new SensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(3, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

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
            Assert.AreEqual(3, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_AddAndRemovePrecomputedParameters()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { 1, 2, 3}),
                new DistributionParameter("b", new Normal(1, 1)),
                new DistributionParameter("c", new Normal(2, 1)),
                new PrecomputedParameter("d", new FloatingPoint[] { 10, 20, 30}),
                new DistributionParameter("e", new Normal(4, 1))
            };

            string expression = "a + b + c";
            Simulation simulation = new Simulation(expression, parameters);
            SensitivitySimulation sensitivity = new SensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate(_numberOfSimulations);
            Assert.AreEqual(3, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

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
            results.AddParameter(new PrecomputedParameter("f", new FloatingPoint[] { 5, 10, 15}));
            results.RecomputeExpression("a + b + c + f");
            Assert.AreEqual(3, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_Multithreaded()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            string expression = "Rf + B * (Rm - Rf) + a";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate_Multithreaded(_numberOfSimulations);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_MultithreadedRegeneration()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            string expression = "Rf + B * (Rm - Rf) + a";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate_Multithreaded(_numberOfSimulations);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            results = results.Regenerate();
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            results = results.Regenerate(100);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(100, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }

        [TestMethod]
        public void Sensitivity_ReplaceParameter()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            string expression = "Rf + B * (Rm - Rf) + a";
            Simulation simulation = new Simulation(expression, parameters);
            ExhaustiveSensitivitySimulation sensitivity = new ExhaustiveSensitivitySimulation(simulation);
            SensitivitySimulationResults results = sensitivity.Simulate_Multithreaded(_numberOfSimulations);
            Assert.AreEqual(36, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            results.ReplaceParameter(parameters[1], new DistributionParameter("B", new Normal(1, 0)));
            Assert.AreEqual(12, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");

            results.ReplaceParameter(parameters[0], new DistributionParameter("alpha", new Normal(3, 0)));
            Assert.AreEqual(4, results.Results.Count, "Incorrect number of simulations was performed.");
            Assert.AreEqual(_numberOfSimulations, results.Results.Values.First().NumberOfSimulations, "Incorrect number of simulations was performed.");
        }
    }
}
