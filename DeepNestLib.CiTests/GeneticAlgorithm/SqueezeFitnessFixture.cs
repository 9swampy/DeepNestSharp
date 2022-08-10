namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using DeepNestLib.GeneticAlgorithm;
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
      scenarioBest.Fitness.Total.Should().BeLessThan(scenarioWorst.Fitness.Total);
    }

    [Fact]
    public void GivenAMidNestThenBestNestFitnessShouldBeLower()
    {
      scenarioBest.Fitness.Total.Should().BeLessThan(scenarioMid.Fitness.Total);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSamePartsPlacedOnEachThenMaterialUtilizationShouldBeSame()
    {
      scenarioWorst.Fitness.Utilization.Should().BeApproximately(scenarioBest.Fitness.Utilization, 1);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSameSheetsUsedOnEachThenSheetsFitnessShouldBeSame()
    {
      scenarioWorst.Fitness.Sheets.Should().BeApproximately(scenarioBest.Fitness.Sheets, 1);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioBest);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioWorst);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioBest);
      sut.Utilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.Utilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioWorst);
      sut.Utilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioBest);
      sut.Wasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.Wasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.Wasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }
  }
}
