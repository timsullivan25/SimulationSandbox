using Microsoft.VisualStudio.TestTools.UnitTesting;
using Probability;

namespace ProbabilityTests
{
    [TestClass]
    public class CountingTests
    {
        [TestMethod]
        public void NumbersBetween()
        {
            // inclusive (normal cases)
            Assert.AreEqual(283, Counting.NumbersBetween(1, 283));           
            Assert.AreEqual(261, Counting.NumbersBetween(23, 283));

            // non-inclusive option
            Assert.AreEqual(281, Counting.NumbersBetween(1, 283, false));
            Assert.AreEqual(259, Counting.NumbersBetween(23, 283, false));

            // edge cases
            Assert.AreEqual(1, Counting.NumbersBetween(1, 1));
            Assert.AreEqual(0, Counting.NumbersBetween(1, 2, false));
        }

        [TestMethod]
        public void Factorial()
        {
            Assert.AreEqual(120, Counting.Factorial(5));
            Assert.AreEqual(39916800, Counting.Factorial(11));
        }

        [TestMethod]
        public void NumberOfSequences()
        {
            // repetition
            Assert.AreEqual(11881376, Counting.NumberOfSequences(26, 5));
            Assert.AreEqual(64, Counting.NumberOfSequences(2, 6));

            // no repetition
            Assert.AreEqual(32760, Counting.NumberOfSequences(15, 4, false));
        }
        [TestMethod]
        public void NumberOfSequences_Subsequences()
        {
            // no repetition
            Assert.AreEqual(203212800, Counting.NumberOfSequences((8, 8, false), (7, 7, false)));

            // repetition
            Assert.AreEqual(2096640, Counting.NumberOfSequences((2, 6, true), (15, 4, false)));
        }

        [TestMethod]
        public void NumberOfCollections()
        {
            // no repetition
            Assert.AreEqual(3003, Counting.NumberOfCollections(15, 5));

            // repetition
            Assert.AreEqual(792, Counting.NumberOfCollections(6, 7, true));
        }
        [TestMethod]
        public void NumberOfCollections_Subcollections()
        {
            // no repetition
            Assert.AreEqual(210, Counting.NumberOfCollections((5, 2, false), (7, 2, false)));

            // repetition
            Assert.AreEqual(2378376, Counting.NumberOfCollections((15, 5, false), (6, 7, true)));
        }

        [TestMethod]
        public void SplitCollection()
        {
            Assert.AreEqual(1260, Counting.SplitCollection(9, 4, 3, 2));

            try
            {
                Counting.SplitCollection(9, 4, 3, 3);
                Assert.Fail("Error not thrown correctly.");
            }
            catch 
            {
                // the above should have thrown an exception
            }
        }
    }
}
