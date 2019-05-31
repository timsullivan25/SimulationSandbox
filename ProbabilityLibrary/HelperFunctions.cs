using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Probability
{
    public static class HelperFunctions
    {
        /// <summary>
        /// Returns the product of a set of numbers.
        /// </summary>
        /// <param name="numbers">Numbers for which to calculate the product.</param>
        /// <returns></returns>
        public static long Product(params long[] numbers)
        {
            long product = 1;

            foreach(long number in numbers)
            {
                product *= number;
            }

            return product;
        }
    }

    public enum BernoulliTrialOption
    {
        Exactly,
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo
    }

    public class Outcome
    {
        public double Payoff { get; set; }
        public double Probability { get; set; }

        public Outcome(double payoff, double probability)
        {
            Payoff = payoff;
            Probability = probability;
        }
    }

    public class Game
    {
        public List<Outcome> Outcomes { get; set; }

        public Game()
        {
            Outcomes = new List<Outcome>();
        }
        public Game(IEnumerable<Outcome> outcomes)
        {
            Outcomes = new List<Outcome>();

            foreach (Outcome outcome in outcomes)
            {
                Outcomes.Add(outcome);
            }
        }
        public Game(params Outcome[] outcomes)
        {
            Outcomes = new List<Outcome>();

            foreach (Outcome outcome in outcomes)
            {
                Outcomes.Add(outcome);
            }
        }

        /// <summary>
        /// Calculates the probability of achieving the desired outcome after n iterations of the game.
        /// </summary>
        /// <param name="outcome">Desired cumulative outcome from all games.</param>
        /// <param name="numberOfIterations">Number of games to play.</param>
        /// <returns></returns>
        public double ProbabilityOfOutcome(double outcome, long numberOfIterations = 1)
        {
            // TODO
            // expected value should work with game and/or set of outcomes
            // standard deviation should allow you to provide expected value for quicker computation
            // provide average win instead of total win? not sure it makes sense but accidentally did it in testing
            // add argument or variations for calculating between, less, greater, exact, etc...

            double expectedValue = numberOfIterations * Probability.ExpectedValue(Outcomes.Select(o => (o.Probability, o.Payoff)).ToArray());
            double standardDeviation = Math.Sqrt(
                                           numberOfIterations 
                                           * Probability.Variance(Outcomes.Select(o => (o.Probability, o.Payoff)).ToArray()));

            double zScore = Probability.ZScore(outcome, expectedValue, standardDeviation);
            return Probability.ZScoreToProbability(zScore, false);
        }
    }
}
