﻿using MathNet.Numerics.Distributions;
using Simulations.Exceptions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Simulations
{
    /// <summary>
    /// Base implementation for FunctionSimulations. Interface is required so that GetParameterValues function can call simulate on any FunctionSimulation to generate values for the current simulation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFunctionSimulation<T>
    {
        T[] Simulate(int numberOfSimulations);
    }

    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T> : IFunctionSimulation<T>
    {
        public Func<T> Function { get; set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        public FunctionSimulation(Func<T> function)
        {
            Function = function;
        }

        /// <summary>
        /// Iterates over the function n number of times.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        public T[] Simulate(int numberOfSimulations)
        {
            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke();

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T> : IFunctionSimulation<T>
    {
        public Func<T1, T> Function { get; set; }
        public object Param1 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        public FunctionSimulation(Func<T1, T> function, object param1)
        {
            Function = function;
            Param1 = param1;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T> function, object param1, object param2)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);          

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T> function, object param1, object param2, object param3)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T> function, object param1, object param2, object param3, object param4)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T5">Type of fifth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T5, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T5, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }
        public object Param5 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        /// <param name="param5">The fifth parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T5, T> function, object param1, object param2, object param3, object param4, object param5)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
            Param5 = param5;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);
            object[] p5 = ParameterParsing.GetParameterValues<T5>(Param5, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i], p5[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T5">Type of fifth input parameter.</typeparam>
    /// <typeparam name="T6">Type of sixth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T5, T6, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T5, T6, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }
        public object Param5 { get; private set; }
        public object Param6 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        /// <param name="param5">The fifth parameter in the function.</param>
        /// <param name="param6">The sixth parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T5, T6, T> function, object param1, object param2, object param3, object param4, object param5, object param6)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
            Param5 = param5;
            Param6 = param6;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);
            object[] p5 = ParameterParsing.GetParameterValues<T5>(Param5, numberOfSimulations);
            object[] p6 = ParameterParsing.GetParameterValues<T6>(Param6, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i], p5[i], p6[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T5">Type of fifth input parameter.</typeparam>
    /// <typeparam name="T6">Type of sixth input parameter.</typeparam>
    /// <typeparam name="T6">Type of seventh input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T5, T6, T7, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T5, T6, T7, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }
        public object Param5 { get; private set; }
        public object Param6 { get; private set; }
        public object Param7 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        /// <param name="param5">The fifth parameter in the function.</param>
        /// <param name="param6">The sixth parameter in the function.</param>
        /// <param name="param7">The seventh parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T5, T6, T7, T> function, object param1, object param2, object param3, object param4, object param5, object param6, object param7)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
            Param5 = param5;
            Param6 = param6;
            Param7 = param7;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);
            object[] p5 = ParameterParsing.GetParameterValues<T5>(Param5, numberOfSimulations);
            object[] p6 = ParameterParsing.GetParameterValues<T6>(Param6, numberOfSimulations);
            object[] p7 = ParameterParsing.GetParameterValues<T7>(Param7, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i], p5[i], p6[i], p7[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T5">Type of fifth input parameter.</typeparam>
    /// <typeparam name="T6">Type of sixth input parameter.</typeparam>
    /// <typeparam name="T7">Type of seventh input parameter.</typeparam>
    /// <typeparam name="T8">Type of eigth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T5, T6, T7, T8, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T5, T6, T7, T8, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }
        public object Param5 { get; private set; }
        public object Param6 { get; private set; }
        public object Param7 { get; private set; }
        public object Param8 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        /// <param name="param5">The fifth parameter in the function.</param>
        /// <param name="param6">The sixth parameter in the function.</param>
        /// <param name="param7">The seventh parameter in the function.</param>
        /// <param name="param8">The eigth parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T5, T6, T7, T8, T> function, object param1, object param2, object param3, object param4, object param5, object param6, object param7, object param8)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
            Param5 = param5;
            Param6 = param6;
            Param7 = param7;
            Param8 = param8;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);
            object[] p5 = ParameterParsing.GetParameterValues<T5>(Param5, numberOfSimulations);
            object[] p6 = ParameterParsing.GetParameterValues<T6>(Param6, numberOfSimulations);
            object[] p7 = ParameterParsing.GetParameterValues<T7>(Param7, numberOfSimulations);
            object[] p8 = ParameterParsing.GetParameterValues<T8>(Param8, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i], p5[i], p6[i], p7[i], p8[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T5">Type of fifth input parameter.</typeparam>
    /// <typeparam name="T6">Type of sixth input parameter.</typeparam>
    /// <typeparam name="T7">Type of seventh input parameter.</typeparam>
    /// <typeparam name="T8">Type of eigth input parameter.</typeparam>
    /// <typeparam name="T9">Type of ninth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }
        public object Param5 { get; private set; }
        public object Param6 { get; private set; }
        public object Param7 { get; private set; }
        public object Param8 { get; private set; }
        public object Param9 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        /// <param name="param5">The fifth parameter in the function.</param>
        /// <param name="param6">The sixth parameter in the function.</param>
        /// <param name="param7">The seventh parameter in the function.</param>
        /// <param name="param8">The eigth parameter in the function.</param>
        /// <param name="param9">The ninth parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> function, object param1, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
            Param5 = param5;
            Param6 = param6;
            Param7 = param7;
            Param8 = param8;
            Param9 = param9;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);
            object[] p5 = ParameterParsing.GetParameterValues<T5>(Param5, numberOfSimulations);
            object[] p6 = ParameterParsing.GetParameterValues<T6>(Param6, numberOfSimulations);
            object[] p7 = ParameterParsing.GetParameterValues<T7>(Param7, numberOfSimulations);
            object[] p8 = ParameterParsing.GetParameterValues<T8>(Param8, numberOfSimulations);
            object[] p9 = ParameterParsing.GetParameterValues<T9>(Param9, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i], p5[i], p6[i], p7[i], p8[i], p9[i]);

            return results;
        }
    }
    /// <summary>
    /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
    /// </summary>
    /// <typeparam name="T1">Type of first input parameter.</typeparam>
    /// <typeparam name="T2">Type of second input parameter.</typeparam>
    /// <typeparam name="T3">Type of third input parameter.</typeparam>
    /// <typeparam name="T4">Type of fourth input parameter.</typeparam>
    /// <typeparam name="T5">Type of fifth input parameter.</typeparam>
    /// <typeparam name="T6">Type of sixth input parameter.</typeparam>
    /// <typeparam name="T7">Type of seventh input parameter.</typeparam>
    /// <typeparam name="T8">Type of eigth input parameter.</typeparam>
    /// <typeparam name="T9">Type of ninth input parameter.</typeparam>
    /// <typeparam name="T10">Type of tenth input parameter.</typeparam>
    /// <typeparam name="T">Output type of underlying function.</typeparam>
    public class FunctionSimulation<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> : IFunctionSimulation<T>
    {
        public Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> Function { get; set; }
        public object Param1 { get; private set; }
        public object Param2 { get; private set; }
        public object Param3 { get; private set; }
        public object Param4 { get; private set; }
        public object Param5 { get; private set; }
        public object Param6 { get; private set; }
        public object Param7 { get; private set; }
        public object Param8 { get; private set; }
        public object Param9 { get; private set; }
        public object Param10 { get; private set; }

        /// <summary>
        /// A simulation that iterates through function calls while changing the input parameters in order to generate a set of possible outcomes.
        /// </summary>
        /// <param name="function">Function over which to iterate.</param>
        /// <param name="param1">The first parameter in the function.</param>
        /// <param name="param2">The second parameter in the function.</param>
        /// <param name="param3">The third parameter in the function.</param>
        /// <param name="param4">The fourth parameter in the function.</param>
        /// <param name="param5">The fifth parameter in the function.</param>
        /// <param name="param6">The sixth parameter in the function.</param>
        /// <param name="param7">The seventh parameter in the function.</param>
        /// <param name="param8">The eigth parameter in the function.</param>
        /// <param name="param9">The ninth parameter in the function.</param>
        /// <param name="param10">The tenth parameter in the function.</param>
        public FunctionSimulation(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T> function, object param1, object param2, object param3, object param4, object param5, object param6, object param7, object param8, object param9, object param10)
        {
            Function = function;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Param4 = param4;
            Param5 = param5;
            Param6 = param6;
            Param7 = param7;
            Param8 = param8;
            Param9 = param9;
            Param10 = param10;
        }

        /// <summary>
        /// Iterates over the function n number of times using the provided parameters as input.
        /// </summary>
        /// <param name="numberOfSimulations">Number of times to invoke function.</param>
        /// <returns>An array of objects with the same type as the output value of the function.</returns>
        /// <remarks>IParameters generally produce an output type of double or int. The simulation will attempt to convert the output values into the type of the function input parameter, but there is no guarantee the conversion will be successful in every situation.</remarks>
        public T[] Simulate(int numberOfSimulations)
        {
            // generate input parameters
            object[] p1 = ParameterParsing.GetParameterValues<T1>(Param1, numberOfSimulations);
            object[] p2 = ParameterParsing.GetParameterValues<T2>(Param2, numberOfSimulations);
            object[] p3 = ParameterParsing.GetParameterValues<T3>(Param3, numberOfSimulations);
            object[] p4 = ParameterParsing.GetParameterValues<T4>(Param4, numberOfSimulations);
            object[] p5 = ParameterParsing.GetParameterValues<T5>(Param5, numberOfSimulations);
            object[] p6 = ParameterParsing.GetParameterValues<T6>(Param6, numberOfSimulations);
            object[] p7 = ParameterParsing.GetParameterValues<T7>(Param7, numberOfSimulations);
            object[] p8 = ParameterParsing.GetParameterValues<T8>(Param8, numberOfSimulations);
            object[] p9 = ParameterParsing.GetParameterValues<T9>(Param9, numberOfSimulations);
            object[] p10 = ParameterParsing.GetParameterValues<T10>(Param10, numberOfSimulations);

            // run function specified number of times
            T[] results = new T[numberOfSimulations];

            for (int i = 0; i < numberOfSimulations; i++)
                results[i] = (T)Function.DynamicInvoke(p1[i], p2[i], p3[i], p4[i], p5[i], p6[i], p7[i], p8[i], p9[i], p10[i]);

            return results;
        }
    }

    /// <summary>
    /// Main and helper methods for handling different types of simulation inputs.
    /// </summary>
    public static class ParameterParsing
    {
        /// <summary>
        /// Converts any parameter in an array of objects. IParameters will be parsed the same way they are in the StandardSimulation. FunctionSimulations will be simulated the necessary number of times and can be passed without pre-simulating the results. Collections will be passed through as objects or individual inputs based on the parameter type.
        /// </summary>
        /// <typeparam name="T">The desired type of the individual parameters. The type of value the function being simulated will accept as input.</typeparam>
        /// <param name="parameter">The parameter from which to generate an array of values.</param>
        /// <param name="numberOfSimulations">The number of values to generate.</param>
        /// <returns></returns>
        public static object[] GetParameterValues<T>(object parameter, int numberOfSimulations)
        {
            if (parameter is T)
            {
                return Repeat(parameter, numberOfSimulations);
            }
            else if (parameter is IParameter iParameter)
            {
                #region Conditional Parameter

                if (iParameter is ConditionalParameter)
                {
                    Simulation innerSimulation = (iParameter as ConditionalParameter).ReferenceParameter.Simulation;
                    double[] innerResults = innerSimulation.Simulate(numberOfSimulations).Results;
                    // we're saying an integer simulation will be converted to double.. we could just use the math.net floating point numbers to avoid issues here ??
                    // but the conversion doesn't seem to result in floating point comparison issues.. so we might be good?

                    ConditionalOutcome[] conditionalOutcomes = (iParameter as ConditionalParameter).ConditionalOutcomes;
                    double[] conditionalResults = new double[numberOfSimulations];

                    for (int s = 0; s < innerResults.Length; s++)
                    {
                        bool valueAssigned = false;

                        for (int c = 0; c < conditionalOutcomes.Length; c++)
                        {
                            switch (conditionalOutcomes[c].ComparisonOperator)
                            {
                                case ComparisonOperator.Equal:
                                    if (innerResults[s] == conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.NotEqual:
                                    if (innerResults[s] != conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.LessThan:
                                    if (innerResults[s] < conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.LessThanOrEqual:
                                    if (innerResults[s] <= conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.GreaterThan:
                                    if (innerResults[s] > conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.GreaterThanOrEqual:
                                    if (innerResults[s] >= conditionalOutcomes[c].ConditionalValue)
                                    {
                                        conditionalResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (valueAssigned)
                                break;
                        }

                        if (!valueAssigned)
                            conditionalResults[s] = (iParameter as ConditionalParameter).DefaultValue;
                    }

                    return ConvertAndPackArray(conditionalResults, typeof(T));
                }

                #endregion

                #region Constant Parameter

                else if (iParameter is ConstantParameter)
                {
                    double constant = (iParameter as ConstantParameter).Value;
                    double[] repeatValues = new double[numberOfSimulations];

                    for (int i = 0; i < numberOfSimulations; i++)
                        repeatValues[i] = constant;

                    return ConvertAndPackArray(repeatValues, typeof(T));
                }

                #endregion

                #region Dependent Simulation Parameter

                else if (iParameter is DependentSimulationParameter)
                {
                    DependentSimulationParameterReturnType returnType = (iParameter as DependentSimulationParameter).ReturnType;

                    if (returnType == DependentSimulationParameterReturnType.Results)
                    {
                        // if we are looking for results, we only need to run one simulation
                        // constraints cannot apply to the Results return type because each data point is dependent on the last
                        // i.e. we can't change one of them without having to change everything after it...
                        DependentSimulation innerSimulation = (iParameter as DependentSimulationParameter).DependentSimulation;
                        DependentSimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                        return ConvertAndPackArray(innerResults.Results, typeof(T));
                    }
                    else
                    {
                        // we need to run a bunch of inner simulations, so use as many threads as possible
                        int summaryRunCount = (iParameter as DependentSimulationParameter).SummaryRunCount;
                        DependentSimulation innerSimulation = (iParameter as DependentSimulationParameter).DependentSimulation;

                        #region Multithread Simulations

                        ConcurrentBag<DependentSimulationResults> innerSimulations = new ConcurrentBag<DependentSimulationResults>();
                        List<Task> innerSimulationTasks = new List<Task>();

                        for (int s = 0; s < numberOfSimulations; s++)
                        {
                            Task t = Task.Run(() =>
                            {
                                innerSimulations.Add(innerSimulation.Simulate(summaryRunCount));
                            });

                            innerSimulationTasks.Add(t);
                        }

                        Task.WaitAll(innerSimulationTasks.ToArray());

                        #endregion

                        #region Unpack Summary Statistics

                        // use an array to hold invidual summary statistics
                        double[] summaryStatistics = new double[numberOfSimulations];
                        int i = 0;

                        foreach (var simulation in innerSimulations)
                        {
                            summaryStatistics[i] = simulation.GetSummaryStatistic(returnType);
                            i++;
                        }

                        #endregion

                        #region Check Constraints

                        // checking constraintas is valid for the summary statistic return type, because they are not dependent on each other
                        if ((iParameter as DependentSimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as DependentSimulationParameter).Constraint;

                            for (int s = 0; s < summaryStatistics.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && summaryStatistics[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && summaryStatistics[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        summaryStatistics[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            DependentSimulationResults newResults = innerSimulation.Simulate(summaryRunCount);
                                            double newSample = newResults.GetSummaryStatistic(returnType);

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                summaryStatistics[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            summaryStatistics[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        // assign summary results to output
                        return ConvertAndPackArray(summaryStatistics, typeof(T));
                    }
                }

                #endregion

                #region Discrete Parameter

                else if (iParameter is DiscreteParameter)
                {
                    // generate array of values between 0 and 1
                    double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new ContinuousUniform(0d, 1d));
                    double[] correspondingValues = new double[numberOfSimulations];

                    // go through array of values and use cumulative probability of outcomes to determine the value for each simulation
                    for (int s = 0; s < numberOfSimulations; s++)
                    {
                        DiscreteOutcome[] possibleOutcomes = (iParameter as DiscreteParameter).PossibleOutcomes;

                        for (int o = 0; o < possibleOutcomes.Length; o++)
                        {
                            if (randomProbabilities[s] <= possibleOutcomes[o]._cumulativeProbability)
                            {
                                correspondingValues[s] = possibleOutcomes[o].Value;
                                break;
                            }
                        }
                    }

                    return ConvertAndPackArray(correspondingValues, typeof(T));
                }

                #endregion

                #region Distribution Parameter

                else if (iParameter is DistributionParameter)
                {
                    if ((iParameter as DistributionParameter).Distribution is IDiscreteDistribution)
                    {
                        // must use integers for discrete distributions
                        int[] samples = new int[numberOfSimulations];
                        ((dynamic)(iParameter as DistributionParameter).Distribution).Samples(samples);

                        #region Check Constraints

                        if ((iParameter as DistributionParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as DistributionParameter).Constraint;

                            for (int s = 0; s < samples.Length; s++)
                            {
                                // this is a special case that requires integer conversion for reassigned values
                                // not sure if we'll run into conversion errors on comparison, or if it will forceable retype ints
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && samples[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = Convert.ToInt32(constraint.LowerBound);
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && samples[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = Convert.ToInt32(constraint.UpperBound);
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        samples[s] = Convert.ToInt32(constraint.DefaultValue);
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            int newSample = ((dynamic)(iParameter as DistributionParameter).Distribution).Sample();

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                samples[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            samples[s] = Convert.ToInt32(constraint.DefaultValue);
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        return ConvertAndPackArray(samples, typeof(T));
                    }
                    else
                    {
                        // use doubles for continuous distribution
                        double[] samples = new double[numberOfSimulations];
                        ((dynamic)(iParameter as DistributionParameter).Distribution).Samples(samples);

                        #region Check Constraints

                        if ((iParameter as DistributionParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as DistributionParameter).Constraint;

                            for (int s = 0; s < samples.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && samples[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && samples[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        samples[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            double newSample = ((dynamic)(iParameter as DistributionParameter).Distribution).Sample();

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                samples[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            samples[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        return ConvertAndPackArray(samples, typeof(T));
                    }
                }

                #endregion

                #region Distribution Function Parameter

                else if (iParameter is DistributionFunctionParameter)
                {
                    try
                    {
                        // compute location values using the underlying location parameter
                        double[] locationValues = new Simulation("location", (iParameter as DistributionFunctionParameter).LocationParameter).Simulate(numberOfSimulations).Results;
                        double[] computeValues = new double[numberOfSimulations];

                        // use distribution, function, and location to compute results                   
                        switch ((iParameter as DistributionFunctionParameter).ReturnType)
                        {
                            case DistributionFunctionParameterReturnType.CumulativeDistribution:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).CumulativeDistribution(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.Density:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).Density(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.DensityLn:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).DensityLn(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.InverseCumulativeDistribution:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).InverseCumulativeDistribution(locationValues[i]);
                                break;
                            case DistributionFunctionParameterReturnType.Probability:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).Probability(Convert.ToInt32(locationValues[i]));
                                break;
                            case DistributionFunctionParameterReturnType.ProbabilityLn:
                                for (int i = 0; i < numberOfSimulations; i++)
                                    ((dynamic)(iParameter as DistributionFunctionParameter).Distribution).ProbabilityLn(Convert.ToInt32(locationValues[i]));
                                break;
                            default:
                                break;
                        }

                        return ConvertAndPackArray(computeValues, typeof(T));
                    }
                    catch (Exception innerException)
                    {
                        throw new DistributionFunctionFailureException($"Failed to compute {(iParameter as DistributionFunctionParameter).ReturnType} function for {(iParameter as DistributionFunctionParameter).Distribution.GetType()} distribution. This function may not be valid for this type of distribution. Check the MathNet.Numerics.Distributions documentation and the inner exception for more details.", innerException);
                    }
                }

                #endregion

                #region Precomputed Parameter

                else if (iParameter is PrecomputedParameter param)
                {
                    if (param.PrecomputedValues.Length != numberOfSimulations)
                            throw new PrecomputedValueCountException($"{param.Name} has {param.PrecomputedValues.Length} precomputed values but the simulation being run expects {numberOfSimulations} values.");

                    return ConvertAndPackArray(param.PrecomputedValues, typeof(T));
                }

                #endregion

                #region Qualitative Interpretation Parameter

                else if (iParameter is QualitativeInterpretationParameter)
                {
                    // simulate the set of qualitative data
                    IQualitativeParameter qualitativeParameter = (iParameter as QualitativeInterpretationParameter).QualitativeParameter;
                    QualitativeSimulation qualitativeSimulation = new QualitativeSimulation(qualitativeParameter);
                    string[] qualitativeOutcomes = qualitativeSimulation.Simulate(numberOfSimulations).Results;

                    // interpret outcomes
                    Dictionary<string, double> interpretationDictionary = (iParameter as QualitativeInterpretationParameter).InterpretationDictionary;
                    double[] interpretedResults = new double[numberOfSimulations];

                    for (int outcome = 0; outcome < numberOfSimulations; outcome++)
                    {
                        if (interpretationDictionary.ContainsKey(qualitativeOutcomes[outcome]))
                        {
                            interpretedResults[outcome] = interpretationDictionary[qualitativeOutcomes[outcome]];
                        }
                        else
                        {
                            interpretedResults[outcome] = (iParameter as QualitativeInterpretationParameter).DefaultValue;
                        }
                    }

                    return ConvertAndPackArray(interpretedResults, typeof(T));
                }

                #endregion

                #region Random Bag Parameter

                else if (iParameter is RandomBagParameter)
                {
                    RandomBagParameter randomBag = (RandomBagParameter)iParameter;
                    double[] selections = new double[numberOfSimulations];

                    if (randomBag.IsEmpty)
                        throw new EmptyBagException($"RandomBag parameters must contain at least one object before they can be used in a simulation.");

                    if (randomBag.ReplacementRule == RandomBagReplacement.AfterEachPick)
                    {
                        double[] contents = randomBag.ContentsToArray();
                        int[] selectedIndices = new int[numberOfSimulations];
                        DiscreteUniform distribution = new DiscreteUniform(0, randomBag.NumberOfItems - 1);
                        distribution.Samples(selectedIndices);

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectedIndices[i]];
                    }
                    else if (randomBag.ReplacementRule == RandomBagReplacement.Never)
                    {
                        // validate we can choose the correct number of items
                        if (randomBag.NumberOfItems < numberOfSimulations)
                            throw new RandomBagItemCountException($"{numberOfSimulations} selections were requested from a bag containing {randomBag.NumberOfItems} items. This is not possible when the replacement rule is set to never.");

                        // choose without replacement
                        double[] contents = randomBag.ContentsToArray();
                        int[] selectionOrder = Enumerable.Range(0, numberOfSimulations).ToArray();
                        selectionOrder.Shuffle();

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectionOrder[i]];
                    }
                    else if (randomBag.ReplacementRule == RandomBagReplacement.WhenEmpty)
                    {
                        double[] contents = randomBag.ContentsToArray();
                        int numberOfRefills = (numberOfSimulations / randomBag.NumberOfItems) - (numberOfSimulations % randomBag.NumberOfItems == 0 ? 1 : 0);
                        int[] selectionOrder = new int[(numberOfRefills + 1) * numberOfSimulations];

                        // simulate refilling the bag to get enough indices to satisfy the number of simulations
                        for (int i = 0; i < numberOfRefills + 1; i++)
                        {
                            int[] roundSelectionOrder = Enumerable.Range(0, randomBag.NumberOfItems).ToArray();
                            roundSelectionOrder.Shuffle();

                            for (int j = 0; j < roundSelectionOrder.Length; j++)
                                selectionOrder[j + (i * randomBag.NumberOfItems)] = roundSelectionOrder[j];
                        }

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectionOrder[i]];
                    }
                    else
                    {
                        throw new RandomBagReplacementRuleException($"{randomBag.ReplacementRule} is not a valid replacement rule.");
                    }

                    return ConvertAndPackArray(selections, typeof(T));
                }

                #endregion

                #region Simulation Parameter

                else if (iParameter is SimulationParameter)
                {
                    SimulationParameterReturnType returnType = (iParameter as SimulationParameter).ReturnType;

                    if (returnType == SimulationParameterReturnType.Results)
                    {
                        // if we are looking for results, we only need to run one simulation
                        Simulation innerSimulation = (iParameter as SimulationParameter).Simulation;
                        SimulationResults innerResults = innerSimulation.Simulate(numberOfSimulations);
                        double[] samples = innerResults.Results;

                        #region Check Constraints

                        if ((iParameter as SimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as SimulationParameter).Constraint;

                            for (int s = 0; s < samples.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && samples[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && samples[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        samples[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        samples[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            // standard simulation only requires one data point
                                            // might be better to generate more than one sample to avoid overhead of parameter validation each time?
                                            double newSample = (iParameter as SimulationParameter).Simulation.Simulate(1).First;

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                samples[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            samples[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        return ConvertAndPackArray(samples, typeof(T));
                    }
                    else
                    {
                        // we need to run a bunch of inner simulations, so use as many threads as possible
                        int summaryRunCount = (iParameter as SimulationParameter).SummaryRunCount;
                        Simulation innerSimulation = (iParameter as SimulationParameter).Simulation;

                        #region Multithread Simulations

                        ConcurrentBag<SimulationResults> innerSimulations = new ConcurrentBag<SimulationResults>();
                        List<Task> innerSimulationTasks = new List<Task>();

                        for (int s = 0; s < numberOfSimulations; s++)
                        {
                            Task t = Task.Run(() =>
                            {
                                innerSimulations.Add(innerSimulation.Simulate(summaryRunCount));
                            });

                            innerSimulationTasks.Add(t);
                        }

                        Task.WaitAll(innerSimulationTasks.ToArray());

                        #endregion

                        #region Unpack Summary Statistic

                        // use an array to hold invidual summary statistics
                        double[] summaryStatistics = new double[numberOfSimulations];
                        int i = 0;

                        foreach (var simulation in innerSimulations)
                        {
                            summaryStatistics[i] = simulation.GetSummaryStatistic(returnType);
                            i++;
                        }

                        #endregion

                        #region Check Constraints

                        if ((iParameter as SimulationParameter).Constraint != null)
                        {
                            ParameterConstraint constraint = (iParameter as SimulationParameter).Constraint;

                            for (int s = 0; s < summaryStatistics.Length; s++)
                            {
                                bool requiresResolution = false;

                                // check if constraint was violated, handle closest bound resolution
                                if (constraint.LowerBound != null && summaryStatistics[s] < constraint.LowerBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.LowerBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }
                                else if (constraint.UpperBound != null && summaryStatistics[s] > constraint.UpperBound)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.ClosestBound)
                                    {
                                        summaryStatistics[s] = (double)constraint.UpperBound;
                                        continue;
                                    }
                                    else
                                    {
                                        requiresResolution = true;
                                    }
                                }

                                // handle violations when the resolution is not closest bound
                                if (requiresResolution)
                                {
                                    if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.DefaultValue)
                                    {
                                        summaryStatistics[s] = (double)constraint.DefaultValue;
                                    }
                                    else if (constraint.ConstraintViolationResolution == ConstraintViolationResolution.Resimulate)
                                    {
                                        bool successfulResimulation = false;

                                        // resimulate up to the maximum number of times
                                        for (int rs = 0; rs < constraint.MaxResimulations; rs++)
                                        {
                                            SimulationResults newResults = innerSimulation.Simulate(summaryRunCount);
                                            double newSample = newResults.GetSummaryStatistic(returnType);

                                            if ((constraint.LowerBound == null || newSample >= constraint.LowerBound)
                                                && (constraint.UpperBound == null || newSample <= constraint.UpperBound))
                                            {
                                                // stop if we generate a valid data point
                                                summaryStatistics[s] = newSample;
                                                successfulResimulation = true;
                                                break;
                                            }
                                        }

                                        if (!successfulResimulation)
                                        {
                                            // revert back to default if we hit max number of simulations without an acceptable value
                                            summaryStatistics[s] = (double)constraint.DefaultValue;
                                        }
                                    }
                                    else
                                    {
                                        throw new InvalidResolutionException($"{constraint.ConstraintViolationResolution} is not a valid resolution type.");
                                    }
                                }
                            }
                        }

                        #endregion

                        // assign summary results to output
                        return ConvertAndPackArray(summaryStatistics, typeof(T));
                    }
                }

                #endregion 

                #region Invalid Parameter

                else
                {
                    throw new InvalidParameterException($"{iParameter.Name} is a {iParameter.GetType()} parameter, which is not one the parameter types that simulations are able to handle. Acceptable parameter types are: Conditional, Constant, Discrete, Distribution, Precomputed, Simulation, DependentSimulation, and QualitativeInterpretation.");
                }

                #endregion
            }
            else if (parameter is IQualitativeParameter iQualitativeParameter)
            {
                string[] qualitativeResults = new string[numberOfSimulations];

                #region Qualitative Parameter (exhaustive outcomes)

                if (iQualitativeParameter is QualitativeParameter)
                {
                    // generate array of values between 0 and 1
                    double[] randomProbabilities = MathNet.Numerics.Generate.Random(numberOfSimulations, new ContinuousUniform(0d, 1d));

                    // go through array of values and use cumulative probability of outcomes to determine the value for each simulation
                    for (int s = 0; s < numberOfSimulations; s++)
                    {
                        QualitativeOutcome[] possibleOutcomes = (iQualitativeParameter as QualitativeParameter).PossibleOutcomes;

                        for (int o = 0; o < possibleOutcomes.Length; o++)
                        {
                            if (randomProbabilities[s] <= possibleOutcomes[o]._cumulativeProbability)
                            {
                                qualitativeResults[s] = possibleOutcomes[o].Value;
                                break;
                            }
                        }
                    }
                }

                #endregion

                #region Conditional Parameter (based on underlying simulation)

                else if (iQualitativeParameter is QualitativeConditionalParameter)
                {
                    // get results of underlying simulation
                    Simulation underlyingSimulation = new Simulation(expression: (iQualitativeParameter as QualitativeConditionalParameter).ReferenceParameter.Name,
                                                                     parameters: (iQualitativeParameter as QualitativeConditionalParameter).ReferenceParameter);

                    double[] underlyingResults = underlyingSimulation.Simulate(numberOfSimulations).Results;

                    // interpret results
                    QualitativeConditionalOutcome[] conditionalOutcomes = (iQualitativeParameter as QualitativeConditionalParameter).ConditionalOutcomes;

                    for (int s = 0; s < underlyingResults.Length; s++)
                    {
                        bool valueAssigned = false;

                        for (int c = 0; c < conditionalOutcomes.Length; c++)
                        {
                            switch (conditionalOutcomes[c].ComparisonOperator)
                            {
                                case ComparisonOperator.Equal:
                                    if (underlyingResults[s] == conditionalOutcomes[c].ConditionalValue)
                                    {
                                        qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.NotEqual:
                                    if (underlyingResults[s] != conditionalOutcomes[c].ConditionalValue)
                                    {
                                        qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.LessThan:
                                    if (underlyingResults[s] < conditionalOutcomes[c].ConditionalValue)
                                    {
                                        qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.LessThanOrEqual:
                                    if (underlyingResults[s] <= conditionalOutcomes[c].ConditionalValue)
                                    {
                                        qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.GreaterThan:
                                    if (underlyingResults[s] > conditionalOutcomes[c].ConditionalValue)
                                    {
                                        qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                case ComparisonOperator.GreaterThanOrEqual:
                                    if (underlyingResults[s] >= conditionalOutcomes[c].ConditionalValue)
                                    {
                                        qualitativeResults[s] = conditionalOutcomes[c].ReturnValue;
                                        valueAssigned = true;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (valueAssigned)
                                break;
                        }

                        if (!valueAssigned)
                            qualitativeResults[s] = (iQualitativeParameter as QualitativeConditionalParameter).DefaultValue;
                    }
                }

                #endregion

                #region Random Bag Parameter

                else if (iQualitativeParameter is QualitativeRandomBagParameter)
                {
                    QualitativeRandomBagParameter randomBag = (QualitativeRandomBagParameter)iQualitativeParameter;
                    if (randomBag.IsEmpty) throw new EmptyBagException($"QualitativeRandomBag parameters must contain at least one object before they can be used in a simulation.");
                    string[] selections = new string[numberOfSimulations];

                    if (randomBag.ReplacementRule == RandomBagReplacement.AfterEachPick)
                    {
                        string[] contents = randomBag.ContentsToArray();
                        int[] selectedIndices = new int[numberOfSimulations];
                        DiscreteUniform distribution = new DiscreteUniform(0, randomBag.NumberOfItems - 1);
                        distribution.Samples(selectedIndices);

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectedIndices[i]];
                    }
                    else if (randomBag.ReplacementRule == RandomBagReplacement.Never)
                    {
                        // validate we can choose the correct number of items
                        if (randomBag.NumberOfItems < numberOfSimulations)
                            throw new RandomBagItemCountException($"{numberOfSimulations} selections were requested from a bag containing {randomBag.NumberOfItems} items. This is not possible when the replacement rule is set to never.");

                        // choose without replacement
                        string[] contents = randomBag.ContentsToArray();
                        int[] selectionOrder = Enumerable.Range(0, randomBag.NumberOfItems).ToArray();
                        selectionOrder.Shuffle();

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectionOrder[i]];
                    }
                    else if (randomBag.ReplacementRule == RandomBagReplacement.WhenEmpty)
                    {
                        string[] contents = randomBag.ContentsToArray();
                        int numberOfRefills = (numberOfSimulations / randomBag.NumberOfItems) - (numberOfSimulations % randomBag.NumberOfItems == 0 ? 1 : 0);
                        int[] selectionOrder = new int[(numberOfRefills + 1) * numberOfSimulations];

                        // simulate refilling the bag to get enough indices to satisfy the number of simulations
                        for (int i = 0; i < numberOfRefills + 1; i++)
                        {
                            int[] roundSelectionOrder = Enumerable.Range(0, randomBag.NumberOfItems).ToArray();
                            roundSelectionOrder.Shuffle();

                            for (int j = 0; j < roundSelectionOrder.Length; j++)
                                selectionOrder[j + (i * randomBag.NumberOfItems)] = roundSelectionOrder[j];
                        }

                        for (int i = 0; i < numberOfSimulations; i++)
                            selections[i] = contents[selectionOrder[i]];
                    }
                    else
                    {
                        throw new RandomBagReplacementRuleException($"{randomBag.ReplacementRule} is not a valid replacement rule.");
                    }

                    qualitativeResults = selections;
                }

                #endregion

                #region Invalid Parameter

                else
                {
                    throw new InvalidParameterException($"{iQualitativeParameter.Name} is not a valid qualitative parameter.");
                }

                #endregion

                return ConvertAndPackArray(qualitativeResults, typeof(T));
            }
            else if (parameter is IDiscreteDistribution discreteDistribution)
            {
                // must use integers for discrete distributions
                int[] samples = new int[numberOfSimulations];
                discreteDistribution.Samples(samples);

                return ConvertAndPackArray(samples, typeof(T));
            }
            else if (parameter is IContinuousDistribution continuousDistribution)
            {
                // use doubles for continuous distribution
                double[] samples = new double[numberOfSimulations];
                continuousDistribution.Samples(samples);

                return ConvertAndPackArray(samples, typeof(T));
            }            
            else if (parameter is ICollection inputs)
            {
                // assume collection is meant to be a list if the function parameter is not also a collection
                // could be issues if collection of inputs is shorter than number of simulations requested
                object[] inputList = new object[inputs.Count];
                inputs.CopyTo(inputList, 0);
                return inputList;
            }
            else if (parameter is IFunctionSimulation<T> functionSimulation)
            {
                return ConvertAndPackArray(functionSimulation.Simulate(numberOfSimulations), typeof(T));
            }
            else 
            {
                // not sure it's a good thing if we end up here
                // it would mean they passed a type not consistent with input parameter type
                // and also not a special type that can be parsed

                try
                {
                    // try making a minor type adjustment for different number formats
                    return Repeat(parameter is T ? parameter : Convert.ChangeType(parameter, typeof(T)),
                                  numberOfSimulations);
                }
                catch
                {
                    // if the above doesn't work, there's not much else we can do
                    throw new Exception($"Parameter of type {parameter.GetType()} is not consistent with the type of input expected by the function and is not a special type that can be parsed.");
                }
            }
        }

        /// <summary>
        /// Creates an array with n amount of identical objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="numberOfTimes"></param>
        /// <returns></returns>
        private static T[] Repeat<T>(T obj, int numberOfTimes)
        {
            T[] arr = new T[numberOfTimes];
            for (int i = 0; i < numberOfTimes; i++)
                arr[i] = obj;

            return arr;
        }

        /// <summary>
        /// Takes an array of any type, converts the objects to the type of the function parameter, then returns a generic array of objects. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <returns></returns>
        private static object[] ConvertAndPackArray<T>(T[] arr, Type parameterType)
        {
            int length = arr.Length;
            object[] values = new object[length];
            for (int i = 0; i < length; i++)
                values[i] = arr[i] is T ? arr[i] : Convert.ChangeType(arr[i], parameterType);

            return values;
        }
    }

    /// <summary>
    /// A wrapper for parsing simulation outcomes when you know the outcomes are of a type that can be converted to double.
    /// </summary>
    /// <typeparam name="T">Type of object contained in results array.</typeparam>
    public class NumericSimulationResults<T>
    {
        public T[] Results { get; private set; }
        private double[] _results { get; set; }

        // public summary statistics
        public int NumberOfSimulations
        {
            get
            {
                return Results.Length;
            }
        }
        public T First
        {
            get
            {
                return Results.First();
            }
        }
        public T Last
        {
            get
            {
                return Results.Last();
            }
        }
        public double Minimum
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Minimum(_results);
            }
        }
        public double LowerQuartile
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.LowerQuartileInplace(_results);
            }
        }
        public double Mean
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Mean(_results);
            }
        }
        public double Median
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.MedianInplace(_results);
            }
        }
        public double UpperQuartile
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.UpperQuartileInplace(_results);
            }
        }
        public double Maximum
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Maximum(_results);
            }
        }
        public double Variance
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.Variance(_results);
            }
        }
        public double StandardDeviation
        {
            get
            {
                return MathNet.Numerics.Statistics.ArrayStatistics.StandardDeviation(_results);
            }
        }
        public double Kurtosis
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Kurtosis(_results);
            }
        }
        public double Skewness
        {
            get
            {
                return MathNet.Numerics.Statistics.Statistics.Skewness(_results);
            }
        }

        // summary methods
        /// <summary>
        /// Returns the confidence interval of the results assuming that they are normally distributed.
        /// </summary>
        /// <param name="confidenceLevel">Confidence level to use when generating the confidence interval.</param>
        /// <returns></returns>
        public ConfidenceInterval ConfidenceInterval(ConfidenceLevel confidenceLevel)
        {
            double z = confidenceLevel.ZScore();
            double sqrtOfNumberOfSimulations = Math.Sqrt(NumberOfSimulations);
            double standardDeviation = StandardDeviation;
            double upperBound = Mean + z * (standardDeviation / sqrtOfNumberOfSimulations);
            double lowerBound = Mean - z * (standardDeviation / sqrtOfNumberOfSimulations);
            return new ConfidenceInterval(confidenceLevel, lowerBound, upperBound);
        }
        /// <summary>
        /// Returns the summary statistic associated with the passed enum. Can only return summary statistics that are of type double.
        /// </summary>
        /// <param name="summaryStatistic">Enum corresponding to the desired summary statistic.</param>
        /// <returns></returns>
        public double GetSummaryStatistic(SimulationParameterReturnType summaryStatistic)
        {
            switch (summaryStatistic)
            {
                case SimulationParameterReturnType.Minimum:
                    return Minimum;
                case SimulationParameterReturnType.LowerQuartile:
                    return LowerQuartile;
                case SimulationParameterReturnType.Mean:
                    return Mean;
                case SimulationParameterReturnType.Median:
                    return Median;
                case SimulationParameterReturnType.UpperQuartile:
                    return UpperQuartile;
                case SimulationParameterReturnType.Maximum:
                    return Maximum;
                case SimulationParameterReturnType.Variance:
                    return Variance;
                case SimulationParameterReturnType.StandardDeviation:
                    return StandardDeviation;
                case SimulationParameterReturnType.Kurtosis:
                    return Kurtosis;
                case SimulationParameterReturnType.Skewness:
                    return Skewness;
                default:
                    throw new InvalidSummaryStatisticException($"Cannot return {summaryStatistic}. Only statistics that return a single number can be accessed using this function.");
            }
        }
        
        /// <summary>
        /// Creates a results object with pre-computed summary statistics. Can easily be used in conjunction with a function simulation to generate a results object in one line of code and speed up the time it takes to analyze the outcomes.
        /// </summary>
        /// <param name="rawData">A collection of numeric outcomes, presumably from a function simulation.</param>
        public NumericSimulationResults(ICollection<T> rawData)
        {
            Results = rawData.ToArray();
            _results = rawData.Select(r => Convert.ToDouble(r)).ToArray();
        }
    }

    /// <summary>
    /// A class used to generate a series of values between a start and an end value using an interpolation rule.
    /// </summary>
    public class Range
    {
        public double Start { get; set; }
        public double End { get; set; }
        public RangeInterpolation Interpolation { get; set; }

        /// <summary>
        /// Creates a generator that will use the interpolation rule to generate any number of values between the start and end value.
        /// </summary>
        /// <param name="start">First value in the range.</param>
        /// <param name="end">Last value in the range.</param>
        /// <param name="interpolation">Rule for determining how to generate values between the start and end values.</param>
        public Range(double start, double end, RangeInterpolation interpolation = RangeInterpolation.Linear)
        {
            Start = start;
            End = end;
            Interpolation = interpolation;
        }

        /// <summary>
        /// Uses the interpolation rule to generate the specified number of values between the start and end values.
        /// </summary>
        /// <param name="numberOfValues">Number of values range should contain, including start and end values.</param>
        /// <returns></returns>
        public double[] GetValues(int numberOfValues)
        {
            double[] values = new double[numberOfValues];

            if (Interpolation == RangeInterpolation.Linear)
            {
                // should probably throw error unless start and end are the same if they only request 1 value (or negative number)
                double step = numberOfValues == 1 ? 0 : (End - Start) / (numberOfValues - 1);
                values[0] = Start;
                values[numberOfValues - 1] = End;

                for (int i = 1; i < numberOfValues - 1; i++)
                {
                    values[i] = values[i - 1] + step;
                }
            }
            else
            {
                throw new Exception("RangeInterpolation rule not yet implemented.");
            }

            return values;
        }
    }

    /// <summary>
    /// A collection of numbers. Primarily used for sensitivity analysis.
    /// </summary>
    public class Set
    {
        public double[] Members { get; set; }

        /// <summary>
        /// Generate a set containing the specified numbers.
        /// </summary>
        /// <param name="members">A collection of numbers to include in the set.</param>
        public Set(ICollection<double> members)
        {
            Members = members.ToArray();
        }

        /// <summary>
        /// Generate a set using a range object.
        /// </summary>
        /// <param name="range">The range object containing a start and end value and an interpolation rule.</param>
        /// <param name="numberOfValues">The number of values to include in the set (and therefore interpolate).</param>
        public Set(Range range, int numberOfValues)
        {
            Members = range.GetValues(numberOfValues);
        }
    }
}
