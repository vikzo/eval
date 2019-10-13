using System;
using System.Collections.Generic;
using Eval.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;

namespace Eval.Test.Unit.Util
{
    class RouletteEntry
    {
        public int N { get; set; }
        public RouletteEntry(int n)
        {
            N = n;
        }
    }


    [TestClass]
    public class RouletteTests
    {

        [TestMethod]
        public void TestRouletteProbabilitiesWithoutRemoval()
        {
            var roulette = new Roulette<RouletteEntry>(new DefaultRandomNumberGenerator());

            roulette.Add(new RouletteEntry(0), 0.5);
            roulette.Add(new RouletteEntry(1), 0.25);
            roulette.Add(new RouletteEntry(2), 0.25);

            var count = new int[3];

            for (int i = 0; i < 1000000; i++)
            {
                var e = roulette.Spin(false);
                count[e.N]++;
            }

            int entry0 = Convert.ToInt32(count[0] / 10000.0);
            int entry1 = Convert.ToInt32(count[1] / 10000.0);
            int entry2 = Convert.ToInt32(count[2] / 10000.0);

            entry0.Should().BeCloseTo(50, 1);
            entry1.Should().BeCloseTo(25, 1);
            entry2.Should().BeCloseTo(25, 1);
        }

        [TestMethod]
        public void TestRouletteProbabilitiesWithRemoval()
        {
            var count = new int[3];
            var count2 = new int[3];

            for (int i = 0; i < 1000000; i++)
            {
                var roulette = new Roulette<RouletteEntry>(new DefaultRandomNumberGenerator());

                roulette.Add(new RouletteEntry(0), 0.99998);
                roulette.Add(new RouletteEntry(1), 0.00001);
                roulette.Add(new RouletteEntry(2), 0.00001);


                var e1 = roulette.Spin(true);
                count[e1.N]++;
                var e2 = roulette.Spin(true);
                count2[e2.N]++;
            }

            int entry0 = Convert.ToInt32(count[0] / 10000.0);
            int entry1 = Convert.ToInt32(count[1] / 10000.0);
            int entry2 = Convert.ToInt32(count[2] / 10000.0);

            int entry20 = Convert.ToInt32(count2[0] / 10000.0);
            int entry21 = Convert.ToInt32(count2[1] / 10000.0);
            int entry22 = Convert.ToInt32(count2[2] / 10000.0);

            entry0.Should().BeCloseTo(100, 1);
            entry1.Should().BeCloseTo(0, 1);
            entry2.Should().BeCloseTo(0, 1);

            entry20.Should().BeCloseTo(0, 2);
            entry21.Should().BeCloseTo(50, 2);
            entry22.Should().BeCloseTo(50, 2);
        }

        [TestMethod]
        public void TestRouletteShouldThrowExceptionOnEmpty()
        {
            var roulette = new Roulette<RouletteEntry>(new DefaultRandomNumberGenerator());

            roulette.Add(new RouletteEntry(0), 1);

            roulette.Invoking(r => r.Spin(true)).Should().NotThrow();
            roulette.Invoking(r => r.Spin(true)).Should().Throw<InvalidOperationException>();
        }
    }
}
