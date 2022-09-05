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
using System.Text;

namespace Eval.Core.Util.EARandom
{

    public class DefaultRandomNumberGenerator : Random, IRandomNumberGenerator
    {
        /// <summary>
        /// Initializes a new instance of the class, using a time-dependent default seed value.
        /// </summary>
        public DefaultRandomNumberGenerator()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class, using the specified seed value.
        /// </summary>
        /// <param name="seed"></param>
        public DefaultRandomNumberGenerator(int seed)
            : base(seed)
        {
        }

        public bool NextBool()
        {
            return NextDouble() < 0.5;
        }

        public double NextDouble(double maxValue)
        {
            return NextDouble() * maxValue;
        }

        public double NextDouble(double minValue, double maxValue)
        {
            return NextDouble() * (maxValue - minValue) + minValue;
        }
    }
}
