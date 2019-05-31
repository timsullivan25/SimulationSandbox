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
}
