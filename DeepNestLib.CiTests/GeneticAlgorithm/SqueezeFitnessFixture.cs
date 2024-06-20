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

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioBest);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioMid);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioWorst);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioBest);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioMid);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioWorst);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioBest);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioMid);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      OriginalFitnessSheet sut = new OriginalFitnessSheet(scenarioMid);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }
  }
}
