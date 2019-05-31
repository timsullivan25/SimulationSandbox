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
        /// <param name="standardDeviationOfStocksReturns">Standard deviation of stock's returns. Square root of the quadratic variation of the stock's log price process.</param>
        /// <param name="riskFreeRate">Annualized risk-free interest rate, continuously compounded.</param>
        /// <returns></returns>
        public static double BlackScholes(OptionContractType optionContractType, 
                                          double priceOfUnderlyingAsset,
                                          double strikePrice,
                                          double timeToMaturity,
                                          double standardDeviationOfStocksReturns,
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

                double d1 = (1 / (standardDeviationOfStocksReturns * Math.Sqrt(timeToMaturity)))
                            * (Math.Log(priceOfUnderlyingAsset / strikePrice)
                               + (riskFreeRate + ((Math.Pow(standardDeviationOfStocksReturns, 2) / 2) * timeToMaturity)));

                double d2 = d1 - (standardDeviationOfStocksReturns * Math.Sqrt(timeToMaturity));
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
                                                      standardDeviationOfStocksReturns,
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
        /// <param name="standardDeviationOfStocksReturns">Standard deviation of stock's returns. Square root of the quadratic variation of the stock's log price process.</param>
        /// <param name="riskFreeRate">Annualized risk-free interest rate, continuously compounded.</param>
        /// <returns></returns>
        public static Simulation BlackScholes(OptionContractType optionContractType,
                                              IParameter priceOfUnderlyingAsset,
                                              IParameter strikePrice,
                                              IParameter timeToMaturity,
                                              IParameter standardDeviationOfStocksReturns,
                                              IParameter riskFreeRate)
        {
            // NOTE: The subsequent string manipulation is my attempt at a slightly dirty hack to compute the CDF of d1 and d2 inside the simulation's expression. Originally, I had planned to use the DistributionFunctionParameter, but realized that the issue with this is that breaking this simulation into seperate equations would cause values used by multiple equations to be resimulated for each equation... Essentially, they would not be consistent between different equations in the same simulation, so the answer would have been useless. I am not entirely sure my workaround solves this issue, but by testing this against the above function I should be able to draw a conclusion as to whether or not this works. Hopefully it does, because I don't have a better idea as to how to allow Black-Scholes simulations in the context of the generic simulation. If it doesn't work, Black-Scholes will need to become it's own simulation type and the parameters will all need to be converted to precomputed parameters so that the equations can be broken up without issue.
            

            // rename parameters to work with shortened version of equation
            priceOfUnderlyingAsset.Name = "St";
            strikePrice.Name = "K";
            timeToMaturity.Name = "t";
            standardDeviationOfStocksReturns.Name = "o";
            riskFreeRate.Name = "r";

            // create the pieces needed to form either expression
            string d1 = "((1 / o * t^0.5) * (ln(St / K) + (r + o^2 / 2) * t))";
            string d2 = "(((1 / o * t^0.5) * (ln(St / K) + (r + o^2 / 2) * t)) - o * t^0.5)";
            string PV_K = "(K * e^(-r * t))";

            // create simulation based on type of option
            if (optionContractType == OptionContractType.Call)
            {
                // d's are positive for call option
                string t1 = $"(1 / (1 + 0.3275911 * {d1}))";
                string t2 = $"(1 / (1 + 0.3275911 * {d2}))";

                string N1 = $"(1 - ((((((1.061405429 * {t1} + -1.453152027) * {t1}) + 1.421413741) * {t1} + -0.284496736) * {t1}) + 0.254829592) * {t1} * e^(-1 * {d1} * {d1}))";
                string N2 = $"(1 - ((((((1.061405429 * {t2} + -1.453152027) * {t2}) + 1.421413741) * {t2} + -0.284496736) * {t2}) + 0.254829592) * {t2} * e^(-1 * {d2} * {d2}))";

                string expression = $"{N1} * St - {N2} * {PV_K}";

                return new Simulation(expression,
                                      priceOfUnderlyingAsset,
                                      strikePrice,
                                      timeToMaturity,
                                      standardDeviationOfStocksReturns,
                                      riskFreeRate);
            }
            else
            {
                // d's are negative for put option
                string t1 = $"(1 / (1 + 0.3275911 * -{d1}))";
                string t2 = $"(1 / (1 + 0.3275911 * -{d2}))";

                string N1 = $"(1 - ((((((1.061405429 * {t1} + -1.453152027) * {t1}) + 1.421413741) * {t1} + -0.284496736) * {t1}) + 0.254829592) * {t1} * e^(-1 * -{d1} * -{d1}))";
                string N2 = $"(1 - ((((((1.061405429 * {t2} + -1.453152027) * {t2}) + 1.421413741) * {t2} + -0.284496736) * {t2}) + 0.254829592) * {t2} * e^(-1 * -{d2} * -{d2}))";

                string expression = $"{N2} * {PV_K} - {N2} * St";

                return new Simulation(expression,
                                      priceOfUnderlyingAsset,
                                      strikePrice,
                                      timeToMaturity,
                                      standardDeviationOfStocksReturns,
                                      riskFreeRate);
            }
        }
    }
}
