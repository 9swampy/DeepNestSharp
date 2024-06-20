namespace DeepNestLib.CiTests.Placement.OverlappingPlacement
{
  using System;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.GeneticAlgorithm;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class ChromosomeFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = A.Dummy<Chromosome>();
      act.Should().NotThrow();
    }

    [Fact]
    public void GivenChromosomeIsInstructionOnlyWhenGetPartExpectOnlyEverAClone()
    {
      NoFitPolygon actual = A.Dummy<NoFitPolygon>();
      Chromosome sut = new Chromosome(actual, 0);
      sut.Part.Should().NotBe(actual);
    }

    [Fact]
    public void GivenChromosomeIsInstructionOnlyWhenGetPartExpectEquivalentClone()
    {
      Random random = new Random();
      INfp actual = NoFitPolygon.FromDxf(new DxfGenerator().Rectangle(10, 20));
      actual.Children.Add(NoFitPolygon.FromDxf(new DxfGenerator().Rectangle(5, 10)).Shift(2.5, 5));
      actual.Id = random.Next();
      actual.Rotate(random.Next());
      Chromosome sut = new Chromosome(actual, 0);
      sut.Part.Should().BeEquivalentTo(actual);
    }
  }
}
