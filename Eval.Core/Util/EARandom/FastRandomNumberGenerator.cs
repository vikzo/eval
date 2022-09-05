#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Newtonsoft.Json;
using SharpNeatLib.Maths;
using System;
using System.Runtime.CompilerServices;

namespace Eval.Core.Util.EARandom
{

    /// <summary>
    /// An IRandomNumberGenerator using FastRandom has pseudo-number generator.
    /// <see href="http://www.codeproject.com/Articles/9187/A-fast-equivalent-for-System-Random"/>
    /// </summary>
    public class FastRandomNumberGenerator : IRandomNumberGenerator
    {
        [JsonProperty]
        private readonly FastRandom _rng;

        /// <summary>
        /// Initializes a new instance of the class, using a time-dependent default seed value.
        /// </summary>
        public FastRandomNumberGenerator()
        {
            int seed = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _rng = new FastRandom(seed);
        }

        /// <summary>
        /// Initializes a new instance of the class, using the specified seed value.
        /// </summary>
        /// <param name="seed"></param>
        public FastRandomNumberGenerator(int seed)
        {
            _rng = new FastRandom(seed);
        }

        public int Next()
        {
            return _rng.Next(0, int.MaxValue);
        }

        public int Next(int maxValue)
        {
            return _rng.Next(0, maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return _rng.Next(minValue, maxValue);
        }

        public bool NextBool()
        {
            return _rng.NextDouble() < 0.5;
        }

        public void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)_rng.Next(0, 256);
            }
        }

        public double NextDouble(double maxValue)
        {
            return _rng.NextDouble() * maxValue;
        }

        public double NextDouble(double minValue, double maxValue)
        {
            return _rng.NextDouble() * (maxValue - minValue) + minValue;
        }

        public double NextDouble()
        {
            return _rng.NextDouble();
        }
    }
}
