namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class DeepNestGeneFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new DeepNestGene(new List<Chromosome>());

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldDummyCtor()
    {
      Action act = () => _ = A.Dummy<DeepNestGene>();

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldStoreChromosome()
    {
      var expected = new Chromosome(A.Dummy<INfp>(), 123);
      var sut = new DeepNestGene(new List<Chromosome>() { expected });
      sut.Should().Contain(expected);
      sut.Length.Should().Be(1);
    }

    [Fact]
    public void ShouldSerializeWithoutThrow()
    {
      var expected = new Chromosome(new NoFitPolygon(), 123);
      var sut = new DeepNestGene(new List<Chromosome>() { expected });
      var json = sut.ToJson();

      Action act = () => _ = json.Should().NotBeNullOrWhiteSpace();
      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var expected = new Chromosome(new NoFitPolygon(), 123);
      var sut = new DeepNestGene(new List<Chromosome>() { expected });
      var json = sut.ToJson();

      DeepNestGene.FromJson(json).Should().BeEquivalentTo(sut);
    }
  }
}
