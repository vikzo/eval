using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Util.EARandom
{
    public interface IRandomNumberGenerator
    {
        double NextDouble();

        int Next();
        int Next(int maxValue);
        int Next(int minValue, int maxValue);

        void NextBytes(byte[] buffer);
    }
}
