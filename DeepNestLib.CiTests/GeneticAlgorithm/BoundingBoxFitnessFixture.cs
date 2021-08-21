namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using DeepNestLib.GeneticAlgorithm;
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
      scenarioBest = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-BestBoundingBoxAllDxfSamples.json");
      scenarioMid = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-MidBoundingBoxAllDxfSamples.json");
      scenarioWorst = SheetPlacementJsonHelper.LoadSheetPlacement("GeneticAlgorithm.SheetPlacementScenario3-WorstBoundingBoxAllDxfSamples.json");
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
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestBoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioBest);
      sut.Bounds.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidBoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.Bounds.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstBoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.Bounds.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioBest);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioWorst);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioBest);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }
  }
}
