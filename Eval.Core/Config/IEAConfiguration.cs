using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;

namespace Eval.Core.Config
{
    public interface IEAConfiguration
    {
        // TODO: doc
        int PopulationSize { get; }
        double OverproductionFactor { get; }
        int MaximumGenerations { get; }
        Crossover Crossover { get; }
        AdultSelection AdultSelection { get; }
        ParentSelection ParentSelection { get; }
        double CrossoverRate { get; }
        double MutationRate { get; }
        int TournamentSize { get; }
        double TournamentProbability { get; }
        double TargetFitness { get; }
        EAMode Mode { get; }
        int Elites { get; }
        bool ReevaluateElites { get; }
    }
}
