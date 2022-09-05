#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Eval.Core
{
    public abstract class EA<TPhenotype> : EA where TPhenotype : class, IPhenotype
    {
        public new TPhenotype Best => (TPhenotype)base.Best;
        public new TPhenotype GenerationalBest => (TPhenotype)base.GenerationalBest;

        public EA(IEAConfiguration configuration, IRandomNumberGenerator rng) 
            : base(configuration, rng)
        {
        }
    }

    public abstract class EA
    {
        public event Action<int> NewGenerationEvent;
        public event Action<IPhenotype, int> NewBestFitnessEvent;
        public event Action<TerminationReason> TerminationEvent;
        public event Action<IPhenotype> PhenotypeEvaluatedEvent;
        public event Action<PopulationStatistics> PopulationStatisticsCalculated;

        public IEAConfiguration EAConfiguration { get; set; }
        public bool IsRunning { get; private set; }
        public bool IsStarted { get; private set; }
        public int Generation { get; private set; }
        public TimeSpan GetDuration => _stopwatch.Elapsed;

        public IPhenotype GenerationalBest { get; private set; }
        public IPhenotype Best { get; private set; }
        public bool AbortRequested { get; private set; }

        protected IParentSelection ParentSelection;
        protected IAdultSelection AdultSelection;
        protected IRandomNumberGenerator RNG;
        protected List<PopulationStatistics> PopulationStatistics { get; private set; }
        protected List<IPhenotype> Elites;
        protected Population Population { get; private set; }
        private Population _offspring;
        private int _offspringSize;

        [NonSerialized]
        private Stopwatch _stopwatch;
        private TimeSpan _runtime;
        public TimeSpan Runtime 
        {
            get 
            {
                return _runtime + _stopwatch.Elapsed;
            }
            private set 
            {
                _runtime = value; 
            } 
        }

        public EA(
            IEAConfiguration configuration, 
            IRandomNumberGenerator rng = null)
        {
            EAConfiguration = configuration;
            RNG = rng ?? new DefaultRandomNumberGenerator();

            PopulationStatistics = new List<PopulationStatistics>(512);
            Elites = new List<IPhenotype>(Math.Max(EAConfiguration.Elites, 0));
            AdultSelection = CreateAdultSelection();
            ParentSelection = CreateParentSelection();
        }

        public EA(
            IEAConfiguration configuration, 
            IAdultSelection adultSelection,
            IParentSelection parentSelection,
            Population population,
            IRandomNumberGenerator rng = null)
        {
            EAConfiguration = configuration;
            AdultSelection = adultSelection;
            ParentSelection = parentSelection;
            Population = population;
            RNG = rng ?? new DefaultRandomNumberGenerator();
        }

        protected abstract IPhenotype CreateRandomPhenotype();

        protected abstract IPhenotype CreatePhenotype(IGenotype genotype);

        /// <summary>
        /// Creates a new population filled with phenotypes from CreateRandomPhenotype method.
        /// Override this method to seed the EA with a specific population.
        /// </summary>
        /// <param name="populationSize"></param>
        /// <returns></returns>
        protected virtual Population CreateInitialPopulation(int populationSize)
        {
            var population = new Population(populationSize);
            population.Fill(CreateRandomPhenotype);
            return population;
        }

        protected virtual IAdultSelection CreateAdultSelection()
        {
            return EAConfiguration.AdultSelectionType switch
            {
                AdultSelectionType.GenerationalMixing => new GenerationalMixingAdultSelection(RNG),
                AdultSelectionType.GenerationalReplacement => new GenerationalReplacementAdultSelection(),
                AdultSelectionType.Overproduction => new OverproductionAdultSelection(RNG),
                _ => throw new NotImplementedException($"AdultSelectionType {EAConfiguration.AdultSelectionType}"),
            };
        }

        protected virtual IParentSelection CreateParentSelection()
        {
            return EAConfiguration.ParentSelectionType switch
            {
                ParentSelectionType.FitnessProportionate => new ProportionateParentSelection(),
                ParentSelectionType.Rank => new RankParentSelection(),
                ParentSelectionType.SigmaScaling => new SigmaScalingParentSelection(),
                ParentSelectionType.Tournament => new TournamentParentSelection(EAConfiguration),
                _ => throw new NotImplementedException($"ParentSelectionType {EAConfiguration.ParentSelectionType}"),
            };
        }

        public virtual EAResult Evolve()
        {
            _stopwatch = Stopwatch.StartNew();
            IsRunning = true;
            IsStarted = true;

            if (Population == null || !Population.IsFilled) 
            { 
                Population = CreateInitialPopulation(EAConfiguration.PopulationSize);
            }
            _offspringSize = (int)(EAConfiguration.PopulationSize * Math.Max(EAConfiguration.OverproductionFactor, 1));
            _offspring = new Population(_offspringSize);

            CalculateFitnesses(Population);
            Generation = 1;

            Best = null;

            while (true)
            {
                Population.Sort(EAConfiguration.Mode);
                var generationBest = Population.First();

                if (IsBetterThan(generationBest, Best))
                {
                    Best = generationBest;
                    NewBestFitnessEvent?.Invoke(Best, Generation);
                }
                
                CalculateStatistics(Population);

                NewGenerationEvent?.Invoke(Generation);

                if (!RunCondition(Generation))
                {
                    break;
                }
                
                Elites.Clear();
                for (int i = 0; i < EAConfiguration.Elites; i++)
                {
                    Elites.Add(Population[i]);
                }
                
                var parents = ParentSelection.SelectParents(Population, _offspringSize - Elites.Count, EAConfiguration.Mode, RNG);
                
                _offspring.Clear();
                foreach (var couple in parents)
                {
                    IGenotype geno1 = couple.Item1.Genotype;
                    IGenotype geno2 = couple.Item2.Genotype;

                    IGenotype newgeno;
                    if (RNG.NextDouble() < EAConfiguration.CrossoverRate)
                        newgeno = geno1.CrossoverWith(geno2, EAConfiguration.CrossoverType, RNG);
                    else
                        newgeno = geno1.Clone();

                    newgeno.Mutate(EAConfiguration.MutationRate, RNG);
                    var child = CreatePhenotype(newgeno);
                    _offspring.Add(child);
                }
                
                CalculateFitnesses(_offspring);
                
                AdultSelection.SelectAdults(_offspring, Population, EAConfiguration.PopulationSize - Elites.Count, EAConfiguration.Mode);
                
                for (int i = 0; i < EAConfiguration.Elites; i++)
                {
                    Population.Add(Elites[i]);
                }

                Generation++;
            }

            IsRunning = false;
            _stopwatch.Stop();
            return new EAResult
            {
                Winner = Best,
                EndPopulation = Population
            };
        }

        public void Abort()
        {
            AbortRequested = true;
        }

        private bool IsBetterThan(IPhenotype subject, IPhenotype comparedTo)
        {
            if (comparedTo == null)
                return true;

            return EAConfiguration.Mode switch
            {
                EAMode.MaximizeFitness => subject.Fitness > comparedTo.Fitness,
                EAMode.MinimizeFitness => subject.Fitness < comparedTo.Fitness,
                _ => throw new NotImplementedException($"IsBetterThan not implemented for EA mode {EAConfiguration.Mode}"),
            };
        }

        protected virtual bool RunCondition(int generation)
        {
            if (AbortRequested)
            {
                TerminationEvent?.Invoke(TerminationReason.Aborted);
                return false;
            }
            if (EAConfiguration.MaxDuration != null && GetDuration > EAConfiguration.MaxDuration)
            {
                TerminationEvent?.Invoke(TerminationReason.DurationLimitReached);
                return false;
            }
            if (generation >= EAConfiguration.MaximumGenerations)
            {
                TerminationEvent?.Invoke(TerminationReason.GenerationLimitReached);
                return false;
            }
            if ((EAConfiguration.Mode == EAMode.MaximizeFitness && Best.Fitness >= EAConfiguration.TargetFitness) ||
                (EAConfiguration.Mode == EAMode.MinimizeFitness && Best.Fitness <= EAConfiguration.TargetFitness))
            {
                TerminationEvent?.Invoke(TerminationReason.FitnessLimitReached);
                return false;
            }
            return true;
        }

        

        protected virtual void CalculateStatistics(Population population)
        {
            if (!EAConfiguration.CalculateStatistics)
                return;

            var popstats = population.CalculatePopulationStatistics();
            PopulationStatisticsCalculated?.Invoke(popstats);
            PopulationStatistics.Add(popstats);
        }

        protected virtual void CalculateFitnesses(Population population)
        {
            if (!EAConfiguration.MultiThreaded)
            {
                foreach (var p in population)
                {
                    p.Evaluate();
                    PhenotypeEvaluatedEvent?.Invoke((IPhenotype)p);
                }
            }
            else
            {
                using var countdownEvent = new CountdownEvent(population.Count);
                foreach (var p in population)
                {
                    ThreadPool.QueueUserWorkItem(FitnessWorker, new object[] { p, countdownEvent });
                }
                countdownEvent.Wait();
            }
        }

        private void FitnessWorker(object state)
        {
            var input = state as object[];
            var pheno = input[0] as IPhenotype;
            var countdownEvent = input[1] as CountdownEvent;

            pheno.Evaluate();
            PhenotypeEvaluatedEvent?.Invoke(pheno);

            countdownEvent.Signal();
        }

        public virtual void AssertConfigurationCompatibility(IEAConfiguration other)
        {
            if (EAConfiguration.PopulationSize != other.PopulationSize)
                throw new ArgumentException("Population size cannot differ between snapshots. In snapshot: "+other.PopulationSize);
        }
    }

}
