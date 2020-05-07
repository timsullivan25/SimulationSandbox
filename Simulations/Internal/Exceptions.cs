using System;

namespace Simulations
{
    namespace Exceptions
    {
        public class EmptyBagException : Exception
        {
            public EmptyBagException() { }

            public EmptyBagException(string message)
                : base(message)
            {
            }

            public EmptyBagException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class DistributionFunctionFailureException : Exception
        {
            public DistributionFunctionFailureException() { }

            public DistributionFunctionFailureException(string message)
                : base(message)
            {
            }

            public DistributionFunctionFailureException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidConfidenceIntervalException : Exception
        {
            public InvalidConfidenceIntervalException() { }

            public InvalidConfidenceIntervalException(string message)
                : base(message)
            {
            }

            public InvalidConfidenceIntervalException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidConstraintException : Exception
        {
            public InvalidConstraintException() { }

            public InvalidConstraintException(string message)
                : base(message)
            {
            }

            public InvalidConstraintException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidInterpolationTypeException : Exception
        {
            public InvalidInterpolationTypeException() { }

            public InvalidInterpolationTypeException(string message)
                : base(message)
            {
            }

            public InvalidInterpolationTypeException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidMoveException : Exception
        {
            public InvalidMoveException() { }

            public InvalidMoveException(string message)
                : base(message)
            {
            }

            public InvalidMoveException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidOptionTypeException : Exception
        {
            public InvalidOptionTypeException() { }

            public InvalidOptionTypeException(string message)
                : base(message)
            {
            }

            public InvalidOptionTypeException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

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

        public class InvalidResolutionException : Exception
        {
            public InvalidResolutionException() { }

            public InvalidResolutionException(string message)
                : base(message)
            {
            }

            public InvalidResolutionException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidReturnTypeException : Exception
        {
            public InvalidReturnTypeException() { }

            public InvalidReturnTypeException(string message)
                : base(message)
            {
            }

            public InvalidReturnTypeException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class InvalidSummaryStatisticException : Exception
        {
            public InvalidSummaryStatisticException() { }

            public InvalidSummaryStatisticException(string message)
                : base(message)
            {
            }

            public InvalidSummaryStatisticException(string message, Exception inner)
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

        public class NonExistentOutcomeException : Exception
        {
            public NonExistentOutcomeException() { }

            public NonExistentOutcomeException(string message)
                : base(message)
            {
            }

            public NonExistentOutcomeException(string message, Exception inner)
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

        public class RandomBagItemCountException : Exception
        {
            public RandomBagItemCountException() { }

            public RandomBagItemCountException(string message)
                : base(message)
            {
            }

            public RandomBagItemCountException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class RandomBagReplacementRuleException : Exception
        {
            public RandomBagReplacementRuleException() { }

            public RandomBagReplacementRuleException(string message)
                : base(message)
            {
            }

            public RandomBagReplacementRuleException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        public class TooManyParentsException : Exception
        {
            public TooManyParentsException() { }

            public TooManyParentsException(string message)
                : base(message)
            {
            }

            public TooManyParentsException(string message, Exception inner)
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
    }
}