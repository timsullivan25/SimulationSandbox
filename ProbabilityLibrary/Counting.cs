using System;
using System.Linq;

namespace Probability
{
    public static class Counting
    {
        /// <summary>
        /// Calculates the number of numbers between k and n.
        /// </summary>
        /// <param name="k">Lower-bound; must be >= 1.</param>
        /// <param name="n">Upper-bound; must be >= k.</param>
        /// <param name="inclusive">Include k and n when counting the number of numbers.</param>
        /// <returns></returns>
        public static long NumbersBetween(long k, long n, bool inclusive = true)
        {
            if (k == 1)
            {
                return inclusive? n : Math.Max(0,  n - 2);
            }
            else
            {
                return inclusive ? n - k + 1 : Math.Max(0, n - k - 1);
            }
        }

        /// <summary>
        /// Calculates the factorial of n (n!).
        /// </summary>
        /// <param name="n">The number for which to calculate the factorial.</param>
        /// <returns></returns>
        public static long Factorial(long n)
        {
            if (n >= 2) return n * Factorial(n - 1);
            return 1;
        }

        /// <summary>
        /// Calculates the number of sequences of k objects chosen from a collection of n objects.
        /// </summary>
        /// <param name="n">Size of collection.</param>
        /// <param name="k">Number of items chosen from collection.</param>
        /// <param name="repetition">Indicates whether the same object be chosen twice.</param>
        /// <returns></returns>
        public static long NumberOfSequences(long n, long k, bool repetition = true)
        {
            if (repetition)
            {
                return Convert.ToInt32(Math.Pow(n, k));
            }
            else
            {
                return Factorial(n) / Factorial(n - k);
            }
        }
        /// <summary>
        /// Calculates the number of sequences for a group of subsequences where the order the subsequences occurs in does not change. i.e. Only the order within each sequence can change.
        /// </summary>
        /// <param name="subsequences">One or more subsequences that will occur in a defined order.</param>
        /// <returns></returns>
        public static long NumberOfSequences(params (long n, long k, bool replacement)[] subsequences)
        {
            return HelperFunctions.Product(subsequences.Select(seq => NumberOfSequences(seq.n, seq.k, seq.replacement)).ToArray());
        }

        /// <summary>
        /// Calculates the number of collections of k objects chosen from a set of n objects. Collections do not make any distinction for the order in which the objects are chosen.
        /// </summary>
        /// <param name="n">Size of collection.</param>
        /// <param name="k">Number of items chosen from collection.</param>
        /// <param name="repetition">Indicates whether the same object be chosen twice.</param>
        /// <returns></returns>
        public static long NumberOfCollections(long n, long k, bool repetition = false)
        {
            if (repetition)
            {
                return NumberOfCollections(n + k - 1, n - 1, false);
            }
            else
            {
                return Factorial(n) / (Factorial(n - k) * Factorial(k));
            }
        }
        /// <summary>
        /// Calculates the total number of outcomes for a group of indepedent subcollections.
        /// </summary>
        /// <param name="subcollections">One or more subcollections that will occur indepedently of each other.</param>
        /// <returns></returns>
        public static long NumberOfCollections(params (long n, long k, bool repetition)[] subcollections)
        {
            return HelperFunctions.Product(subcollections.Select(col => NumberOfCollections(col.n, col.k, col.repetition)).ToArray());
        }

        /// <summary>
        /// Calculates the total number of ways a set of objects can be divided into collections of the specified sizes. This function assumes there is no repetition in the new collections and that every item in the original set is used exactly one time.
        /// </summary>
        /// <param name="n">Total number of items.</param>
        /// <param name="a">Sizes of each of the collections into which the original set will be split.</param>
        /// <returns></returns>
        public static long SplitCollection(long n, params long[] a)
        {
            if (a.Sum(a1 => a1) != n)
            {
                throw new Exception($"Number of items in the new collections must equal the original number of items. The original number of items is {n}, but the number of items in the new collections is {a.Sum(a1 => a1)}.");
            }

            return Factorial(n) / HelperFunctions.Product(a.Select(a1 => Factorial(a1)).ToArray());
        }
        
    }
}
