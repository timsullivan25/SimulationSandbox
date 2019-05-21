using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Simulations.Exceptions;

namespace Simulations
{
    public static class Extensions
    {
        // these two aren't used... yet.. but would be useful to have later
        public static IEnumerable<IEnumerable<T>> PowerSets<T>(this IList<T> set)
        {
            var totalSets = BigInteger.Pow(2, set.Count);
            for (BigInteger i = 0; i < totalSets; i++)
            {
                yield return set.SubSet(i);
            }
        }

        public static void Shuffle<T>(this T[] array)
        {
            Random random = new Random();

            for (int i = array.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(0, i + 1);
                T temp = array[i];
                array[i] = array[swapIndex];
                array[swapIndex] = temp;
            }
        }

        public static IEnumerable<T> SubSet<T>(this IList<T> set, BigInteger n)
        {
            for (int i = 0; i < set.Count && n > 0; i++)
            {
                if ((n & 1) == 1)
                {
                    yield return set[i];
                }

                n = n >> 1;
            }
        }

        // this makes the magic happen for sensitivity analysis
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));
        }

        public static double ZScore(this ConfidenceLevel confidenceLevel)
        {
            switch (confidenceLevel)
            {
                case ConfidenceLevel._80:
                    return 1.282;
                case ConfidenceLevel._85:
                    return 1.440;
                case ConfidenceLevel._90:
                    return 1.645;
                case ConfidenceLevel._95:
                    return 1.960;
                case ConfidenceLevel._99:
                    return 2.576;
                case ConfidenceLevel._99_5:
                    return 2.807;
                case ConfidenceLevel._99_9:
                    return 3.291;
                default:
                    throw new InvalidConfidenceIntervalException($"{confidenceLevel} is not a valid confidence level.");
            }
        }
    }
}
