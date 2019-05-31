using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probability;

namespace ProbabilityTests
{
    [TestClass]
    public class HelperFunctionTests
    {
        [TestMethod]
        public void Product()
        {
            Assert.AreEqual(4, HelperFunctions.Product(2, 2));
            Assert.AreEqual(18, HelperFunctions.Product(2, 3, 3));
            Assert.AreEqual(168, HelperFunctions.Product(7, 2, 3, 4));
        }

        [TestMethod]
        public void Game()
        {
            Game coinFlip = new Game(new Outcome(1, 0.5), new Outcome(0, 0.5));

            // expected probability of heads or tails
            Assert.AreEqual(0.5, coinFlip.ProbabilityOfOutcome(0.50));
            Assert.AreEqual(0.5, coinFlip.ProbabilityOfOutcome(500, 1000));

            // expected probability of deviating from mean when flipping coins
            Assert.AreEqual(0.02275, coinFlip.ProbabilityOfOutcome(60, 100), 0.01);
            Assert.AreEqual(0.1038, coinFlip.ProbabilityOfOutcome(520, 1000), 0.01);

            // rolling an average of 4+ on a series of dice rolls
            Game diceRoll = new Game(new Outcome(1, 0.16667), 
                                     new Outcome(2, 0.16667), 
                                     new Outcome(3, 0.16667), 
                                     new Outcome(4, 0.16667), 
                                     new Outcome(5, 0.16667), 
                                     new Outcome(6, 0.16667));

            Assert.AreEqual(0.00171, diceRoll.ProbabilityOfOutcome(400, 100), 0.01);
        }
    }
}
