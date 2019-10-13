using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eval.Core.Models
{
    public class Population : IEnumerable<IPhenotype>
    {
        public int Size { get { return population.Length; } }
        public bool IsFilled { get; private set; }

        private readonly IPhenotype[] population;

        public Population(int size)
        {
            population = new IPhenotype[size];
        }

        public IPhenotype this[int key]
        {
            get => population[key];
        }

        public void Fill(Func<IPhenotype> phenotypeFactory)
        {
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = phenotypeFactory();
            }
            IsFilled = true;
        }

        public void Evaluate(bool reevaluate, Action<IPhenotype> phenotypeEvaluatedEvent)
        {
            ThrowIfNotFilled();
            foreach (var individual in population.Where(i => !i.IsEvaluated || reevaluate))
            {
                individual.Evaluate();
                phenotypeEvaluatedEvent(individual);
            }
        }

        public void Sort(EAMode mode)
        {
            ThrowIfNotFilled();
            switch (mode)
            {
                case EAMode.MaximizeFitness:
                    population.OrderByDescending(i => i.Fitness);
                    break;

                case EAMode.MinimizeFitness:
                    population.OrderBy(i => i.Fitness);
                    break;

                default:
                    throw new NotImplementedException($"EA mode {mode} is not implemented");
            }
        }

        private void ThrowIfNotFilled()
        {
            if (!IsFilled)
            {
                throw new InvalidOperationException("The population is not filled");
            }
        }

        public IEnumerator<IPhenotype> GetEnumerator()
        {
            foreach (var individual in population)
            {
                yield return individual;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}
