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
using Eval.Core.Config;
using Eval.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class PopulationTests
    {
        private readonly int preferredPopSize = 120;
        private Population population = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            population = new Population(preferredPopSize);
        }

        [TestMethod]
        public void ShouldCreatePopulationOfCorrectSize()
        {
            population.Size.Should().Be(preferredPopSize);
        }

        [TestMethod]
        public void IsFilledFlagShouldNotInitiallyBeSet()
        {
            population.IsFilled.Should().BeFalse();
        }

        [TestMethod]
        public void FillShouldCallFactoryCorrectNumberOfTimes()
        {
            var phenotypeFactoryMock = new Mock<Func<IPhenotype>>();
            phenotypeFactoryMock.Setup(f => f.Invoke()).Returns(new Mock<IPhenotype>().Object);
            population.Fill(phenotypeFactoryMock.Object);
            phenotypeFactoryMock.Verify(factory => factory(), Times.Exactly(preferredPopSize));
        }

        [TestMethod]
        public void FillShouldPopulatePopulationWithCorrectElements()
        {
            var phenotypeMock = new Mock<IPhenotype>();
            population.Fill(() => phenotypeMock.Object);

            foreach (var individual in population)
            {
                individual.Should().NotBeNull();
                individual.Should().BeSameAs(phenotypeMock.Object);
            }
        }

        [TestMethod]
        public void FillShouldSetIsFilledFlag()
        {
            population.Fill(() => new Mock<IPhenotype>().Object);
            population.IsFilled.Should().BeTrue();
        }

        [TestMethod]
        public void EvaluateShouldThrowIfPopulationIsEmpty()
        {
            population.Invoking(p => p.Evaluate(false, e => { }))
                .Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void EvaluateShouldEvaluateAllAndCallEvaluatedEvent()
        {
            var (mocks, _) = FillWithMocks(population);

            var evaluatedEventMock = new Mock<Action<IPhenotype>>();
            population.Evaluate(false, evaluatedEventMock.Object);

            foreach (var mock in mocks)
            {
                mock.Verify(p => p.Evaluate(), Times.Once);
                evaluatedEventMock.Verify(e => e(mock.Object), Times.Once);
            }
        }

        [TestMethod]
        public void EvaluateShouldSkipAlreadyEvaluatedIndividuals()
        {
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.IsEvaluated).Returns(true));

            var evaluatedEventMock = new Mock<Action<IPhenotype>>();
            population.Evaluate(false, evaluatedEventMock.Object);

            foreach (var mock in mocks)
            {
                mock.Verify(p => p.Evaluate(), Times.Never);
                evaluatedEventMock.Verify(e => e(It.IsAny<IPhenotype>()), Times.Never);
            }
        }

        [TestMethod]
        public void EvaluateShouldReevaluateIfConfigured()
        {
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.IsEvaluated).Returns(true));

            var evaluatedEventMock = new Mock<Action<IPhenotype>>();
            population.Evaluate(true, evaluatedEventMock.Object);

            foreach (var mock in mocks)
            {
                mock.Verify(p => p.Evaluate(), Times.Once);
                evaluatedEventMock.Verify(e => e(mock.Object), Times.Once);
            }
        }

        [TestMethod]
        public void SortShouldThrowIfPopulationIsEmpty()
        {
            population.Invoking(p => p.Sort(EAMode.MaximizeFitness))
                .Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void WhenEAModeIsMaximize_SortShouldSortDescending()
        {
            var rng = new Random("this is seed".GetHashCode());
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.Fitness).Returns(rng.NextDouble()));

            population.Sort(EAMode.MaximizeFitness);

            population.Should().BeInDescendingOrder(i => i.Fitness);
        }

        [TestMethod]
        public void WhenEAModeIsMinimize_SortShouldSortAscending()
        {
            var rng = new Random("this is seed".GetHashCode());
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.Fitness).Returns(rng.NextDouble()));

            population.Sort(EAMode.MinimizeFitness);

            population.Should().BeInAscendingOrder(i => i.Fitness);
        }

        private static (IList<Mock<IPhenotype>>, Func<IPhenotype>) FillWithMocks(
            Population population,
            Action<Mock<IPhenotype>>? setups = null)
        {
            var mocks = new List<Mock<IPhenotype>>();
            IPhenotype phenotypeMockFactory()
            {
                var mock = new Mock<IPhenotype>();
                setups?.Invoke(mock);
                mocks.Add(mock);
                return mock.Object;
            }
            population.Fill(phenotypeMockFactory);
            return (mocks, phenotypeMockFactory);
        }

        [TestMethod]
        public void ClearShouldWipePopulation()
        {
            var popsize = 5;
            population = new Population(popsize);
            FillWithMocks(population);
            population.Clear();

            for (int i = 0; i < population.Size; i++)
            {
                Assert.IsNull(population[i]);
            }
        }


        [TestMethod]
        public void ClearWithOneEliteShouldLeavePhenotypeWithMaxFitness()
        {
            var popsize = 3;
            population = new Population(popsize);

            var m0 = new Mock<IPhenotype>();
            m0.SetupGet(p => p.Fitness).Returns(1.0);
            population.Add(m0.Object);

            var m1 = new Mock<IPhenotype>();
            m1.Setup(p => p.Fitness).Returns(0.8);
            population.Add(m1.Object);

            var m2 = new Mock<IPhenotype>();
            m2.Setup(p => p.Fitness).Returns(0.6);
            population.Add(m2.Object);

            population.Evaluate(true, null);
            population.Clear(1, EAMode.MaximizeFitness);

            for (int i = 1; i < population.Size; i++)
            {
                Assert.IsNull(population[i]);
            }
            population[0].Fitness.Should().Be(1.0);
        }

        [TestMethod]
        public void ClearWithOneEliteShouldLeavePhenotypeWithMinFitness()
        {
            var popsize = 3;
            population = new Population(popsize);

            var m0 = new Mock<IPhenotype>();
            m0.SetupGet(p => p.Fitness).Returns(1.0);
            population.Add(m0.Object);

            var m1 = new Mock<IPhenotype>();
            m1.Setup(p => p.Fitness).Returns(0.8);
            population.Add(m1.Object);

            var m2 = new Mock<IPhenotype>();
            m2.Setup(p => p.Fitness).Returns(0.6);
            population.Add(m2.Object);

            population.Evaluate(true, null);
            population.Clear(1, EAMode.MinimizeFitness);

            for (int i = 1; i < population.Size; i++)
            {
                Assert.IsNull(population[i]);
            }
            population[0].Fitness.Should().Be(0.6);
        }

        [TestMethod]
        public void ClearWithMultipleElitesShouldLeavePhenotypesWithMaxFitness()
        {
            var popsize = 5;
            population = new Population(popsize);

            var m0 = new Mock<IPhenotype>();
            m0.SetupGet(p => p.Fitness).Returns(1.0);
            population.Add(m0.Object);

            var m1 = new Mock<IPhenotype>();
            m1.Setup(p => p.Fitness).Returns(0.8);
            population.Add(m1.Object);

            var m2 = new Mock<IPhenotype>();
            m2.Setup(p => p.Fitness).Returns(0.6);
            population.Add(m2.Object);

            var m3 = new Mock<IPhenotype>();
            m3.Setup(p => p.Fitness).Returns(0.4);
            population.Add(m3.Object);

            var m4 = new Mock<IPhenotype>();
            m4.Setup(p => p.Fitness).Returns(0.2);
            population.Add(m4.Object);

            population.Evaluate(true, null);
            population.Clear(2, EAMode.MaximizeFitness);

            for (int i = 2; i < population.Size; i++)
            {
                Assert.IsNull(population[i]);
            }
            population[0].Fitness.Should().Be(1.0);
            population[1].Fitness.Should().Be(0.8);
        }

        [TestMethod]
        public void ClearWithMultipleElitesShouldLeavePhenotypesWithMinFitness()
        {
            var popsize = 4;
            population = new Population(popsize);

            var m0 = new Mock<IPhenotype>();
            m0.SetupGet(p => p.Fitness).Returns(1.0);
            population.Add(m0.Object);

            var m1 = new Mock<IPhenotype>();
            m1.Setup(p => p.Fitness).Returns(0.8);
            population.Add(m1.Object);

            var m2 = new Mock<IPhenotype>();
            m2.Setup(p => p.Fitness).Returns(0.6);
            population.Add(m2.Object);

            var m3 = new Mock<IPhenotype>();
            m3.Setup(p => p.Fitness).Returns(0.4);
            population.Add(m3.Object);

            population.Evaluate(true, null);
            population.Clear(2, EAMode.MinimizeFitness);

            for (int i = 2; i < population.Size; i++)
            {
                Assert.IsNull(population[i]);
            }
            population[0].Fitness.Should().Be(0.4);
            population[1].Fitness.Should().Be(0.6);
        }

        [TestMethod]
        public void TestGetProbabilitySelectorWithMaximizeMode()
        {
            var pMocks = new List<Mock<IPhenotype>>
            {
                CreatePhenotypeMock(1),
                CreatePhenotypeMock(2),
                CreatePhenotypeMock(3),
                CreatePhenotypeMock(5)
            };

            var expected = new List<double>() { 1, 2, 3, 5 };

            population = new Population(4);
            foreach (var mock in pMocks)
            {
                population.Add(mock.Object);
            }

            var probSelector = population.GetProbabilitySelector(EAMode.MaximizeFitness);

            for (int i = 0; i < pMocks.Count; i++)
            {
                probSelector(pMocks[i].Object).Should().Be(expected[i]);
            }
        }

        [TestMethod]
        public void TestGetProbabilitySelectorWithMinimizeMode()
        {
            var pMocks = new List<Mock<IPhenotype>>
            {
                CreatePhenotypeMock(1),
                CreatePhenotypeMock(2),
                CreatePhenotypeMock(3),
                CreatePhenotypeMock(5)
            };

            var expected = new List<double>() { 5, 4, 3, 1 };

            population = new Population(4);
            foreach (var mock in pMocks)
            {
                population.Add(mock.Object);
            }

            var probSelector = population.GetProbabilitySelector(EAMode.MinimizeFitness);

            for (int i = 0; i < pMocks.Count; i++)
            {
                probSelector(pMocks[i].Object).Should().Be(expected[i]);
            }
        }

        [TestMethod]
        public void GetProbabilitySelectorIsNotMutable()
        {
            population = new Population(2)
            {
                CreatePhenotypeMock(1).Object,
                CreatePhenotypeMock(2).Object
            };

            var firstSelector = population.GetProbabilitySelector(EAMode.MinimizeFitness);

            population.Clear();
            population.Add(CreatePhenotypeMock(3).Object);
            population.Add(CreatePhenotypeMock(100).Object);

            var secondSelector = population.GetProbabilitySelector(EAMode.MinimizeFitness);

            firstSelector(CreatePhenotypeMock(2).Object).Should().Be(1);
            firstSelector(CreatePhenotypeMock(1).Object).Should().Be(2);

            secondSelector(CreatePhenotypeMock(100).Object).Should().Be(3);
            secondSelector(CreatePhenotypeMock(3).Object).Should().Be(100);
        }

        [TestMethod]
        public void CallingAddWithNullShouldThrow()
        {
            population.Invoking(pop => pop.Add(null))
                .Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void PopuationEnumerationShouldNotProduceNull()
        {
            void AssertNoNulls()
            {
                foreach (var p in population) p.Should().NotBeNull();
            }

            AssertNoNulls();

            population.Add(CreatePhenotypeMock(1).Object);
            AssertNoNulls();

            population.Fill(() => CreatePhenotypeMock(1).Object);
            AssertNoNulls();

            population.Clear(2, EAMode.MaximizeFitness);
            AssertNoNulls();

            population.Fill(() => CreatePhenotypeMock(1).Object);
            population.Clear(1, EAMode.MaximizeFitness);
            AssertNoNulls();

            population.Clear();
            AssertNoNulls();
        }

        [TestMethod]
        public void PopulationShouldNotBeInitiallySorted()
        {
            population.IsSorted.Should().BeFalse();
        }

        [TestMethod]
        public void PopulationShouldBeSortedAfterCallingSort()
        {
            population.Fill(() => CreatePhenotypeMock(1).Object);
            population.Sort(EAMode.MaximizeFitness);
            population.IsSorted.Should().BeTrue();
        }

        [TestMethod]
        public void PopulationShouldNotBeSortedAfterClear()
        {
            void Setup()
            {
                population.Fill(() => CreatePhenotypeMock(1).Object);
                population.Sort(EAMode.MaximizeFitness);
                population.IsSorted.Should().BeTrue();
            }

            Setup();
            population.Clear();
            population.IsSorted.Should().BeFalse();

            Setup();
            population.Clear(1, EAMode.MaximizeFitness);
            population.IsSorted.Should().BeFalse();

            Setup();
            population.Clear(2, EAMode.MaximizeFitness);
            population.IsSorted.Should().BeFalse();
        }

        [TestMethod]
        public void PopulationShouldBeSortedAfterAdd()
        {
            population.Add(CreatePhenotypeMock(1).Object);
            population.IsSorted.Should().BeFalse();
        }

        private static Mock<IPhenotype> CreatePhenotypeMock(double fitnessSetup)
        {
            var pmock = new Mock<IPhenotype>();
            pmock.SetupGet(p => p.Fitness).Returns(fitnessSetup);
            pmock.SetupGet(p => p.IsEvaluated).Returns(true);
            return pmock;
        }

        [TestMethod]
        public void PopulationStatisticsTest()
        {
            population = new Population(5);
            for (int i = 0; i < 5; i++)
            {
                population.Add(CreatePhenotypeMock(i).Object);
            }

            var stats = population.CalculatePopulationStatistics();
            stats.MaxFitness.Should().Be(4.0);
            stats.MinFitness.Should().Be(0.0);
            stats.AverageFitness.Should().Be(2.0);
            stats.VarianceFitness.Should().Be(2.0);
            stats.StandardDeviationFitness.Should().BeApproximately(1.4142135623, 1e-10);
        }
    }

}
