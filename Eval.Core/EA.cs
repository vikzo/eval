using Eval.Core.Config;
using Eval.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eval.Core
{
    public abstract class EA
    {
        public IEAConfiguration EAConfiguration { get; set; }

        public event Action<int> NewGenerationEvent;
        public event Action<IPhenotype> NewBestFitnessEvent;
        public event Action<double> FitnessLimitReachedEvent;
        public event Action<int> GenerationLimitReachedEvent;
        public event Action<IPhenotype> PhenotypeEvaluatedEvent;
        public event Action AbortedEvent;

        public EA(IEAConfiguration configuration)
        {
            EAConfiguration = configuration;
        }

        public void Evolve()
        {
            var population = new Population(EAConfiguration.PopulationSize);

            population.Fill(CreateRandomPhenotype);
            population.Evaluate(EAConfiguration.ReevaluateElites, PhenotypeEvaluatedEvent);

            var generation = 0;
            while (true)
            {
                NewGenerationEvent(generation);

                generation++;
            }

        }

        protected abstract IPhenotype CreateRandomPhenotype();

    }

}
