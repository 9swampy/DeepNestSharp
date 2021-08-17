namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using FluentAssertions;
  using Xunit;

  public class GravityFitnessOneSheetBetterThanTwoFixture
  {
    private readonly INestResult scenario1;
    private readonly INestResult scenario2;

    public GravityFitnessOneSheetBetterThanTwoFixture()
    {
      scenario1 = LoadNestResult("GeneticAlgorithm.ReadMeExampleTwoSheetsBetterThanOne1.dnr");
      scenario2 = LoadNestResult("GeneticAlgorithm.ReadMeExampleTwoSheetsBetterThanOne2.dnr");
    }

    public static INestResult LoadNestResult(string relativeResourcePath)
    {
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(relativeResourcePath))
      using (StreamReader reader = new StreamReader(stream))
      {
        return NestResult.FromJson(reader.ReadToEnd());
      }
    }

    [Fact]
    public void OneSheetFitnessShouldBeBetterThanTwo()
    {
      scenario1.Fitness.Should().BeLessThan(scenario2.Fitness);
    }

    [Fact]
    public void GivenTwoBoundsTightAroundTwoPartsTwoSheetsFitnessBoundsShouldBeBetterThanOne()
    {
      scenario2.FitnessBounds.Should().BeLessThan(scenario1.FitnessBounds);
    }

    [Fact]
    public void OneSheetPartsPlacedPercentShouldBeSameAsTwo()
    {
      scenario1.PartsPlacedPercent.Should().Be(scenario2.PartsPlacedPercent);
    }

    [Fact]
    public void OneSheetFitnessUnplacedShouldBeSameAsTwo()
    {
      scenario1.FitnessUnplaced.Should().BeApproximately(scenario2.FitnessUnplaced, 1);
    }
  }

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
    public void GivenTwoSheetPlacementsWhenSamePartsPlacedOnEachButS2IsBetterByGravityIsGuessS2MaterialWastedShouldBeLessTbc()
    {
      scenario2.Fitness.MaterialWasted.Should().BeLessThan(scenario1.Fitness.MaterialWasted);
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
      sut.Bounds.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenario2BoundsShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario2);
      sut.Bounds.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario1);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenario2ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario2);
      sut.MaterialUtilization.Should().BeApproximately(sut.Sheets, sut.Sheets / 2);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario1);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenario2ShouldBeComingCloseToSheets()
    {
      var sut = new OriginalFitnessSheet(scenario2);
      sut.MaterialWasted.Should().BeApproximately(sut.Sheets * 1.5, sut.Sheets);
    }
  }
}
