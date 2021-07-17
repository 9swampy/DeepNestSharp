namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FluentAssertions;
  using Xunit;

  public class SqueezeFitnessFixture
  {
    private readonly ISheetPlacement scenarioWorst;
    private readonly ISheetPlacement scenarioMid;
    private readonly ISheetPlacement scenarioBest;

    public SqueezeFitnessFixture()
    {
      scenarioBest = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-BestSqueezeAllDxfSamples.json");
      scenarioMid = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-MidSqueezeAllDxfSamples.json");
      scenarioWorst = LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-WorstSqueezeAllDxfSamples.json");
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
