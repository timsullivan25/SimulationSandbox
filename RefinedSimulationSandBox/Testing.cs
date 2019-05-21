using MathNet.Numerics.Distributions;
using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinedSimulationSandBox
{
    class Testing
    {
        public static int _numberOfSimulations = 100000;

        #region Quantitative

        public static SimulationResults DoubleDiceRoll()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DiscreteParameter("DiceRoll1", new DiscreteOutcome(1, 0.167),
                                                   new DiscreteOutcome(2, 0.167),
                                                   new DiscreteOutcome(3, 0.167),
                                                   new DiscreteOutcome(4, 0.167),
                                                   new DiscreteOutcome(5, 0.167),
                                                   new DiscreteOutcome(6, 0.167)),

                new DiscreteParameter("DiceRoll2", new DiscreteOutcome(1, 0.167),
                                                   new DiscreteOutcome(2, 0.167),
                                                   new DiscreteOutcome(3, 0.167),
                                                   new DiscreteOutcome(4, 0.167),
                                                   new DiscreteOutcome(5, 0.167),
                                                   new DiscreteOutcome(6, 0.167)),
            };

            // create an expression based on the parameters
            string expression = "DiceRoll1 + DiceRoll2";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults TwentySidedDie()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("twentysided", new DiscreteUniform(1, 20))
            };

            // create an expression based on the parameters
            string expression = "twentysided";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults TwoTwentySidedDie()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("twentysided1", new DiscreteUniform(1, 20)),
                new DistributionParameter("twentysided2", new DiscreteUniform(1, 20))
            };

            // create an expression based on the parameters
            string expression = "twentysided1 + twentysided2";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults RegularTenSidedDie()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("dieroll", new DiscreteUniform(1, 10))
            };

            // create an expression based on the parameters
            string expression = "dieroll";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults ScaledTenSidedDie()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("scalevalue", 10),
                new SimulationParameter("tensidedsimulation", 
                                         new Simulation("dieroll",
                                                        new DistributionParameter("dieroll", new DiscreteUniform(1, 10))))
                
            };

            // create an expression based on the parameters
            string expression = "scalevalue * tensidedsimulation";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults CAPM()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf)";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults ConditionalTest()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new ConditionalParameter(name: "Action",
                                         referenceParameter: new DiscreteParameter("CoinFlip",
                                                                                    new DiscreteOutcome(0, 0.50),
                                                                                    new DiscreteOutcome(1, 0.50)),
                                         defaultValue: 0,
                                         conditionalOutcomes: new ConditionalOutcome[] {
                                                                                            new ConditionalOutcome(ComparisonOperator.GreaterThan, 0.50, 10),
                                                                                            new ConditionalOutcome(ComparisonOperator.LessThanOrEqual, 0.50, 5)
                                                                                       })
            };

            // create an expression based on the parameters
            string expression = "Action";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults ConditionalIntegerTest()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new ConditionalParameter(name: "Action",
                                         referenceParameter: new DiscreteParameter("CoinFlip",
                                                                                    new DiscreteOutcome(1, 0.50),
                                                                                    new DiscreteOutcome(2, 0.50)),
                                         defaultValue: 0,
                                         conditionalOutcomes: new ConditionalOutcome[] {
                                                                                            new ConditionalOutcome(ComparisonOperator.Equal, 1, 10),
                                                                                            new ConditionalOutcome(ComparisonOperator.Equal, 2, 100)
                                                                                       })
            };

            // create an expression based on the parameters
            string expression = "Action / 10";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults PrecomputedValidationErrorTest()
        {
            FloatingPoint[] precomputedValues = new FloatingPoint[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("C", 10),
                new PrecomputedParameter("P", precomputedValues)
            };

            // create an expression based on the parameters
            string expression = "C * P";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static SimulationResults PrecomputedTest()
        {
            FloatingPoint[] precomputedValues = new FloatingPoint[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new ConstantParameter("C", 10),
                new PrecomputedParameter("P", precomputedValues)
            };

            // create an expression based on the parameters
            string expression = "C * P";

            // perform simulations
            int numberOfSimulations = 10;
            Simulation simulation = new Simulation(expression, parameters);
            return simulation.Simulate(numberOfSimulations);
        }

        public static void RecomputeTest()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf)";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            var intialResults = simulation.Simulate(numberOfSimulations);

            // recompute expression
            expression = "Rf";
            var rfOnly = intialResults.RecomputeExpression(expression);

            expression = "2 * (Rf + B * (Rm - Rf))";
            var doubleTheValue = intialResults.RecomputeExpression(expression);
        }

        public static void RegenerationTest()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("B", new Normal(1, 0.1)),
                new ConstantParameter("Rf", 0.02),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf)";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            var initialResults = simulation.Simulate(numberOfSimulations);

            // regenerate same number of results
            var newResults = initialResults.Regenerate();

            // regenerate new number of results
            var newNumber = initialResults.Regenerate(100);
        }

        public static void AddAndRemoveParameters()
        {
            IParameter[] parameters = new IParameter[]
            {
                new DistributionParameter("a", new Normal(0, 1)),
                new DistributionParameter("b", new Normal(1, 1)),
                new DistributionParameter("c", new Normal(2, 1)),
                new DistributionParameter("d", new Normal(3, 1)),
                new DistributionParameter("e", new Normal(4, 1))
            };

            string expression = "a + b + c";
            Simulation simulation = new Simulation(expression, parameters);
            var results = simulation.Simulate(10);

            // remove b --> should throw an error
            try { results.RemoveParameter(parameters[1]); }
            catch { }

            // remove d --> should not be an issue
            results.RemoveParameter(parameters[4]);

            // add parameter f
            results.AddParameter(new DistributionParameter("f", new Normal(5, 1)));

            // change expression
            results.RecomputeExpression("a + b + c + d");
        }

        #endregion

        #region Qualitative

        public static QualitativeSimulationResults DiceRoll()
        {
            //define possible outcomes
            QualitativeOutcome []
            possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.167),
                new QualitativeOutcome("2", 0.167),
                new QualitativeOutcome("3", 0.167),
                new QualitativeOutcome("4", 0.167),
                new QualitativeOutcome("5", 0.167),
                new QualitativeOutcome("6", 0.167)
            };

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            QualitativeSimulation simulation = new QualitativeSimulation(possibleOutcomes);
            return simulation.Simulate(numberOfSimulations);
        }

        public static void QualitativeRegenerationTest()
        {
            //define possible outcomes
            QualitativeOutcome[]
            possibleOutcomes = new QualitativeOutcome[]
            {
                new QualitativeOutcome("1", 0.167),
                new QualitativeOutcome("2", 0.167),
                new QualitativeOutcome("3", 0.167),
                new QualitativeOutcome("4", 0.167),
                new QualitativeOutcome("5", 0.167),
                new QualitativeOutcome("6", 0.167)
            };

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            QualitativeSimulation simulation = new QualitativeSimulation(possibleOutcomes);
            var results = simulation.Simulate(numberOfSimulations);

            // regenerate
            var samenumber = results.Regenerate();
            var newnumber = results.Regenerate(10);
        }

        #endregion

        #region Sensitivity

        public static SensitivitySimulationResults CAPM_Sensitivity()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025 })               
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf)";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return new SensitivitySimulation(simulation).Simulate(numberOfSimulations);
        }

        public static SensitivitySimulationResults CAPM_ExhaustiveSensitivity()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf)";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return new ExhaustiveSensitivitySimulation(simulation).Simulate(numberOfSimulations);
        }

        public static SensitivitySimulationResults CAPM_VeryExhaustiveSensitivity()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),               
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf) + a";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return new ExhaustiveSensitivitySimulation(simulation).Simulate(numberOfSimulations);
        }

        public static void RecomputeSensitivityTest()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf) + a";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            var results =  new ExhaustiveSensitivitySimulation(simulation).Simulate(numberOfSimulations);

            // recompute results
            expression = "Rf + B * (Rm - Rf) + 3 * a";
            var recomputedResults = results.RecomputeExpression(expression);
        }

        public static void SensitivityRegenerationTest()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035))
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf) + a";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            var results = new ExhaustiveSensitivitySimulation(simulation).Simulate(numberOfSimulations);

            // regenerate results
            var samenumber = results.Regenerate();
            var newnumber = results.Regenerate(100);
        }

        public static void SensitivityAddRemoveParameters()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { 1, 2, 3}),
                new DistributionParameter("b", new Normal(1, 1)),
                new DistributionParameter("c", new Normal(2, 1)),
                new PrecomputedParameter("d", new FloatingPoint[] { 10, 20, 30}),
                new DistributionParameter("e", new Normal(4, 1))
            };

            string expression = "a + b + c";
            Simulation simulation = new Simulation(expression, parameters);
            SensitivitySimulation sensitivity = new SensitivitySimulation(simulation);
            var results = sensitivity.Simulate(10);

            // remove b --> should throw an error
            try { results.RemoveParameter(parameters[1]); }
            catch { }

            // remove d --> should not be an issue
            results.RemoveParameter(parameters[4]);

            // add parameter f
            results.AddParameter(new DistributionParameter("f", new Normal(5, 1)));

            // change expression
            results.RecomputeExpression("a + b + c + f");
        }

        public static SensitivitySimulationResults MultithreadedSensitivity()
        {
            // define parameters
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf) + a";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            return new ExhaustiveSensitivitySimulation(simulation).Simulate_Multithreaded(numberOfSimulations);
        }

        public static void MultithreadedSensitivityRegeneration()
        {
            IParameter[] parameters = new IParameter[]
            {
                new PrecomputedParameter("a", new FloatingPoint[] { -0.01, 0.00, 0.01 }),
                new PrecomputedParameter("B", new FloatingPoint[] { 0.5, 1.0, 1.5 }),
                new DistributionParameter("Rm", new Normal(0.08, 0.035)),
                new PrecomputedParameter("Rf", new FloatingPoint[] { 0.015, 0.02, 0.025, 0.050 })
            };

            // create an expression based on the parameters
            string expression = "Rf + B * (Rm - Rf) + a";

            // perform simulations
            int numberOfSimulations = _numberOfSimulations;
            Simulation simulation = new Simulation(expression, parameters);
            var results = new ExhaustiveSensitivitySimulation(simulation).Simulate_Multithreaded(numberOfSimulations);

            // regenerate normal
            results = results.Regenerate();
            results = results.Regenerate_Multithreaded();
        }

        #endregion
    }
}
