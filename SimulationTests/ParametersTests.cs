using Simulations;
using Simulations.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;
using System.Collections.Generic;

namespace SimulationTests
{
    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        public void Parameters_Iteration_Linear_Inclusive()
        {
            IterationParameter iterationParameter = new IterationParameter("x", 10, 110, 10, InterpolationType.Linear, true);
            double[] values = iterationParameter.GenerateValues();

            Assert.AreEqual(10, values.Length, "Incorrect number of values generated.");
            Assert.AreEqual(10.0, values[0], 0.1, "Incorrect first value.");
            Assert.AreEqual(21.1, values[1], 0.1, "Incorrect second value.");
            Assert.AreEqual(32.2, values[2], 0.1, "Incorrect third value.");
            Assert.AreEqual(43.3, values[3], 0.1, "Incorrect fourth value.");
            Assert.AreEqual(54.4, values[4], 0.1, "Incorrect fifth value.");
            Assert.AreEqual(65.6, values[5], 0.1, "Incorrect sixth value.");
            Assert.AreEqual(76.7, values[6], 0.1, "Incorrect seventh value.");
            Assert.AreEqual(87.8, values[7], 0.1, "Incorrect eighth value.");
            Assert.AreEqual(98.9, values[8], 0.1, "Incorrect ninth value.");
            Assert.AreEqual(110.0, values[9], 0.1, "Incorrect last value.");
        }

        [TestMethod]
        public void Parameters_Iteration_Linear_Exclusive()
        {
            IterationParameter iterationParameter = new IterationParameter("x", 10, 110, 10, InterpolationType.Linear, false);
            double[] values = iterationParameter.GenerateValues();

            Assert.AreEqual(10, values.Length, "Incorrect number of values generated.");
            Assert.AreEqual(19.1, values[0], 0.1, "Incorrect first value.");
            Assert.AreEqual(28.2, values[1], 0.1, "Incorrect second value.");
            Assert.AreEqual(37.3, values[2], 0.1, "Incorrect third value.");
            Assert.AreEqual(46.4, values[3], 0.1, "Incorrect fourth value.");
            Assert.AreEqual(55.5, values[4], 0.1, "Incorrect fifth value.");
            Assert.AreEqual(64.5, values[5], 0.1, "Incorrect sixth value.");
            Assert.AreEqual(73.6, values[6], 0.1, "Incorrect seventh value.");
            Assert.AreEqual(82.7, values[7], 0.1, "Incorrect eighth value.");
            Assert.AreEqual(91.8, values[8], 0.1, "Incorrect ninth value.");
            Assert.AreEqual(100.9, values[9], 0.1, "Incorrect last value.");
        }

        [TestMethod]
        public void Parameters_Iteration_Exponential_Inclusive()
        {
            IterationParameter iterationParameter = new IterationParameter("x", 10, 110, 10, InterpolationType.Exponential, true);
            double[] values = iterationParameter.GenerateValues();

            Assert.AreEqual(10, values.Length, "Incorrect number of values generated.");
            Assert.AreEqual(10.0, values[0], 0.1, "Incorrect first value.");
            Assert.AreEqual(13.1, values[1], 0.1, "Incorrect second value.");
            Assert.AreEqual(17.0, values[2], 0.1, "Incorrect third value.");
            Assert.AreEqual(22.2, values[3], 0.1, "Incorrect fourth value.");
            Assert.AreEqual(29.0, values[4], 0.1, "Incorrect fifth value.");
            Assert.AreEqual(37.9, values[5], 0.1, "Incorrect sixth value.");
            Assert.AreEqual(49.5, values[6], 0.1, "Incorrect seventh value.");
            Assert.AreEqual(64.6, values[7], 0.1, "Incorrect eighth value.");
            Assert.AreEqual(84.3, values[8], 0.1, "Incorrect ninth value.");
            Assert.AreEqual(110.0, values[9], 0.1, "Incorrect last value.");
        }

        [TestMethod]
        public void Parameters_Iteration_Exponential_Exclusive()
        {
            IterationParameter iterationParameter = new IterationParameter("x", 10, 110, 10, InterpolationType.Exponential, false);
            double[] values = iterationParameter.GenerateValues();

            Assert.AreEqual(10, values.Length, "Incorrect number of values generated.");
            Assert.AreEqual(12.4, values[0], 0.1, "Incorrect first value.");
            Assert.AreEqual(15.5, values[1], 0.1, "Incorrect second value.");
            Assert.AreEqual(19.2, values[2], 0.1, "Incorrect third value.");
            Assert.AreEqual(23.9, values[3], 0.1, "Incorrect fourth value.");
            Assert.AreEqual(29.7, values[4], 0.1, "Incorrect fifth value.");
            Assert.AreEqual(37.0, values[5], 0.1, "Incorrect sixth value.");
            Assert.AreEqual(46.0, values[6], 0.1, "Incorrect seventh value.");
            Assert.AreEqual(57.2, values[7], 0.1, "Incorrect eighth value.");
            Assert.AreEqual(71.1, values[8], 0.1, "Incorrect ninth value.");
            Assert.AreEqual(88.5, values[9], 0.1, "Incorrect last value.");
        }

        [TestMethod]
        public void Parameters_Iteration_Log_Inclusive()
        {
            IterationParameter iterationParameter = new IterationParameter("x", 10, 110, 10, InterpolationType.Log, true);
            double[] values = iterationParameter.GenerateValues();

            Assert.AreEqual(10, values.Length, "Incorrect number of values generated.");
            Assert.AreEqual(10.0, values[0], 0.1, "Incorrect first value.");
            Assert.AreEqual(40.1, values[1], 0.1, "Incorrect second value.");
            Assert.AreEqual(57.7, values[2], 0.1, "Incorrect third value.");
            Assert.AreEqual(70.2, values[3], 0.1, "Incorrect fourth value.");
            Assert.AreEqual(79.9, values[4], 0.1, "Incorrect fifth value.");
            Assert.AreEqual(87.8, values[5], 0.1, "Incorrect sixth value.");
            Assert.AreEqual(94.5, values[6], 0.1, "Incorrect seventh value.");
            Assert.AreEqual(100.3, values[7], 0.1, "Incorrect eighth value.");
            Assert.AreEqual(105.4, values[8], 0.1, "Incorrect ninth value.");
            Assert.AreEqual(110.0, values[9], 0.1, "Incorrect last value.");
        }

        [TestMethod]
        public void Parameters_Iteration_Log_Exclusive()
        {
            IterationParameter iterationParameter = new IterationParameter("x", 10, 110, 10, InterpolationType.Log, false);
            double[] values = iterationParameter.GenerateValues();

            Assert.AreEqual(10, values.Length, "Incorrect number of values generated.");
            Assert.AreEqual(37.9, values[0], 0.1, "Incorrect first value.");
            Assert.AreEqual(54.2, values[1], 0.1, "Incorrect second value.");
            Assert.AreEqual(65.8, values[2], 0.1, "Incorrect third value.");
            Assert.AreEqual(74.8, values[3], 0.1, "Incorrect fourth value.");
            Assert.AreEqual(82.1, values[4], 0.1, "Incorrect fifth value.");
            Assert.AreEqual(88.3, values[5], 0.1, "Incorrect sixth value.");
            Assert.AreEqual(93.7, values[6], 0.1, "Incorrect seventh value.");
            Assert.AreEqual(98.4, values[7], 0.1, "Incorrect eighth value.");
            Assert.AreEqual(102.7, values[8], 0.1, "Incorrect ninth value.");
            Assert.AreEqual(106.5, values[9], 0.1, "Incorrect last value.");
        }
    }
}
