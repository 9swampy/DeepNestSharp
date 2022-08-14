namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using DeepNestLib.GeneticAlgorithm;
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
      scenario2.Fitness.Total.Should().BeLessThan(scenario1.Fitness.Total);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSamePartsPlacedOnEachThenMaterialUtilizationShouldBeSame()
    {
      scenario1.Fitness.Utilization.Should().BeApproximately(scenario2.Fitness.Utilization, 0.0001);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSamePartsPlacedOnEachButS2IsBetterByGravityIsGuessS2MaterialWastedShouldBeLessTbc()
    {
      var materialWasted1 = scenario1.Fitness.Wasted;
      var materialWasted2 = scenario2.Fitness.Wasted;
      materialWasted2.Should().BeLessThan(materialWasted1);
    }

    [Fact]
    public void GivenTwoSheetPlacementsWhenSameSheetsUsedOnEachThenSheetsFitnessShouldBeSame()
    {
      scenario1.Fitness.Sheets.Should().Be(scenario2.Fitness.Sheets);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1BoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario1);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Mid);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyWhenScalingAppliedThenScenario2BoundsShouldBeLowerRangeSheets()
    {
      var sut = new OriginalFitnessSheet(scenario2);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Lower);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario1);
      sut.Utilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenario2ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario2);
      sut.Utilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario1);
      sut.Wasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenario2ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario2);
      sut.Wasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }
  }
}
