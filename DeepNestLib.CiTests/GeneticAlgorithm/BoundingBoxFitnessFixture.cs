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
      scenarioWorst.Fitness.Utilization.Should().Be(scenarioBest.Fitness.Utilization);
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
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioMidBoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioWorstBoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenarioMid);
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

  public enum FitnessRange
  {
    Lower,
    Mid,
    Upper
  }

  public class FitnessAlignment
  {
    internal static void BoundsPenaltyShouldBeInLineWithSheetsPenalty(OriginalFitnessSheet sut, FitnessRange fitnessRange)
    {
      BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut.Bounds, sut.Sheets, fitnessRange);
    }

    internal static void BoundsPenaltyShouldBeInLineWithSheetsPenalty(NestResult nestResult, FitnessRange fitnessRange)
    {
      BoundsPenaltyShouldBeInLineWithSheetsPenalty(nestResult.FitnessBounds, nestResult.FitnessSheets, fitnessRange);
    }
    private static void BoundsPenaltyShouldBeInLineWithSheetsPenalty(double actual, double max, FitnessRange fitnessRange)
    {
      switch (fitnessRange)
      {
        case FitnessRange.Lower:
          actual.Should().BeInRange(0, max / 2);
          break;
        case FitnessRange.Mid:
          actual.Should().BeInRange(max * .25, max * .75);
          break;
        case FitnessRange.Upper:
          actual.Should().BeInRange(max / 2, max);
          break;
        default:
          actual.Should().BeInRange(0, max);
          break;
      }
    }
  }
}
