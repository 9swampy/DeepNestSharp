namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FluentAssertions;
  using Xunit;

  public class GravityFitnessFixture
  {
    private readonly ISheetPlacement scenario1;
    private readonly ISheetPlacement scenario2;

    public GravityFitnessFixture()
    {
      scenario1 = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario1.json");
      scenario2 = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario2.json");
    }

    private ISheetPlacement LoadSheetPlacement(string relativeResourcePath)
    {
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(relativeResourcePath))
      using (StreamReader reader = new StreamReader(stream))
      {
        return SheetPlacement.FromJson(reader.ReadToEnd());
      }
    }

    [Fact]
    public void GivenABetterGravityNestThenFitnessShouldBeLower()
    {
      scenario2.Fitness.Evaluate().Should().BeLessThan(scenario1.Fitness.Evaluate());
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSamePartsPlacedOnEachThenMaterialUtilizationShouldBeSame()
    {
      scenario1.Fitness.MaterialUtilization.Should().Be(scenario2.Fitness.MaterialUtilization);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSameSheetsUsedOnEachThenSheetsFitnessShouldBeSame()
    {
      scenario1.Fitness.Sheets.Should().Be(scenario2.Fitness.Sheets);
    }
  }
}
