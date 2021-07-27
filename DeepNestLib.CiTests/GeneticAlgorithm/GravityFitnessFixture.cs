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
      scenario1 = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario1.json");
      scenario2 = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario2.json");
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
