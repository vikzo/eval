using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.Test.Unit.Util
{
    [TestClass]
    public class FastRandomTests
    {

        [TestMethod]
        public void TestRandomDistribution()
        {
            var rng = new FastRandomNumberGenerator();
            var counts = new List<int>(new int[10]);
            var rounds = 1_000_000;
            for (int i = 0; i < rounds; i++)
            {
                var index = rng.Next(10);
                counts[index]++;
            }

            for (int i = 0; i < counts.Count; i++)
            {
                var rate = counts[i] / (double)rounds;
                rate.Should().BeApproximately(0.1, 0.001);
            }
        }
    }
}
