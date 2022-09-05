﻿#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class ArrayGenotypeTests : AbstractListGenotypeTestBase<ArrayGenotype<TestGenoElement>, TestGenoElement[], TestGenoElement>
    {
        protected override ArrayGenotype<TestGenoElement> GetFirstGenotype(Func<Func<TestGenoElement>, IEnumerable<TestGenoElement>> elementFactory)
        {
            return new ArrayGenotype<TestGenoElement>(elementFactory(() => new TestGenoElement(1)).ToArray()); // All 1
        }

        protected override ArrayGenotype<TestGenoElement> GetSecondGenotype(Func<Func<TestGenoElement>, IEnumerable<TestGenoElement>> elementFactory)
        {
            return new ArrayGenotype<TestGenoElement>(elementFactory(() => new TestGenoElement(2)).ToArray()); // All 2
        }
    }

    [TestClass]
    public class ArrayGenotypeBaseTests : GenotypeTestBase<ArrayGenotype<TestGenoElement>>
    {
        protected override ArrayGenotype<TestGenoElement> CreateGenotype => new(Repeat(() => new TestGenoElement(0), 10).ToArray());
    }

    public class TestGenoElement : IGenotypeElement
    {
        public double Value { get; set; }

        public TestGenoElement(double value)
        {
            Value = value;
        }

        public object Clone()
        {
            return new TestGenoElement(Value);
        }

        public void Mutate(double factor, IRandomNumberGenerator random)
        {
            Value += factor * random.NextDouble(-1, 1);
        }


        public override bool Equals(object? obj)
        {
            return obj is TestGenoElement other && Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
}
