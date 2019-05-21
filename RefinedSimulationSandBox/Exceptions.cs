using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RefinedSimulationSandBox
{
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException() { }

        public InvalidParameterException(string message)
            : base(message)
        {
        }

        public InvalidParameterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class InvalidProbabilityException : Exception
    {
        public InvalidProbabilityException() { }

        public InvalidProbabilityException(string message)
            : base(message)
        {
        }

        public InvalidProbabilityException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class MissingPrecomputedParameterException : Exception
    {
        public MissingPrecomputedParameterException() { }

        public MissingPrecomputedParameterException(string message)
            : base(message)
        {
        }

        public MissingPrecomputedParameterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class ParameterInExpressionException : Exception
    {
        public ParameterInExpressionException() { }

        public ParameterInExpressionException(string message)
            : base(message)
        {
        }

        public ParameterInExpressionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class PrecomputedValueCountException : Exception
    {
        public PrecomputedValueCountException() { }

        public PrecomputedValueCountException(string message)
            : base(message)
        {
        }

        public PrecomputedValueCountException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class UncomputedSimulationException : Exception
    {
        public UncomputedSimulationException() { }

        public UncomputedSimulationException(string message)
            : base(message)
        {
        }

        public UncomputedSimulationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    enum ComparisonOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,        
    }

    public static class Extensions
    {
        // this two aren't used... yet.. but would be useful to have later
        public static IEnumerable<IEnumerable<T>> PowerSets<T>(this IList<T> set)
        {
            var totalSets = BigInteger.Pow(2, set.Count);
            for (BigInteger i = 0; i < totalSets; i++)
            {
                yield return set.SubSet(i);
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
    }
}
