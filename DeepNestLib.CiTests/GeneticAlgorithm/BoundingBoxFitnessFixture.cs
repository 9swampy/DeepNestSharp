namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FluentAssertions;
  using Xunit;

  public class BoundingBoxFitnessFixture
  {
    private readonly ISheetPlacement scenarioWorst;
    private readonly ISheetPlacement scenarioMid;
    private readonly ISheetPlacement scenarioBest;

    public BoundingBoxFitnessFixture()
    {
      scenarioBest = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-BestBoundingBoxAllDxfSamples.json");
      scenarioMid = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-MidBoundingBoxAllDxfSamples.json");
      scenarioWorst = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-WorstBoundingBoxAllDxfSamples.json");
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
    public void GivenABetterNestThenFitnessShouldBeLower()
    {
      scenarioBest.Fitness.Evaluate().Should().BeLessThan(scenarioWorst.Fitness.Evaluate());
    }

    [Fact]
    public void GivenAMidNestThenBestNestFitnessShouldBeLower()
    {
      scenarioBest.Fitness.Evaluate().Should().BeLessThan(scenarioMid.Fitness.Evaluate());
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSamePartsPlacedOnEachThenMaterialUtilizationShouldBeSame()
    {
      scenarioWorst.Fitness.MaterialUtilization.Should().Be(scenarioBest.Fitness.MaterialUtilization);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSameSheetsUsedOnEachThenSheetsFitnessShouldBeSame()
    {
      scenarioWorst.Fitness.Sheets.Should().Be(scenarioBest.Fitness.Sheets);
    }
  }
}
