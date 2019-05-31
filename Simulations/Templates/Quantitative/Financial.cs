using System;
using MathNet.Numerics.Distributions;

namespace Simulations.Templates
{
    /// <summary>
    /// QuantitativeTemplates return a Simulation that was configured based on the passed arguments. The Simulation can be run immediate by calling the Simulate method or used as the Simulation argument in a simulation parameter. Essentially, these are shortcuts for creating common simulation scenarios.
    /// </summary>
    public static partial class QuantitativeTemplates
    {
        /// <summary>
        /// This simulation can be run on its own or used as the Simulation argument in a SimulationParameter.
        /// </summary>
        /// <returns></returns>
        public static Simulation CAPM(IParameter riskFreeRate, IParameter beta, IParameter marketReturn)
        {
            // generate expression
            string expression = $"{riskFreeRate.Name} + ({beta.Name} * ({marketReturn.Name} - {riskFreeRate.Name}))";

            // return simulation
            return new Simulation(expression, riskFreeRate, beta, marketReturn);
        }

        /// <summary>
        /// Calculates the value of a European call or put option using the Black-Scholes model.
        /// </summary>
        /// <param name="optionContractType">Call or put option.</param>
        /// <param name="priceOfUnderlyingAsset">Price of underlying asset at time t.</param>
        /// <param name="strikePrice">Strike price of option.</param>
        /// <param name="timeToMaturity">Time to maturity in years.</param>
        /// <param name="volatilityOfReturns">Standard deviation of stock's returns. Square root of the quadratic variation of the stock's log price process.</param>
        /// <param name="riskFreeRate">Annualized risk-free interest rate, continuously compounded.</param>
        /// <returns></returns>
        public static double BlackScholes(OptionContractType optionContractType, 
                                          double priceOfUnderlyingAsset,
                                          double strikePrice,
                                          double timeToMaturity,
                                          double volatilityOfReturns,
                                          double riskFreeRate)
        {            
            // S(t)   = the price of the underlying asset at time t
            // V(S,t) = the price of the option as a function of the underlying asset, S at time t
            // C(S,t) = the price of a European call option and P(S,t) the price of a European put option
            // K      = the strike price of the option, also known as the exercise price
            // r      = the annualized risk-free interest rate, continuously compounded
            // u      = the drift rate of S, annualized
            // o      = standard deviation of the stock's returns; the square root of the quadratic variation of the stock's log price process
            // t      = a time in years; we generally use: now 0, expiry T
            // II     = the value of the portfolio
            // N      = cumulative distribution function of the standard normal distribution

            if (optionContractType == OptionContractType.Call)
            {
                // C(S,t) = N * d1 * S(t) - N * d2 * PV(K)
                // d1 = (1 / (o * sqrt(T - t))) * (ln(S(t) / K) + (r + o^2 / 2) * (T - t))
                // d2 = d1 - o * sqrt(T - t)
                // PV(K) = K * e^(-r * (T - t))

                Normal normal = new Normal();

                double d1 = 1 / volatilityOfReturns / Math.Sqrt(timeToMaturity)
                            * (Math.Log(priceOfUnderlyingAsset / strikePrice)
                               + (riskFreeRate + Math.Pow(volatilityOfReturns, 2) / 2) * timeToMaturity);

                double d2 = d1 - volatilityOfReturns * Math.Sqrt(timeToMaturity);
                double PV_K = strikePrice * Math.Pow(Math.E, -riskFreeRate * timeToMaturity);
               
                return normal.CumulativeDistribution(d1) * priceOfUnderlyingAsset
                       - normal.CumulativeDistribution(d2) * PV_K;
            }
            else
            {
                // P(S,t) = K * e^(-r * (T - t)) - S(t) + C(S,t)
                // P(S,t) = N * -d2 * K * e ^ (-r * (T - t)) - N * -d1 * S(t)

                double callOptionPrice = BlackScholes(OptionContractType.Call,
                                                      priceOfUnderlyingAsset,
                                                      strikePrice,
                                                      timeToMaturity,
                                                      volatilityOfReturns,
                                                      riskFreeRate);

                double PV_K = strikePrice * Math.Pow(Math.E, -riskFreeRate * timeToMaturity);
                return PV_K - priceOfUnderlyingAsset + callOptionPrice;
            }
        }

        /// <summary>
        /// Creates a simulation for calculating the value of a European call or put option using the Black-Scholes model. This simulation allows any of the parameters to be flexed for analysis purposes. Use constant parameters for any known numbers. Use precomputed parameters to perform sensitivity analysis.
        /// </summary>
        /// <param name="optionContractType">Call or put option.</param>
        /// <param name="priceOfUnderlyingAsset">Price of underlying asset at time t.</param>
        /// <param name="strikePrice">Strike price of option.</param>
        /// <param name="timeToMaturity">Time to maturity in years.</param>
        /// <param name="volatilityOfReturns">Standard deviation of stock's returns. Square root of the quadratic variation of the stock's log price process.</param>
        /// <param name="riskFreeRate">Annualized risk-free interest rate, continuously compounded.</param>
        /// <returns></returns>
        public static Simulation BlackScholes(OptionContractType optionContractType,
                                              IParameter priceOfUnderlyingAsset,
                                              IParameter strikePrice,
                                              IParameter timeToMaturity,
                                              IParameter volatilityOfReturns,
                                              IParameter riskFreeRate)
        {
            // NOTE: The subsequent string manipulation is my attempt at a slightly dirty hack to compute the CDF of d1 and d2 inside the simulation's expression. Originally, I had planned to use the DistributionFunctionParameter, but realized that the issue with this is that breaking the simulation into seperate equations would cause values used by multiple equations to be resimulated for each equation... Essentially, they would not be consistent between different equations in the same simulation, so the answer would have been useless. This workaround seems to generate option prices within ~0.05 of the above function, which I consider to be a success based on the sheer number of calculations being performed which leaves ample room for rounding errors. I have checked the formulas pretty closely, but it is possible there is still a typo lurking some where as well.
      

            // rename parameters to work with shortened version of equation
            priceOfUnderlyingAsset.Name = "St";
            strikePrice.Name = "K";
            timeToMaturity.Name = "t";
            volatilityOfReturns.Name = "o";
            riskFreeRate.Name = "r";

            // create the pieces needed to form either expression
            string d1 = "(1 / o / sqrt(t) * (ln(St / K) + (r + o^2 / 2) * t))";
            string d2 = "((1 / o / sqrt(t) * (ln(St / K) + (r + o^2 / 2) * t)) - o * sqrt(t))";
            string PV_K = "(K * exp(-r * t))";

            string a1 = "0.254829592";
            string a2 = "-0.284496736";
            string a3 = "1.421413741";
            string a4 = "-1.453152027";
            string a5 = "1.061405429";
            string p  = "0.3275911";

            string x1 = $"(abs({d1}) / sqrt(2))";
            string x2 = $"(abs({d2}) / sqrt(2))";

            string t1 = $"(1 / (1 + {p} * {x1}))";
            string t2 = $"(1 / (1 + {p} * {x2}))";

            string sign = optionContractType == OptionContractType.Call ? "+" : "-";
            string N1 = $"(0.5 * (1 {sign} (1 - ((((({a5}*{t1} + {a4})*{t1}) + {a3})*{t1} + {a2})*{t1} + {a1})*{t1}*exp(-{x1}*{x1}))))";
            string N2 = $"(0.5 * (1 {sign} (1 - ((((({a5}*{t2} + {a4})*{t2}) + {a3})*{t2} + {a2})*{t2} + {a1})*{t2}*exp(-{x2}*{x2}))))";

            // create simulation based on type of option
            if (optionContractType == OptionContractType.Call)
            {
                string expression = $"{N1} * St - {N2} * {PV_K}";

                return new Simulation(expression,
                                      priceOfUnderlyingAsset,
                                      strikePrice,
                                      timeToMaturity,
                                      volatilityOfReturns,
                                      riskFreeRate);
            }
            else
            {
                string expression = $"{N2} * {PV_K} - {N1} * St";

                return new Simulation(expression,
                                      priceOfUnderlyingAsset,
                                      strikePrice,
                                      timeToMaturity,
                                      volatilityOfReturns,
                                      riskFreeRate);
            }
        }
    }
}
