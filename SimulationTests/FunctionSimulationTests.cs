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

            // test passing constructor
            FunctionSimulation<ICollection<double>, FunctionSimulation<double, double>> simulationConstructor =
                new FunctionSimulation<ICollection<double>, FunctionSimulation<double, double>>(CreateListOfInputs, results);

            Assert.IsTrue(simulationConstructor.Simulate(1)[0] is FunctionSimulation<double, double>);
        }

        private double Mirror(double simulationAverage)
        {
            return simulationAverage;
        }

        private double AvgAvg(Simulation simulation)
        {
            return simulation.Simulate(1000).Mean;
        }

        private FunctionSimulation<double, double> CreateListOfInputs(ICollection<double> inputs)
        {
            return new FunctionSimulation<double, double>(Mirror, inputs);
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
                new FunctionSimulation<string, int, int, int>(CoinFlip, coinFlip.Results, 3, 4);

            Assert.AreEqual(0, simulation.Simulate(1000).Average(r => r), 0.1);
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

        #region T5

        [TestMethod]
        public void T5()
        {
            FunctionSimulation<int, int, int, int, int, int> simulation = new FunctionSimulation<int,int,int,int,int,int>(TestT5, 1, 2, 3, 4, 5);
            Assert.AreEqual(15, simulation.Simulate(1)[0]);
        }

        private int TestT5(int p1, int p2, int p3, int p4, int p5)
        {
            return p1 + p2 + p3 + p4 + p5;
        }

        #endregion

        #region T6

        [TestMethod]
        public void T6()
        {
            FunctionSimulation<int, int, int, int, int, int, int> simulation = new FunctionSimulation<int, int, int, int, int, int, int>(TestT6, 1, 2, 3, 4, 5, 6);
            Assert.AreEqual(21, simulation.Simulate(1)[0]);
        }

        private int TestT6(int p1, int p2, int p3, int p4, int p5, int p6)
        {
            return p1 + p2 + p3 + p4 + p5 + p6;
        }

        #endregion

        #region T7

        [TestMethod]
        public void T7()
        {
            FunctionSimulation<int, int, int, int, int, int, int, int> simulation = 
                new FunctionSimulation<int, int, int, int, int, int, int, int>(TestT7, 1, 2, 3, 4, 5, 6, 7);
            Assert.AreEqual(28, simulation.Simulate(1)[0]);
        }

        private int TestT7(int p1, int p2, int p3, int p4, int p5, int p6, int p7)
        {
            return p1 + p2 + p3 + p4 + p5 + p6 + p7;
        }

        #endregion

        #region T8

        [TestMethod]
        public void T8()
        {
            FunctionSimulation<int, int, int, int, int, int, int, int, int> simulation =
                new FunctionSimulation<int, int, int, int, int, int, int, int, int>(TestT8, 1, 2, 3, 4, 5, 6, 7, 8);
            Assert.AreEqual(36, simulation.Simulate(1)[0]);
        }

        private int TestT8(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8)
        {
            return p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8;
        }

        #endregion

        #region T9

        [TestMethod]
        public void T9()
        {
            FunctionSimulation<int, int, int, int, int, int, int, int, int, int> simulation =
                new FunctionSimulation<int, int, int, int, int, int, int, int, int, int>(TestT9, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            Assert.AreEqual(45, simulation.Simulate(1)[0]);
        }

        private int TestT9(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9)
        {
            return p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
        }

        #endregion

        #region T10

        [TestMethod]
        public void T10()
        {
            FunctionSimulation<int, int, int, int, int, int, int, int, int, int, int> simulation =
                new FunctionSimulation<int, int, int, int, int, int, int, int, int, int, int>(TestT10, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
            Assert.AreEqual(55, simulation.Simulate(1)[0]);
        }

        private int TestT10(int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10)
        {
            return p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10;
        }

        #endregion

        #region NumericSimulationResults

        [TestMethod]
        public void NumericSimulationResults()
        {
            var simulation = new FunctionSimulation<int>(Five);
            var results = new NumericSimulationResults<int>(simulation.Simulate(100));

            Assert.AreEqual(100, results.NumberOfSimulations);
            Assert.AreEqual(5, results.Mean, 0.01);
            Assert.AreEqual(5, results.Median, 0.01);
            Assert.AreEqual(5, results.Maximum, 0.01);
            Assert.AreEqual(5, results.Minimum, 0.01);
        }

        #endregion
    }
}
