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
    public void GivenEmptyCollectionWhenAddResultThenCountIncrement()
    {
      var sut = new TopNestResultsCollection(new DefaultSvgNestConfig());
      sut.Add(A.Fake<INestResult>());

      sut.Count.Should().Be(1);
    }

    [Fact]
    public void GivenEmptyCollectionWhenAddResultThenMemoise()
    {
      var sut = new TopNestResultsCollection(new DefaultSvgNestConfig());
      var expected = A.Fake<INestResult>();
      sut.Add(expected);

      sut.Single().Should().Be(expected);
    }

    [Fact]
    public void MaxCapacityShouldBeGreaterThanOrEqualToEliteSurvivors()
    {
      var sut = new TopNestResultsCollection(new DefaultSvgNestConfig());
      sut.MaxCapacity.Should().BeGreaterOrEqualTo(sut.EliteSurvivors);
    }

    [Fact]
    public void GivenHigherFitnessWhenAddResultThenMemoiseAfterExisting()
    {
      var sut = new TopNestResultsCollection(new DefaultSvgNestConfig());
      var first = A.Fake<INestResult>();
      sut.Add(first);
      A.CallTo(() => first.Fitness).Returns(1);
      sut.First().Should().Be(first);

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(2);
      sut.Add(second);

      sut.Skip(1).First().Should().Be(second);
    }

    [Fact]
    public void GivenLowerFitnessWhenAddResultThenMemoiseBeforeExisting()
    {
      var sut = new TopNestResultsCollection(new DefaultSvgNestConfig());
      var first = A.Fake<INestResult>();
      sut.Add(first);
      A.CallTo(() => first.Fitness).Returns(2);
      sut.First().Should().Be(first);

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(1);
      sut.Add(second);

      sut.First().Should().Be(second);
    }

    [Fact]
    public void GivenPopulationFullWhenAddTopResultThenNoCountIncrement()
    {
      var random = new Random();
      var config = new DefaultSvgNestConfig();
      var sut = new TopNestResultsCollection(config);
      for (int i = 0; i < sut.MaxCapacity; i++)
      {
        var item = A.Fake<INestResult>();
        A.CallTo(() => item.Fitness).Returns(i + 1);
        sut.Add(item);
        sut.Should().Contain(item);
      }

      var second = A.Fake<INestResult>();
      A.CallTo(() => second.Fitness).Returns(0);
      sut.Add(second);

      sut.First().Should().Be(second);
      sut.Count.Should().Be(sut.MaxCapacity);
    }
  }
}
