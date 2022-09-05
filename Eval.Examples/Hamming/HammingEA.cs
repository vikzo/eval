#region LICENSE
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
using System.Text;

namespace Eval.Examples
{

    public class StringGenotype : Genotype
    {
        public string str = null!;

        public StringGenotype(string str) : base()
        {
            this.str = str;
        }

        public override IGenotype Clone()
        {
            var geno = new StringGenotype(str);
            return geno;
        }

        public override IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random)
        {
            var newgeno = new StringGenotype(string.Empty);
            var og = (StringGenotype)other;
            switch (crossover)
            {
                case CrossoverType.Uniform:
                    var sb = new StringBuilder();
                    for (int i = 0; i < Math.Min(str.Length, og.str.Length); i++)
                        sb.Append(random.NextBool() ? str[i] : og.str[i]);
                    newgeno.str = sb.ToString();
                    break;

                case CrossoverType.OnePoint:
                    var point = random.Next(Math.Min(str.Length, og.str.Length));
                    newgeno.str = string.Concat(str.AsSpan(0, point), og.str.AsSpan(point));
                    break;

                default: throw new NotImplementedException(crossover.ToString());
            }
            return newgeno;
        }

        public override void Mutate(double probability, IRandomNumberGenerator random)
        {
            if (random.NextDouble() >= probability)
                return;

            var sb = new StringBuilder();
            sb.Append(str);
            if (str.Length == 0)
                sb.Append((char)random.Next(32, 122));
            else
                sb[random.Next(str.Length)] = (char)random.Next(32, 122);

            if (random.NextDouble() < 0.25)
            {
                if (random.NextBool())
                    sb.Append((char)random.Next(32, 122));
                else
                    sb.Remove(sb.Length - 1, 1);
            }

            str = sb.ToString();
        }

        public override string ToString()
        {
            return str;
        }
        
    }

    
    public class HammingPhenotype : Phenotype<StringGenotype>
    {
        public HammingPhenotype(StringGenotype genotype) : base(genotype)
        {
        }

        protected override double CalculateFitness()
        {
            int hammingdist = 2 * Math.Abs(Genotype.str.Length - HammingEA.TARGET.Length);
            for (int i = 0; i < Math.Min(Genotype.str.Length, HammingEA.TARGET.Length); i++)
                hammingdist += Genotype.str[i] == HammingEA.TARGET[i] ? 0 : 1;
            return hammingdist;
        }

        public override string ToString()
        {
            return Genotype.ToString();
        }
    }

    /// <summary>
    /// Optimizes towards a given string. 
    /// This is a minimization problem using hamming distance for fitness function
    /// </summary>
    
    public class HammingEA : EA<HammingPhenotype>
    {
        public static readonly string TARGET = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

        public HammingEA(IEAConfiguration config, IRandomNumberGenerator rng) : base(config, rng)
        {}

        protected override HammingPhenotype CreatePhenotype(IGenotype genotype)
        {
            var phenotype = new HammingPhenotype((StringGenotype)genotype);
            return phenotype;
        }

        protected override HammingPhenotype CreateRandomPhenotype()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < RNG.Next(1, 10); i++)
                sb.Append((char)RNG.Next(32, 122));
            var p = new HammingPhenotype(new StringGenotype(sb.ToString()));
            return p;
        }


        public static void Run()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 100,
                OverproductionFactor = 1.0,
                MaximumGenerations = 100000,
                CrossoverType = CrossoverType.Uniform,
                AdultSelectionType = AdultSelectionType.GenerationalReplacement,
                ParentSelectionType = ParentSelectionType.Tournament,
                CrossoverRate = 0.1,
                MutationRate = 0.99,
                TournamentSize = 10,
                TournamentProbability = 0.77,
                TargetFitness = 0.0,
                Mode = EAMode.MinimizeFitness,
                Elites = 1,
                CalculateStatistics = true
            };

            var hammingEA = new HammingEA(config, new DefaultRandomNumberGenerator());

            var stopwatchtot = new Stopwatch();
            var stopwatchgen = new Stopwatch();

            var currentStats = new PopulationStatistics();

            hammingEA.PopulationStatisticsCalculated += (stats) =>
            {
                currentStats = stats;
            };
            
            hammingEA.NewBestFitnessEvent += (pheno, gen) => {
                double progress = (gen / (double)config.MaximumGenerations) * 100.0;
                var totruntime = stopwatchtot.Elapsed;
                var genruntime = stopwatchgen.Elapsed;
                Console.WriteLine();
                Console.WriteLine(string.Format("G# {0}    best_f: {1:F2}    avg_f: {2:F2}    SD: {3:F2}    Progress: {4,5:F2}    Gen: {5}   Tot: {6}", gen, hammingEA.Best.Fitness, currentStats.AverageFitness, currentStats.StandardDeviationFitness, progress, genruntime, totruntime));
                Console.WriteLine("Generation winner: " + (hammingEA.Best?.Genotype));

                stopwatchgen.Restart();
            };

            hammingEA.TerminationEvent += (r) =>
            {
                if (r == TerminationReason.FitnessLimitReached)
                    Console.WriteLine($"Fitness limit reached: {hammingEA.Best.Fitness}");
            };

            stopwatchtot.Start();
            stopwatchgen.Start();

            var res = hammingEA.Evolve();

            Console.WriteLine("\n\nDone!");
            Console.WriteLine($"Winner: {res.Winner}");
            WriteResultToFile(hammingEA.PopulationStatistics);
            Console.Read();
        }
            
        private static void WriteResultToFile(List<PopulationStatistics> stats)
        {
            using var file = new StreamWriter($"hamming_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv");
            file.WriteLine("MinFitness,MaxFitness,AverageFitness,StandardDeviationFitness,VarianceFitness");
            foreach (var stat in stats)
            {
                file.WriteLine($"{stat.MinFitness},{stat.MaxFitness},{stat.AverageFitness},{stat.StandardDeviationFitness},{stat.VarianceFitness}");
            }
        }
    }
}
