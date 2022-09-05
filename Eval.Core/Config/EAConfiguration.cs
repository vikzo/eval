#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.IO;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Newtonsoft.Json;

namespace Eval.Core.Config
{
    
    public class EAConfiguration : IEAConfiguration
    {
        public int PopulationSize { get; set; }
        public double TargetFitness { get; set; }
        public EAMode Mode { get; set; } = EAMode.MaximizeFitness;
        public double OverproductionFactor { get; set; } = 1.0; 
        public int MaximumGenerations { get; set; } = int.MaxValue;
        public CrossoverType CrossoverType { get; set; } = CrossoverType.Uniform;
        public AdultSelectionType AdultSelectionType { get; set; } = AdultSelectionType.GenerationalReplacement;
        public ParentSelectionType ParentSelectionType { get; set; } = ParentSelectionType.Tournament;
        public MutationStrategy MutationStrategy { get; set; } = MutationStrategy.ConstantProbability;
        public double CrossoverRate { get; set; } = 0.5;
        public double MutationRate { get; set; } = 0.5;
        public int TournamentSize { get; set; } = 2;
        public double TournamentProbability { get; set; } = 1.0;
        public int Elites { get; set; } = 1;
        public bool ReevaluateElites { get; set; } = false;
        public double RankSelectionMinProbability { get; set; } = 0.5;
        public double RankSelectionMaxProbability { get; set; } = 1.5;
        public bool CalculateStatistics { get; set; } = false;
        public bool MultiThreaded { get; set; } = false;
        public TimeSpan? MaxDuration { get; set; }


        public static EAConfiguration ReadConfigurationFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return ParseFromJsonString(json);
        }

        public static EAConfiguration ParseFromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<EAConfiguration>(json);
        }

    }
}
