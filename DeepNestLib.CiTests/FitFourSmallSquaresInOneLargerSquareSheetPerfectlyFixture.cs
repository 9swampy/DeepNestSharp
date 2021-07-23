namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class FitFourSmallSquaresInOneLargerSquareSheetPerfectlyFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private NestResult nestResult;
    private int firstSheetIdSrc = new Random().Next();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();
    private int thirdPartIdSrc = new Random().Next();
    private int fourthPartIdSrc = new Random().Next();

    public FitFourSmallSquaresInOneLargerSquareSheetPerfectlyFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      NFP firstSheet;
      DxfGenerator.GenerateSquare("Sheet", 23D, RectangleType.FileLoad).TryImportFromRawDetail(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      NFP firstPart;
      DxfGenerator.GenerateSquare("firstPart", 11D, RectangleType.FitFour).TryImportFromRawDetail(firstPartIdSrc, out firstPart).Should().BeTrue();
      // firstPart = firstPart.Rotate(180);
      firstPart.Rotation = 180;
      NFP secondPart;
      DxfGenerator.GenerateSquare("secondPart", 11D, RectangleType.FitFour).TryImportFromRawDetail(secondPartIdSrc, out secondPart).Should().BeTrue();
      // secondPart = secondPart.Rotate(180);
      secondPart.Rotation = 180;
      NFP thirdPart;
      DxfGenerator.GenerateSquare("thirdPart", 11D, RectangleType.FitFour).TryImportFromRawDetail(thirdPartIdSrc, out thirdPart).Should().BeTrue();
      // thirdPart = thirdPart.Rotate(180);
      thirdPart.Rotation = 180;
      NFP fourthPart;
      DxfGenerator.GenerateSquare("fourthPart", 11D, RectangleType.FitFour).TryImportFromRawDetail(fourthPartIdSrc, out fourthPart).Should().BeTrue();
      // fourthPart = fourthPart.Rotate(180);
      fourthPart.Rotation = 180;
      var config = new DefaultSvgNestConfig();
      config.PlacementType = PlacementTypeEnum.Gravity;
      this.nestResult = new Background(A.Fake<IProgressDisplayer>()).PlaceParts(new NFP[] { firstSheet }, new NFP[] { firstPart, secondPart, thirdPart, fourthPart }, config, 0);
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
    [Obsolete]
    public void ShouldHaveExpectedFitness()
    {
      this.nestResult.Fitness.Should().BeApproximately(976, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessBoundsAsOriginal()
    {
      this.nestResult.FitnessBounds.Should().BeApproximately(322, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessUnplacedAsOriginal()
    {
      this.nestResult.FitnessUnplaced.Should().BeApproximately(0, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessSheetsAsOriginal()
    {
      this.nestResult.FitnessSheets.Should().Be(529);
    }

    [Fact]
    public void ShouldHaveExpectedFitnessMaterialWasted()
    {
      this.nestResult.MaterialWasted.Should().Be(90);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.nestResult.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveOnePlacement()
    {
      this.nestResult.UsedSheets.Count.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOneNestResultWithOneSheet()
    {
      this.nestResult.UsedSheets.Count.Should().Be(1);
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
    public void ShouldHaveFirstSheet()
    {
      this.nestResult.UsedSheets[0].PartPlacements.Count.Should().Be(4, "they all fit on one sheet");
    }

    [Fact]
    public void ShouldHaveFirstPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].x.Should().Be(10.999999933643268, "bottom left");
    }

    [Fact]
    public void ShouldHaveFirstPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].y.Should().Be(10.999999933643265, "bottom left");
    }

    [Fact]
    public void ShouldHaveFirstPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveSecondPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[1].x.Should().BeApproximately(10.999999933643268, 0.001, "bottom right");
    }

    [Fact]
    public void ShouldHaveSecondPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[1].y.Should().BeApproximately(21.9999999, 0.001, "bottom right");
    }

    [Fact]
    public void ShouldHaveSecondPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[1].rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveThirdPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[2].x.Should().BeApproximately(21.999999900000002, 0.001, "bottom left");
    }

    [Fact]
    public void ShouldHaveThirdPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[2].y.Should().BeApproximately(10.999999933643265, 0.001, "bottom left");
    }

    [Fact]
    public void ShouldHaveThirdPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[2].rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveFourthPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[3].x.Should().BeApproximately(21.999999900000002, 0.001, "top right");
    }

    [Fact]
    public void ShouldHaveFourthPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[3].y.Should().BeApproximately(21.9999999, 0.001, "top right");
    }

    [Fact]
    public void ShouldHaveFourthPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[3].rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }
  }
}
