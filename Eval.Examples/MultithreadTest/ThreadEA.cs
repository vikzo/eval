﻿#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System.Diagnostics;

// This is just a test. delete it at some point
namespace Eval.Examples.MultithreadTest
{
    public class ThreadPhenotype : Phenotype
    {
        public ThreadPhenotype(BinaryGenotype genotype)
            : base(genotype)
        {
        }

        protected override double CalculateFitness()
        {
            var fakeRuntime = TimeSpan.FromMilliseconds(160);
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < fakeRuntime) { }
            stopwatch.Stop();
            return 1;
        }
    }

    public class ThreadEA : EA
    {
        public ThreadEA(IEAConfiguration configuration, IRandomNumberGenerator rng)
            : base(configuration, rng)
        {
        }

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            return new ThreadPhenotype((BinaryGenotype)genotype);
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            var geno = new BinaryGenotype(8);
            geno.Mutate(1, RNG);
            return CreatePhenotype(geno);
        }

        public static void Run()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 1000,
                OverproductionFactor = 1.0,
                MaximumGenerations = 10000,
                CrossoverType = CrossoverType.Uniform,
                AdultSelectionType = AdultSelectionType.GenerationalReplacement,
                ParentSelectionType = ParentSelectionType.Tournament,
                CrossoverRate = 0.8,
                MutationRate = 0.2,
                TournamentSize = 10,
                TournamentProbability = 0.8,
                TargetFitness = 1000.0,
                Mode = EAMode.MaximizeFitness,
                Elites = 0,
                CalculateStatistics = true,
                ReevaluateElites = false,
                MultiThreaded = true
            };
            var ea = new ThreadEA(config, new DefaultRandomNumberGenerator());
            ea.NewGenerationEvent += gen => Console.WriteLine($"Generation {gen}");
            ea.NewBestFitnessEvent += (best, gen) => Console.WriteLine($"New best at generation {gen}. Fitness = {best.Fitness}");
            ea.Evolve();
        }
    }
}
