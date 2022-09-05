﻿#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Util;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Util
{
    [TestClass]
    public class BitArrayListTests
    {
        [TestMethod]
        public void EqualsShouldCompareValue()
        {
            var a = new BitArrayList(10);
            var b = new BitArrayList(10);
            a.Equals(b).Should().BeTrue();
            a[3] = true;
            a.Equals(b).Should().BeFalse();
            b[3] = true;
            a.Equals(b).Should().BeTrue();
        }
    }
}
