using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Util
{
    public static class MathUtil
    {
        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
