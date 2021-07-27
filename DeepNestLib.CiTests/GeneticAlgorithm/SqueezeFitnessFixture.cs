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
      scenarioBest = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-BestSqueezeAllDxfSamples.json");
      scenarioMid = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-MidSqueezeAllDxfSamples.json");
      scenarioWorst = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-WorstSqueezeAllDxfSamples.json");
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
