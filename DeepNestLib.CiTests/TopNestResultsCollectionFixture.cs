namespace DeepNestLib.CiTests
{
  using System;
  using System.Linq;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class TopNestResultsCollectionFixture
  {
    [Fact]
    public void GivenEmptyCollectionWhenMaxCapacityZeroThenThrow()
    {
      var sut = A.Dummy<TopNestResultsCollection>();
      Action act = () => sut.TryAdd(A.Fake<INestResult>());

      act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GivenEmptyCollectionWhenAddResultThenReturnWasAdded()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      sut.TryAdd(A.Fake<INestResult>()).Should().Be(TryAddResult.Added);
    }

    [Fact]
    public void GivenEmptyCollectionWhenAddResultThenCountIncrement()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      sut.TryAdd(A.Fake<INestResult>());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenEmptyCollectionWhenAddResultThenMemoise()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      var expected = A.Fake<INestResult>();
      sut.TryAdd(expected);

      sut.Single().Should().Be(expected);
    }

    [Fact]
    public void MaxCapacityShouldBeGreaterThanOrEqualToEliteSurvivors()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      sut.MaxCapacity.Should().BeGreaterOrEqualTo(sut.EliteSurvivors);
    }

    [Fact]
    public void GivenHigherFitnessWhenAddResultThenMemoiseAfterExisting()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      var first = A.Fake<INestResult>();
      sut.TryAdd(first);
      A.CallTo(() => first.Fitness).Returns(1);
      sut.First().Should().Be(first);

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(2);
      sut.TryAdd(second);

      sut.Skip(1).First().Should().Be(second);
    }

    [Fact]
    public void GivenLowerFitnessWhenAddResultThenMemoiseBeforeExisting()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      var first = A.Fake<INestResult>();
      sut.TryAdd(first);
      A.CallTo(() => first.Fitness).Returns(2);
      sut.First().Should().Be(first);

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(1);
      sut.TryAdd(second);

      sut.First().Should().Be(second);
    }

    [Fact]
    public void GivenMarginallyDifferentTopWhenComparedThenIsANovelNest()
    {
      TopNestResultsCollection.IsANovelNest(1.000, 1.01, 0, TestSvgNestConfig.Default.TopDiversity).Should().BeTrue();
    }

    [Fact]
    public void GivenMarginallySameTopWhenComparedThenNotIsANovelNest()
    {
      TopNestResultsCollection.IsANovelNest(1.000, 1.001, 0, TestSvgNestConfig.Default.TopDiversity).Should().BeFalse();
    }

    [Fact]
    public void GivenMarginallyDifferentMidWhenComparedThenIsANovelNest()
    {
      TopNestResultsCollection.IsANovelNest(10000.000, 10000 * (1 + TestSvgNestConfig.Default.TopDiversity) + 1, 10, TestSvgNestConfig.Default.TopDiversity).Should().BeTrue();
    }

    [Fact]
    public void GivenMarginallySameMidWhenComparedThenNotIsANovelNest()
    {
      TopNestResultsCollection.IsANovelNest(10000.000, 10000 * (1 + TestSvgNestConfig.Default.TopDiversity), 10, TestSvgNestConfig.Default.TopDiversity).Should().BeFalse();
    }

    [Fact]
    public void GivenMarginallySameFitnessWhenAddResultThenReportAddedButDont()
    {
      var sut = new TopNestResultsCollection(new TestSvgNestConfig(), A.Fake<IDispatcherService>());
      FillToMaxCapacity(sut);

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(sut.Skip(1).First().Fitness);
      sut.TryAdd(second).Should().Be(TryAddResult.Duplicate);

      sut.Should().NotContain(second, "it was considered a duplicate.");
    }

    [Fact]
    public void GivenPopulationFullWhenAddTopResultThenNoCountIncrement()
    {
      var random = new Random();
      var config = new TestSvgNestConfig();
      var sut = new TopNestResultsCollection(config, A.Fake<IDispatcherService>());
      FillToMaxCapacity(sut);

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(0);
      sut.TryAdd(second).Should().Be(TryAddResult.Added, "gets added and the least fit is eliminated.");

      sut.First().Should().Be(second);
      sut.Count.Should().Be(sut.MaxCapacity);
    }

    private static void FillToMaxCapacity(TopNestResultsCollection sut)
    {
      for (int i = 0; i < sut.MaxCapacity; i++)
      {
        var item = A.Fake<INestResult>();
        A.CallTo(() => item.Fitness).Returns(sut.IsEmpty ? 10000 : sut.Top.Fitness * (1 - TestSvgNestConfig.Default.TopDiversity) - 1);
        sut.TryAdd(item);
        sut.Should().Contain(item);
      }
    }
  }
}
