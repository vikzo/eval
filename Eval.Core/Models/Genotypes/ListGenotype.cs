#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;

namespace Eval.Core.Models
{

    public abstract class ListGenotype<T> : Genotype
    {
        public List<T> Elements { get; set; }

        public int Count => Elements.Count;

        protected ListGenotype()
        {
            Elements = new List<T>();
        }

        protected ListGenotype(int length)
        {
            Elements = new List<T>(length);
        }

        protected ListGenotype(List<T> elements)
        {
            Elements = elements;
        }

        public override IGenotype Clone()
        {
            var newElements = new List<T>(Count);
            for (int i = 0; i < Count; i++)
            {
                newElements[i] = CloneElement(Elements[i]);
            }
            return CreateNewGenotype(newElements);
        }

        public override void Mutate(double probability, IRandomNumberGenerator random)
        {
            for (int i = 0; i < Count; i++)
            {
                Elements[i] = MutateElement(Elements[i], probability, random);
            }
        }

        public override IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random)
        {
            switch (crossover)
            {
                case CrossoverType.OnePoint: return OnePointCrossover((ListGenotype<T>)other, random);
                case CrossoverType.Uniform: return UniformCrossover((ListGenotype<T>)other, random);
                default:
                    throw new NotImplementedException($"Crossover {crossover} is not implemented in ArrayGenotype");
            }
        }

        protected virtual ListGenotype<T> OnePointCrossover(ListGenotype<T> other, IRandomNumberGenerator random)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException("ArrayGenotypes differs in length"); // TODO: support variable length AbstractListGenotype
            }

            var length = Count;
            var splitIndex = random.Next(length + 1);
            var newElements = new List<T>(length);

            for (int i = 0; i < splitIndex; i++)
            {
                newElements.Add(CloneElement(Elements[i]));
            }
            for (int i = splitIndex; i < length; i++)
            {
                newElements.Add(CloneElement(other.Elements[i]));
            }

            return CreateNewGenotype(newElements);
        }

        protected virtual ListGenotype<T> UniformCrossover(ListGenotype<T> other, IRandomNumberGenerator random)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException("ArrayGenotypes differs in length");
            }

            var length = Count;
            var newElements = new List<T>(length);

            for (int i = 0; i < length; i++)
            {
                newElements.Add(random.NextBool() ? CloneElement(Elements[i]) : CloneElement(other.Elements[i]));
            }

            return CreateNewGenotype(newElements);
        }

        /// <summary>
        /// Create a new genotype using the specified elements.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        protected abstract ListGenotype<T> CreateNewGenotype(List<T> elements);

        /// <summary>
        /// Mutates the specified element, where the provided factor describes the degree of mutation.
        /// The element may be mutated in-place and a reference to itself returned, or a mutated copy may be returned.
        /// </summary>
        /// <param name="element">The element to mutate</param>
        /// <param name="factor">A non-negative factor that determines the degree of mutation</param>
        /// <returns>The mutated element</returns>
        protected abstract T MutateElement(T element, double factor, IRandomNumberGenerator random);

        /// <summary>
        /// Returns a deep-clone of the specified element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected abstract T CloneElement(T element);

    }
}
