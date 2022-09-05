using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public class PrimitiveTypeListGenotype<T> : ListGenotype<T>
    {
        private Func<T> _generator;

        public PrimitiveTypeListGenotype(Func<T> randomGeneGenerator, int count)
        {
            _generator = randomGeneGenerator;
            Elements = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                Elements.Add(_generator.Invoke());
            }
        }

        public PrimitiveTypeListGenotype(List<T> elements, Func<T> randomGeneGenerator)
        {
            _generator = randomGeneGenerator;
            Elements = elements;
        }

        protected override T CloneElement(T element)
        {
            return element;
        }

        protected override ListGenotype<T> CreateNewGenotype(List<T> elements)
        {
            return new PrimitiveTypeListGenotype<T>(elements, _generator);
        }

        protected override T MutateElement(T element, double factor, IRandomNumberGenerator random)
        {
            if (random.NextDouble() < factor)
            {
                return _generator.Invoke();
            }
            return element;
        }
    }
}
