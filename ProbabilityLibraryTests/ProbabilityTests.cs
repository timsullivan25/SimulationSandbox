using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probability;

namespace ProbabilityTests
{
    [TestClass]
    public class ProbabilityTests
    {
        [TestMethod]
        public void ExpectedValue()
        {
            // probabilities
            Assert.AreEqual(0.5, Probability.Probability.ExpectedValue((0.5, 1.0)), 0.01);
            Assert.AreEqual(1.0, Probability.Probability.ExpectedValue((0.333, 2.0), (0.333, 1.0), (0.333, 0.0)), 0.01);

            // number of outcomes
            Assert.AreEqual(0.5, Probability.Probability.ExpectedValue((100L, 1.0), (100L, 0.0)), 0.01);
            Assert.AreEqual(1.0, Probability.Probability.ExpectedValue((300L, 2.0), (300L, 1.0), (300L, 0.0)), 0.01);
        }

        [TestMethod]
        public void WinningPercentage()
        {
            Assert.AreEqual(0.25, Probability.Probability.WinningPercentage((0.5, 0.5)), 0.01);
            Assert.AreEqual(0.5, Probability.Probability.WinningPercentage((0.75, 0.333), (0.25, 1.00)), 0.01);
            Assert.AreEqual(0.41667, Probability.Probability.WinningPercentage((0.6667, 0.50), (0.3333, 0.25)), 0.01);
        }

        [TestMethod]
        public void BayesTheorem()
        {
            Assert.AreEqual(0.096, Probability.Probability.BayesTheorem(0.15625, 0.375, 0.04, null), 0.001);
            Assert.AreEqual(0.04, Probability.Probability.BayesTheorem(0.15625, 0.375, null, 0.096), 0.001);
            Assert.AreEqual(0.375, Probability.Probability.BayesTheorem(0.15625, null, 0.04, 0.096), 0.001);
            Assert.AreEqual(0.15625, Probability.Probability.BayesTheorem(null, 0.375, 0.04, 0.096), 0.001);

            try
            {
                Assert.AreEqual(0.096, Probability.Probability.BayesTheorem(0.15625, 0.375, 0.04, 0.096), 0.001);
                Assert.Fail();
            }
            catch
            {
                // should throw an exception when there's nothing to solve for
            }

            try
            {
                Assert.AreEqual(0.096, Probability.Probability.BayesTheorem(0.15625, 0.375, null, null), 0.001);
                Assert.Fail();
            }
            catch
            {
                // should throw an exception when there's too many variables to solve for
            }
        }

        [TestMethod]
        public void BernoulliTrials()
        {
            Assert.AreEqual(0.269, Probability.Probability.BernoulliTrials(12, 1, 0.1667), 0.01);
            Assert.AreEqual(0.619, Probability.Probability.BernoulliTrials(12, 2, 0.1667, BernoulliTrialOption.GreaterThanOrEqualTo), 0.01);
            Assert.AreEqual(0.3229, Probability.Probability.BernoulliTrials(12, 2, 0.1667, BernoulliTrialOption.GreaterThan), 0.01);
            Assert.AreEqual(0.619, Probability.Probability.BernoulliTrials(12, 10, 0.8333, BernoulliTrialOption.LessThanOrEqualTo), 0.01);
            Assert.AreEqual(0.3229, Probability.Probability.BernoulliTrials(12, 10, 0.8333, BernoulliTrialOption.LessThan), 0.01);
        }

        [TestMethod]
        public void RandomWalk()
        {
            Assert.AreEqual(0.30993, Probability.Probability.RandomWalk(10, 20, 0.48), 0.01);
        }

        [TestMethod]
        public void Variance()
        {
            // probabilities
            Assert.AreEqual(4.11, Probability.Probability.Variance((0.5787, 0), (0.3472, 2), (0.0694, 2), (0.0046, 27)), 0.01);

            // number of outcomes -- it appears this is slightly off due to converting longs to probabilities
            Assert.AreEqual(4.11, Probability.Probability.Variance((125L, 0.0), (75L, 2.0), (15L, 2), (1L, 27)), 0.02);
        }

        [TestMethod]
        public void StandardDeviation()
        {
            // probabilities
            Assert.AreEqual(2.03, Probability.Probability.StandardDeviation((0.5787, 0), (0.3472, 2), (0.0694, 2), (0.0046, 27)), 0.01);

            // number of outcomes
            Assert.AreEqual(2.03, Probability.Probability.StandardDeviation((125L, 0.0), (75L, 2.0), (15L, 2), (1L, 27)), 0.01);
        }

        [TestMethod]
        public void ZScore()
        {
            Assert.AreEqual(0.0, Probability.Probability.ZScore(5.0, 5.0, 1), 0.01);
            Assert.AreEqual(0.4, Probability.Probability.ZScore(0.60, 0.50, 0.25), 0.01);
        }

        [TestMethod]
        public void ZScoreToProbability()
        {
            Assert.AreEqual(0.9357, Probability.Probability.ZScoreToProbability(1.52), 0.01);
            Assert.AreEqual(0.4013, Probability.Probability.ZScoreToProbability(0.25, false), 0.01);
        }

        [TestMethod]
        public void BoundedZScoreProbability()
        {
            Assert.AreEqual(0.2349, Probability.Probability.BoundedZScoreProbability(0.7, 2.45), 0.01);
            Assert.AreEqual(0.3174, Probability.Probability.BoundedZScoreProbability(-1.00, 1.00, false), 0.01);
        }

        [TestMethod]
        public void ProbabilityToZScore()
        {
            Assert.AreEqual(1.66, Probability.Probability.ProbabilityToZScore(0.9515), 0.01);
            Assert.AreEqual(0.43, Probability.Probability.ProbabilityToZScore(0.6664), 0.01);
        }
    }
}
