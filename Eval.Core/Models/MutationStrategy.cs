using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public enum MutationStrategy
    {
        /// <summary>
        /// Probability of mutation of each individual is set to <c>MutationRate</c>
        /// </summary>
        ConstantProbability,

        /// <summary>
        /// Rank Based Adaptive Mutation Probability.
        /// Formula:
        /// p = pMax * (1 - (r-1)/(N-1))
        /// where 
        /// p = mutation probability of genotype,
        /// pMax = maximum mutation probability set to <c>MutationRate</c>
        /// r = rank of phenotype
        /// N = population size
        /// </summary>
        RankBasedProbability,
    }
}
