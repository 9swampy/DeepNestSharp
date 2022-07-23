namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using DeepNestLib.CiTests.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class FitTwoSmallSquaresPartInTwoLargerSquareSheetsFixture
  {
    private readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private readonly NestResult nestResult;
    private readonly int firstSheetIdSrc = new Random().Next();
    private readonly int secondSheetIdSrc = new Random().Next();
    private readonly int firstPartIdSrc = new Random().Next();
    private readonly int secondPartIdSrc = new Random().Next();

    public FitTwoSmallSquaresPartInTwoLargerSquareSheetsFixture()
    {
      ISheet firstSheet;
      DxfGenerator.GenerateSquare("Sheet", 20D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      ISheet secondSheet;
      DxfGenerator.GenerateSquare("Sheet", 20D, RectangleType.FileLoad).TryConvertToSheet(secondSheetIdSrc, out secondSheet).Should().BeTrue();
      Chromosome firstPart;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryConvertToNfp(firstPartIdSrc, out firstPart).Should().BeTrue();
      Chromosome secondPart;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryConvertToNfp(secondPartIdSrc, out secondPart).Should().BeTrue();
      var config = A.Fake<IPlacementConfig>();
      A.CallTo(() => config.PlacementType).Returns(PlacementTypeEnum.BoundingBox);
      A.CallTo(() => config.UseDllImport).Returns(false);
      A.CallTo(() => config.Rotations).Returns(2);
      A.CallTo(() => config.ExportExecutions).Returns(false);
      this.nestResult = new PlacementWorker(A.Dummy<NfpHelper>(), new ISheet[] { firstSheet, secondSheet }, new Chromosome[] { firstPart, secondPart }.ApplyIndex(), config, A.Dummy<Stopwatch>(), A.Fake<INestState>()).PlaceParts();
    }

    [Fact]
    public void ShouldHaveReturnedASheetPlacement()
    {
      this.nestResult.Should().NotBeNull();
    }

    [Fact]
    public void GivenOnePartOnlyThenShouldBeNoMergedLines()
    {
      this.nestResult.MergedLength.Should().Be(0, "there was only one part on each sheet; no lines to merge possible.");
    }

    [Fact]
    public void ShouldHaveExpectedFitness()
    {
      // Churns consistently on 2619, but occasionally comes back with 2506. Why? Hmm, moving the Bounds return
      // inside the lock and now it's stable on 2506 which is the correct answer. . . don't follow. . .
      this.nestResult.FitnessSheets.Should().BeApproximately(800, 1);
      this.nestResult.MaterialWasted.Should().BeApproximately(961, 1);
      this.nestResult.MaterialUtilization.Should().BeApproximately(1, 1);
      this.nestResult.UsedSheets.Count.Should().Be(2);
      this.nestResult.UsedSheets[0].PlacementType.Should().Be(PlacementTypeEnum.BoundingBox);
      this.nestResult.UsedSheets[0].RectBounds.X.Should().Be(0);
      this.nestResult.UsedSheets[0].RectBounds.Y.Should().Be(0);
      this.nestResult.UsedSheets[0].Hull.Area.Should().Be(121);
      this.nestResult.UsedSheets[0].RectBounds.Width.Should().Be(11);
      this.nestResult.UsedSheets[0].RectBounds.Height.Should().Be(11);
      this.nestResult.UsedSheets[0].Fitness.Bounds.Should().BeApproximately(103, 1);
      this.nestResult.Fitness.Should().BeApproximately(2506, 1);
    }

    [Fact]
    public void ShouldHaveNoFitnessUnplaced()
    {
      this.nestResult.FitnessUnplaced.Should().Be(0);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1BoundsShouldBeComingCloseToSheets()
    {
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(nestResult, FitnessRange.Mid);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.nestResult.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveOneNestResultWithTwoSheets()
    {
      this.nestResult.UsedSheets.Count.Should().Be(2);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedId()
    {
      this.nestResult.UsedSheets[0].SheetId.Should().Be(0);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedSource()
    {
      this.nestResult.UsedSheets[0].SheetSource.Should().Be(firstSheetIdSrc, "this is the first sheet in the single nest result");
    }

    [Fact]
    public void SecondSheetShouldHaveExpectedSource()
    {
      this.nestResult.UsedSheets[1].SheetSource.Should().Be(secondSheetIdSrc, "this is the second sheet in the single nest result");
    }

    [Fact]
    public void ShouldHaveFirstSheet()
    {
      this.nestResult.UsedSheets[0].PartPlacements.Count.Should().Be(1, "there is one part on each sheet");
    }

    [Fact]
    public void ShouldHaveSecondSheet()
    {
      this.nestResult.UsedSheets[1].PartPlacements.Count.Should().Be(1, "there is one part on each sheet");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].X.Should().Be(0, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].Y.Should().Be(0, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].Rotation.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[1].PartPlacements[0].X.Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[1].PartPlacements[0].Y.Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[1].PartPlacements[0].Rotation.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMaxXThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MaxX.Should().Be(11);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMaxYThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MaxY.Should().Be(11);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMinXThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MinX.Should().Be(0);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMinYThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MinY.Should().Be(0);
    }
  }
}
