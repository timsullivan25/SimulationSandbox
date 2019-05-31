using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Symbolics;
using System.Collections.Generic;

namespace SimulationTests
{
    [TestClass]
    public class MathNetSymbolicsTests
    {
        [TestMethod]
        public void E()
        {
            Expression expression = Infix.ParseOrThrow("e");
            double result = Evaluate.Evaluate(null, expression).RealValue;
            Assert.AreEqual(2.718, result, 0.01, "Did not correctly compute Euler's constant.");
        }

        [TestMethod]
        public void Ln()
        {
            Expression expression = Infix.ParseOrThrow("ln(e)");
            double result = Evaluate.Evaluate(null, expression).RealValue;
            Assert.AreEqual(1.00, result, 0.01, "Did not correctly compute natural log.");
        }

        [TestMethod]
        public void Sqrt()
        {
            Expression expression = Infix.ParseOrThrow("sqrt(16)");
            double result = Evaluate.Evaluate(null, expression).RealValue;
            Assert.AreEqual(4.00, result, 0.01, "Did not correctly compute square root.");
        }

        [TestMethod]
        public void Exponent()
        {
            Expression expression = Infix.ParseOrThrow("5^2");
            double result = Evaluate.Evaluate(null, expression).RealValue;
            Assert.AreEqual(25.00, result, 0.01, "Did not correctly compute exponent.");
        }

        [TestMethod]
        public void Exp()
        {
            Expression expression = Infix.ParseOrThrow("exp(5)");
            double result = Evaluate.Evaluate(null, expression).RealValue;
            Assert.AreEqual(148.413, result, 0.01, "Did not correctly compute e^5.");
        }

        [TestMethod]
        public void Abs()
        {
            Expression expression = Infix.ParseOrThrow("abs(-10)");
            double result = Evaluate.Evaluate(null, expression).RealValue;
            Assert.AreEqual(10.00, result, 0.01, "Did not correctly compute absolute value.");
        }

        [TestMethod]
        public void d1()
        {
            Dictionary<string, FloatingPoint> variables = new Dictionary<string, FloatingPoint>();
            variables.Add("o", 0.1404);
            variables.Add("t", 4d/365d);
            variables.Add("St", 210.59);
            variables.Add("K", 205.0);
            variables.Add("r", 0.2175);

            Expression expression = Infix.ParseOrThrow("(1 / o / sqrt(t) * (ln(St / K) + (r + o^2 / 2) * t))");
            //Expression expression = Infix.ParseOrThrow("(ln(St / K) + (r + (o^2 / 2)) * t) / (o * sqrt(t))"); -- investopedia equals mine
            double result = Evaluate.Evaluate(variables, expression).RealValue;
            Assert.AreEqual(1.999947, result, 0.01, "Did not correctly compute d1.");
        }

        [TestMethod]
        public void d2()
        {
            Dictionary<string, FloatingPoint> variables = new Dictionary<string, FloatingPoint>();
            variables.Add("o", 0.1404);
            variables.Add("t", 4d / 365d);
            variables.Add("St", 210.59);
            variables.Add("K", 205.0);
            variables.Add("r", 0.2175);

            Expression expression = Infix.ParseOrThrow("((1 / o / sqrt(t) * (ln(St / K) + (r + o^2 / 2) * t)) - o * sqrt(t))");
            //Expression expression = Infix.ParseOrThrow("(ln(St / K) + (r + (o^2 / 2)) * t) / (o * sqrt(t)) - o * sqrt(t)"); -- investopedia equals mine
            double result = Evaluate.Evaluate(variables, expression).RealValue;
            Assert.AreEqual(1.985249, result, 0.01, "Did not correctly compute d2.");
        }

        [TestMethod]
        public void PV_K()
        {
            Dictionary<string, FloatingPoint> variables = new Dictionary<string, FloatingPoint>();
            variables.Add("o", 0.1404);
            variables.Add("t", 4d / 365d);
            variables.Add("St", 210.59);
            variables.Add("K", 205.0);
            variables.Add("r", 0.2175);

            Expression expression = Infix.ParseOrThrow("(K * exp(-r * t))");
            double result = Evaluate.Evaluate(variables, expression).RealValue;
            Assert.AreEqual(204.512, result, 0.01, "Did not correctly compute PV(K).");
        }

        // neither this nor erf exist in symbolics library
        //[TestMethod]
        //public void Erfc()
        //{
        //    Expression expression = Infix.ParseOrThrow("erfc(1)");
        //    double result = Evaluate.Evaluate(null, expression).RealValue;
        //    Assert.AreEqual(0.157299, result, 0.01, "Did not correctly compute gaussian complementary error function.");
        //}
    }
}
