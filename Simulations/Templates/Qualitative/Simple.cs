using System.Collections.Generic;
using System.Linq;

namespace Simulations.Templates
{
    /// <summary>
    /// QualitativeTemplates return the results of the QualitativeSimulation that is created and run using the passed arguments. This happens because QualitativeSimulations only accept one parameter, so there is no reason to return the parameter instead of executing the simulation immediately.
    /// </summary>
    public static partial class QualitativeTemplates
    {
        /// <summary>
        /// Flips a coin the specified number of times.
        /// </summary>
        /// <param name="numberOfFlips">Number of times to flip the coin.</param>
        /// <returns></returns>
        public static QualitativeSimulationResults CoinFlip(int numberOfFlips)
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("coinflip", RandomBagReplacement.AfterEachPick);
            bag.Add("Heads");
            bag.Add("Tails");
            return new QualitativeSimulation(bag).Simulate(numberOfFlips);
        }

        /// <summary>
        /// Rolls an n-sided die the specified number of times.
        /// </summary>
        /// <param name="numberOfSides">Number of sides on the die.</param>
        /// <param name="numberOfRolls">Number of times to roll the die.</param>
        /// <returns></returns>
        public static QualitativeSimulationResults DiceRoll(int numberOfSides, int numberOfRolls)
        {
            QualitativeRandomBagParameter bag = new QualitativeRandomBagParameter("diceroll", RandomBagReplacement.AfterEachPick);

            for (int i = 1; i <= numberOfSides; i++)
                bag.Add(i.ToString());

            return new QualitativeSimulation(bag).Simulate(numberOfRolls);
        }

        /// <summary>
        /// Draws n number of cards for a standard 52-card deck without replacing the cards between picks.
        /// </summary>
        /// <param name="numberOfCards">Number of cards to draw from the deck.</param>
        /// <returns></returns>
        public static QualitativeSimulationResults DeckOfCards(int numberOfCards)
        {
            QualitativeRandomBagParameter deck = new QualitativeRandomBagParameter("deck", RandomBagReplacement.Never);

            string[] suits = new string[] { "C", "D", "H", "S" };
            string[] numbers = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            string[] cards = new string[suits.Length * numbers.Length];

            for (int s = 0; s < suits.Length; s++)
            {
                for (int n = 0; n < numbers.Length; n++)
                {
                    deck.Add(numbers[n] + suits[s]);
                }
            }

            return new QualitativeSimulation(deck).Simulate(numberOfCards);
        }

        /// <summary>
        /// A helpful method for choosing which restaurant to eat at when no one can agree on a choice.
        /// </summary>
        /// <param name="restaurants">One or more restuarants from which to choose the place you will eat.</param>
        /// <returns></returns>
        public static string RestaurantPicker(params string[] restaurants)
        {
            if (restaurants == null || restaurants.Length == 0)
            {
                throw new Exceptions.EmptyBagException("You must provide one or more restaurants to choose from.");
            }

            QualitativeRandomBagParameter choices = new QualitativeRandomBagParameter("restaurants", RandomBagReplacement.Never);

            foreach (string r in restaurants)
            {
                choices.Add(r);
            }

            QualitativeSimulationResults results = new QualitativeSimulation(choices).Simulate(1);
            return results.Results[0];
        }

        /// <summary>
        /// Shuffles a list of items and then draws all of them from the bag to generate an ordered list. This is essentially a wrapper for the Shuffle extension function, but fits well thematically.
        /// </summary>
        /// <param name="items">Items to be shuffled.</param>
        /// <returns></returns>
        public static List<string> ShuffleOrder(params string[] items)
        {
            if (items == null || items.Length == 0)
            {
                throw new Exceptions.EmptyBagException("You must provide one or more items to shuffle.");
            }

            QualitativeRandomBagParameter choices = new QualitativeRandomBagParameter("items", RandomBagReplacement.Never);

            foreach (string i in items)
            {
                choices.Add(i);
            }

            QualitativeSimulationResults results = new QualitativeSimulation(choices).Simulate(items.Length);
            return results.Results.ToList();
        }
    }
}
