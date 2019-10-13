namespace Eval.Core.Models
{
    public interface IPhenotype
    {
        bool IsEvaluated { get; }
        Genotype Genotype { get; }
        double Fitness { get; }
        double Evaluate();
    }
}
