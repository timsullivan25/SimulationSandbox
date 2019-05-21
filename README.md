# SimulationSandbox

A class library that allows various types of simulations to be performed and connected through algebraic expressions.

## Overview

The original inspiration for this project came from the [Math.NET Symbolics](https://symbolics.mathdotnet.com/) computer algebra library. This library provides a way to evaluate formulas written in a standard format. For example, you can provide a string such as "1/(1 + a)", specify that the value of "a" is 5, and evaluate the string the obtain a result of ~0.167.

The purpose of my library is to extend this functionality by allowing the value of "a" to be generated using a variety of methods. The goal is to provide an easy, flexible way to simulate different types of events using the same building blocks. As long as the outcome can be expressed with an algebraic expression, it can be evaluated by generating values for the underlying parameters. Furthermore, simulations can also be used as parameters for other simulations, essentially allowing you to construct a system of equations to define complex problems.

## Simulation Types

- Standard Simulation
- Dependent Simulation
- Sensitivity Simulation
- Exhaustive Sensitivity Simulation
- Qualitative Simulation
- Binomial Tree Simulation
- Binomial Option Pricing Simulation

## Parameter Types

#### Quantitative

- Conditional Parameter
- Constant Parameter
- Dependent Simulation Parameter
- Discrete Parameter
- Distribution Parameter
- Distribution Function Parameter
- Precomputed Parameter
- Qualitative Interpretation Parameter
- Random Bag Parameter
- Simulation Parameter

#### Qualitative

- Qualitative Parameter
- Qualitative Conditional Parameter
- Qualitative Random Bag Parameter

## License

I have intentionally chosen not to specify a license. If you wish to make use of this code, please reach out to me at tim.sullivan25@outlook.com to discuss the purpose of your project.
