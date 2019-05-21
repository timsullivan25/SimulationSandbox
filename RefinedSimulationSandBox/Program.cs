using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefinedSimulationSandBox
{
    class Program
    {
        static void Main(string[] args)
        {
            //// showing basic expressions
            //var qualitativeDiceRoll = Testing.DiceRoll();
            //var doubleDiceRoll = Testing.DoubleDiceRoll();
            //var twentySided = Testing.TwentySidedDie();
            //var twoTwentySided = Testing.TwoTwentySidedDie();

            //// proof that simulation inside of a simulation works properly
            //var regularTenSidedDieRoll = Testing.RegularTenSidedDie();
            //var scaledTenSidedDieRoll = Testing.ScaledTenSidedDie();

            //// simulation CAPM with B and Rm estimates, RF constant
            //var capm = Testing.CAPM();

            //// conditional parameters
            //var conditionalTest = Testing.ConditionalTest();
            //var conditionalIntegerTest = Testing.ConditionalIntegerTest();

            //// precomputed parameters
            ////var precomputedValidationErrorTest = Testing.PrecomputedValidationErrorTest();
            //var precomputedTest = Testing.PrecomputedTest();

            //// sensitivity simulation
            //var sensitivitySimulation = Testing.CAPM_Sensitivity();
            //var exhaustiveSensitivitySimulation = Testing.CAPM_ExhaustiveSensitivity();
            //var veryExhaustiveSensitivitySimulation = Testing.CAPM_VeryExhaustiveSensitivity(); // incorporates alpha value

            //// recompute test
            //Testing.RecomputeTest();
            //Testing.RecomputeSensitivityTest();

            //// regeneration test
            //Testing.RegenerationTest();
            //Testing.QualitativeRegenerationTest();
            //Testing.SensitivityRegenerationTest();

            //// add / remove parameters
            //Testing.AddAndRemoveParameters();
            //Testing.SensitivityAddRemoveParameters();

            // compare single to multi thread sensitivity -- these are the same functions without and with multithreading
            //var slow = Testing.CAPM_VeryExhaustiveSensitivity(); 
            //var fast = Testing.MultithreadedSensitivity();
            Testing.MultithreadedSensitivityRegeneration();
        }
    }
}
