using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulations;
using Simulations.Templates;
using MathNet.Numerics.Distributions;

namespace SimulationTests
{
    [TestClass]
    public class FunctionSimulationTests
    {
        #region T0

        [TestMethod]
        public void T0()
        {
            FunctionSimulation<int> simulation = new FunctionSimulation<int>(Five);
            Assert.AreEqual(5, simulation.Simulate(1)[0]);
        }

        private int Five()
        {
            return 5;
        }

        #endregion

        #region T1

        [TestMethod]
        public void T1()
        {
            // test IParameter functionality
            var param = 
                new SimulationParameter("avg",
                    new Simulation("norm",
                        new DistributionParameter("norm", new Normal(5, 1))),
                    SimulationParameterReturnType.Mean,
                    1000);

            FunctionSimulation<double, double> simulation = new FunctionSimulation<double, double>(Mirror, param);
            Assert.AreEqual(5, simulation.Simulate(1)[0], 0.1);

            // test IParameter passthrough functionality
            FunctionSimulation<Simulation, double> simulationPassthrough = 
                new FunctionSimulation<Simulation, double>(AvgAvg, 
                    new Simulation("value", new DistributionParameter("value", new Normal(1, 0))));

            double[] results = simulationPassthrough.Simulate(1000);
            Assert.AreEqual(1, results.Average(r => r), 0.1);

            // test direct distribution parameter
            FunctionSimulation<double, double> simulationDirectDistribution =
                new FunctionSimulation<double, double>(Mirror, new Triangular(10, 20, 15));

            double[] results2 = simulationDirectDistribution.Simulate(1000);
            Assert.AreEqual(15, results2.Average(r => r), 0.25);
        }

        private double Mirror(double simulationAverage)
        {
            return simulationAverage;
        }

        private double AvgAvg(Simulation simulation)
        {
            return simulation.Simulate(1000).Mean;
        }

        #endregion

        #region T2

        [TestMethod]
        public void T2()
        {
            FunctionSimulation<double, double, double> simulation = new FunctionSimulation<double, double, double>(PythagoreanTheorum, 3d, 4d);
            Assert.AreEqual(5, simulation.Simulate(1)[0], 0.0001);
            Assert.AreEqual(10, simulation.Simulate(10).Length);
        }

        public double PythagoreanTheorum(double a, double b)
        {
            double c2 = Math.Pow(a, 2) + Math.Pow(b, 2);
            return Math.Sqrt(c2);
        }

        #endregion

        #region T3

        [TestMethod]
        public void T3()
        {
            QualitativeSimulationResults coinFlip = QualitativeTemplates.CoinFlip(1000);

            FunctionSimulation<string, int, int, int> simulation = 
                new FunctionSimulation<string, int, int, int>(CoinFlip, new ListOfInputs(coinFlip.Results), 3, 4);

            Assert.AreEqual(0, simulation.Simulate(100).Average(r => r), 0.1);
            Assert.AreEqual(25, simulation.Simulate(25).Length);
        }

        public int CoinFlip(string headsOrTails, int a, int b)
        {
            return headsOrTails == "Heads" ? a - b : b - a;
        }

        #endregion

        #region T4

        [TestMethod]
        public void T4()
        {
            var rf = new DistributionParameter("rf", new Normal(0.01, 0.05), 
                new ParameterConstraint(0d, null, ConstraintViolationResolution.DefaultValue, 0, 0.01));

            var rm = new DistributionParameter("rm", new Triangular(0.03, 0.07, 0.05));
            var e = new FunctionSimulation<int>(() => new Random().Next());

            FunctionSimulation<double,double,double,int,double> simulation = 
                new FunctionSimulation<double,double,double,int,double>(
                    CAPM, rf, 0.9d, rm, e);

            Assert.AreEqual(100, simulation.Simulate(100).Length);
        }

        private double CAPM(double rf, double B, double rm, int e)
        {
            return rf + B * (rm - rf) + e;
        }

        #endregion
    }
}
