using System;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace Probability
{
    public static class Probability
    {
        /// <summary>
        /// Provide three known values and one null value to calculate using Bayres' Theorum: P(A | B) * P(B) = P(B | A) * P(A)
        /// </summary>
        /// <param name="pA">Probability of A.</param>
        /// <param name="pB">Probability of B.</param>
        /// <param name="pAgB">Probability of A assuming B.</param>
        /// <param name="pBgA">Probability of B assuming A.</param>
        /// <returns></returns>
        public static double BayesTheorem(double? pA, double? pB, double? pAgB, double? pBgA)
        {
            if (pA == null && pB != null && pAgB != null && pBgA != null)
            {
                return (double)(pAgB * pB / pBgA);
            }
            else if (pB == null && pA != null && pAgB != null && pBgA != null)
            {
                return (double)(pBgA * pA / pAgB);
            }
            else if (pAgB == null && pA != null && pB != null && pBgA != null)
            {
                return (double)(pBgA * pA / pB);
            }
            else if (pBgA == null && pA != null && pB != null && pAgB != null)
            {
                return (double)(pAgB * pB / pA);
            }
            else
            {
                throw new Exception("You must provide exactly one null value and three known values.");
            }
        }
        /// <summary>
        /// Calculates the probability of exactly k events occurring in n trials when each trial has exactly two outcomes.
        /// </summary>
        /// <param name="n">Number of trials to be performed.</param>
        /// <param name="k">Number of occurrences of desired outcome.</param>
        /// <param name="p">The probabiity of the desired outcome occurring in any given trial.</param>
        /// <param name="bernoulliTrialOption">Option to specify which probability to calculate.</param>  
        /// <returns></returns>
        public static double BernoulliTrials(long n, long k, double p, BernoulliTrialOption bernoulliTrialOption = BernoulliTrialOption.Exactly)
        {
            double cumulativeProbability = 0;

            switch (bernoulliTrialOption)
            {
                case BernoulliTrialOption.Exactly:
                    return Counting.NumberOfCollections(n, k) * Math.Pow(p, k) * Math.Pow(1 - p, n - k);

                case BernoulliTrialOption.GreaterThan:
                    for (long i = k; i >= 0; i--)
                        cumulativeProbability += BernoulliTrials(n, i, p);

                    return 1 - cumulativeProbability;

                case BernoulliTrialOption.GreaterThanOrEqualTo:
                    for (long i = k - 1; i >= 0; i--)
                        cumulativeProbability += BernoulliTrials(n, i, p);

                    return 1 - cumulativeProbability;

                case BernoulliTrialOption.LessThan:
                    for (long i = k; i <= n; i++)
                        cumulativeProbability += BernoulliTrials(n, i, p);

                    return 1 - cumulativeProbability;

                case BernoulliTrialOption.LessThanOrEqualTo:
                    for (long i = k + 1; i <= n; i++)
                        cumulativeProbability += BernoulliTrials(n, i, p);

                    return 1 - cumulativeProbability;

                default:
                    throw new Exception("Invalid BernoulliTrialOption.");
            }  
        }
        /// <summary>
        /// Calculates the probability of reaching the ending value on a random walk assuming the change in value is 1 each time.
        /// </summary>
        /// <param name="startingValue">Beginning value that will randomly change by 1 each period.</param>
        /// <param name="endingValue">Ending value at which you will cease playing and consider the outcome a success.</param>
        /// <param name="probabilityOfWinning">Probability of winning 1 unit on any given turn.</param>
        /// <returns></returns>
        public static double RandomWalk(double startingValue, double endingValue, double probabilityOfWinning)
        {
            double s = (1 - probabilityOfWinning) / probabilityOfWinning;
            return (Math.Pow(s, startingValue) - 1) / (Math.Pow(s, endingValue) - 1);
        }

        /// <summary>
        /// Calculates the expected value of a set of outcomes based on the probability and payoff of each outcome. You do not need to specify outcomes that have a payoff of 0.
        /// </summary>
        /// <param name="outcomes">Probability and corresponding payoff for each outcome.</param>
        /// <returns></returns>
        public static double ExpectedValue(params (double probability, double payoff)[] outcomes)
        {
            return outcomes.Sum(o => o.probability * o.payoff);
        }
        /// <summary>
        /// Calculates the expected value of a set of outcomes based on the total number of occurrences and payoff of each outcome. You must specify outcomes that have a payout of 0 for the remainder of the probabilities to be calculated correctly.
        /// </summary>
        /// <param name="outcomes">Number of occurrences and corresponding payoff for each outcome.</param>
        /// <returns></returns>
        public static double ExpectedValue(params (long occurrences, double payoff)[] outcomes)
        {
            double totalOccurrences = outcomes.Sum(o => o.occurrences);
            return ExpectedValue(outcomes.Select(o => (o.occurrences / totalOccurrences, o.payoff)).ToArray());
        }
        /// <summary>
        /// Calculates the changes of winning based on the possible scenarios and their specific win rates.
        /// </summary>
        /// <param name="winScenarios">A list containing all scenarios with their probability of occurring and their win percentage.</param>
        /// <returns></returns>
        public static double WinningPercentage(params (double probabilityOfScenario, double probabilityOfWinning)[] winScenarios)
        {
            return winScenarios.Sum(s => s.probabilityOfScenario * s.probabilityOfWinning);
        }

        /// <summary>
        /// Calculates the variance of a set of outcomes based on the probability and payoff of each outcome. You must specify outcomes that have a payoff of 0.
        /// </summary>
        /// <param name="outcomes">Probability and corresponding payoff for each outcome.</param>
        /// <returns></returns>
        public static double Variance(params (double probability, double payoff)[] outcomes)
        {
            double expectedValue = ExpectedValue(outcomes);
            return outcomes.Sum(o => o.probability * Math.Pow(o.payoff - expectedValue, 2));
        }
        /// <summary>
        /// Calculates the variance of a set of outcomes based on the total number of occurrences and payoff of each outcome. You must specify outcomes that have a payout of 0 for the remainder of the probabilities to be calculated correctly.
        /// </summary>
        /// <param name="outcomes">Number of occurrences and corresponding payoff for each outcome.</param>
        /// <returns></returns>
        public static double Variance(params (long occurrences, double payoff)[] outcomes)
        {
            double totalOccurrences = outcomes.Sum(o => o.occurrences);
            return Variance(outcomes.Select(o => (o.occurrences / totalOccurrences, o.payoff)).ToArray());
        }
        /// <summary>
        /// Calculates the standard deviation of a set of outcomes based on the probability and payoff of each outcome. You must specify outcomes that have a payoff of 0.
        /// </summary>
        /// <param name="outcomes">Probability and corresponding payoff for each outcome.</param>
        /// <returns></returns>
        public static double StandardDeviation(params (double probability, double payoff)[] outcomes)
        {
            double expectedValue = ExpectedValue(outcomes);
            return Math.Sqrt(outcomes.Sum(o => o.probability * Math.Pow(o.payoff - expectedValue, 2)));
        }
        /// <summary>
        /// Calculates the standard deviation of a set of outcomes based on the total number of occurrences and payoff of each outcome. You must specify outcomes that have a payout of 0 for the remainder of the probabilities to be calculated correctly.
        /// </summary>
        /// <param name="outcomes">Number of occurrences and corresponding payoff for each outcome.</param>
        /// <returns></returns>
        public static double StandardDeviation(params (long occurrences, double payoff)[] outcomes)
        {
            double totalOccurrences = outcomes.Sum(o => o.occurrences);
            return StandardDeviation(outcomes.Select(o => (o.occurrences / totalOccurrences, o.payoff)).ToArray());
        }
        
        /// <summary>
        /// Calculates the z-score for the specified outcome.
        /// </summary>
        /// <param name="targetValue">Desired outcome.</param>
        /// <param name="expectedValue">Expected value of all outcomes.</param>
        /// <param name="standardDeviation">Standard deviation of outcomes.</param>
        /// <returns></returns>
        public static double ZScore(double targetValue, double expectedValue, double standardDeviation)
        {
            return (targetValue - expectedValue) / standardDeviation;
        }
        /// <summary>
        /// Calculates the area under the curve (probability) to the left or right of the z-score.
        /// </summary>
        /// <param name="zScore">Z-score for which to find the probability.</param>
        /// <param name="lessThan">True to calculate probability of a number less than the z-score. False for to calculate probability of a number greater than the z-score.</param>
        /// <returns></returns>
        public static double ZScoreToProbability(double zScore, bool lessThan = true)
        {
            Normal normal = new Normal();
            double probability = normal.CumulativeDistribution(zScore);
            return lessThan ? probability : 1 - probability;
        }
        /// <summary>
        /// Calculates the probability of an event occurring inside or outside the range between two z-scores.
        /// </summary>
        /// <param name="lowerBound">The lower bound of the z-score range.</param>
        /// <param name="upperBound">The upper bound of the z-score rage.</param>
        /// <param name="between">True to calculate area between the z-scores. False to calculate area outside of the z-score range.</param>
        /// <returns></returns>
        public static double BoundedZScoreProbability(double lowerBound, double upperBound, bool between = true)
        {
            double boundedProbability = ZScoreToProbability(upperBound) - ZScoreToProbability(lowerBound);
            return between ? boundedProbability : 1 - boundedProbability;
        }
        /// <summary>
        /// Converts an outcome into its equivalent z-score. Essentially, how many standard deviations the outcome is from the mean.
        /// </summary>
        /// <param name="probability">Probability to convert to z-score.</param>
        /// <returns></returns>
        public static double ProbabilityToZScore(double probability)
        {
            Normal normal = new Normal();
            return normal.InverseCumulativeDistribution(probability);            
        }
    }
}
