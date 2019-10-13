using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public interface IGenotype
    {
        IGenotype Clone();
        IGenotype CrossoverWith(IGenotype other, Crossover crossover);
        void Mutate(double factor);
    }
}
