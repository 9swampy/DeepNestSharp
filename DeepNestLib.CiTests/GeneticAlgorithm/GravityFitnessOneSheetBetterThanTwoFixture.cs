namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FluentAssertions;
  using Xunit;

  public class GravityFitnessOneSheetBetterThanTwoFixture
  {
    private readonly INestResult scenario1;
    private readonly INestResult scenario2;

    public GravityFitnessOneSheetBetterThanTwoFixture()
    {
      scenario1 = LoadNestResult("GeneticAlgorithm.ReadMeExampleTwoSheetsBetterThanOne1.dnr");
      scenario2 = LoadNestResult("GeneticAlgorithm.ReadMeExampleTwoSheetsBetterThanOne2.dnr");
    }

    public static INestResult LoadNestResult(string relativeResourcePath)
    {
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(relativeResourcePath))
      using (StreamReader reader = new StreamReader(stream))
      {
        return NestResult.FromJson(reader.ReadToEnd());
      }
    }

    [Fact]
    public void OneSheetFitnessShouldBeBetterThanTwo()
    {
      scenario1.FitnessTotal.Should().BeLessThan(scenario2.FitnessTotal);
    }

    [Fact]
    public void GivenTwoBoundsTightAroundTwoPartsTwoSheetsFitnessBoundsShouldBeBetterThanOne()
    {
      scenario2.FitnessBounds.Should().BeLessThan(scenario1.FitnessBounds);
    }

    [Fact]
    public void OneSheetPartsPlacedPercentShouldBeSameAsTwo()
    {
      scenario1.PartsPlacedPercent.Should().Be(scenario2.PartsPlacedPercent);
    }

    [Fact]
    public void OneSheetFitnessUnplacedShouldBeSameAsTwo()
    {
      scenario1.FitnessUnplaced.Should().BeApproximately(scenario2.FitnessUnplaced, 1);
    }
  }
}
