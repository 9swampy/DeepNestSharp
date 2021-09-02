namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
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
      ISheet firstSheet;
      DxfGenerator.GenerateSquare("Sheet", 23D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      INfp firstPart;
      DxfGenerator.GenerateSquare("firstPart", 11D, RectangleType.FitFour).TryConvertToNfp(firstPartIdSrc, out firstPart).Should().BeTrue();
      // firstPart = firstPart.Rotate(180);
      firstPart.Rotation = 180;
      INfp secondPart;
      DxfGenerator.GenerateSquare("secondPart", 11D, RectangleType.FitFour).TryConvertToNfp(secondPartIdSrc, out secondPart).Should().BeTrue();
      // secondPart = secondPart.Rotate(180);
      secondPart.Rotation = 180;
      INfp thirdPart;
      DxfGenerator.GenerateSquare("thirdPart", 11D, RectangleType.FitFour).TryConvertToNfp(thirdPartIdSrc, out thirdPart).Should().BeTrue();
      // thirdPart = thirdPart.Rotate(180);
      thirdPart.Rotation = 180;
      INfp fourthPart;
      DxfGenerator.GenerateSquare("fourthPart", 11D, RectangleType.FitFour).TryConvertToNfp(fourthPartIdSrc, out fourthPart).Should().BeTrue();
      // fourthPart = fourthPart.Rotate(180);
      fourthPart.Rotation = 180;
      var config = new DefaultSvgNestConfig();
      config.PlacementType = PlacementTypeEnum.Gravity;
      this.nestResult = new PlacementWorker(A.Dummy<NfpHelper>(), new ISheet[] { firstSheet }, new INfp[] { firstPart, secondPart, thirdPart, fourthPart }.ApplyIndex(), config, A.Dummy<Stopwatch>(), A.Fake<INestState>()).PlaceParts();
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
      this.nestResult.Fitness.Should().BeApproximately(1207, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessBoundsAsOriginal()
    {
      this.nestResult.FitnessBounds.Should().BeApproximately(598, 10);
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
      this.nestResult.MaterialWasted.Should().BeApproximately(45, 10D);
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
      this.nestResult.UsedSheets[0].PartPlacements[0].X.Should().BeApproximately(11d, 0.01d, "bottom left");
    }

    [Fact]
    public void ShouldHaveFirstPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].Y.Should().BeApproximately(11d, 0.01d, "bottom left");
    }

    [Fact]
    public void ShouldHaveFirstPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].Rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveSecondPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[1].X.Should().BeApproximately(10.999999933643268f, 0.001f, "bottom right");
    }

    [Fact]
    public void ShouldHaveSecondPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[1].Y.Should().BeApproximately(21.9999999f, 0.001f, "bottom right");
    }

    [Fact]
    public void ShouldHaveSecondPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[1].Rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveThirdPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[2].X.Should().BeApproximately(21.999999900000002f, 0.001f, "bottom left");
    }

    [Fact]
    public void ShouldHaveThirdPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[2].Y.Should().BeApproximately(10.999999933643265f, 0.001f, "bottom left");
    }

    [Fact]
    public void ShouldHaveThirdPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[2].Rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveFourthPartOnPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[3].X.Should().BeApproximately(21.999999900000002f, 0.001f, "top right");
    }

    [Fact]
    public void ShouldHaveFourthPartOnPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[3].Y.Should().BeApproximately(21.9999999f, 0.001f, "top right");
    }

    [Fact]
    public void ShouldHaveFourthPartOnPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[3].Rotation.Should().Be(180);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMaxXThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MaxX.Should().BeApproximately(22, 0.1);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMaxYThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MaxY.Should().BeApproximately(22, 0.1);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMinXThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MinX.Should().BeApproximately(0, 0.1);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMinYThenShouldBeExpected()
    {
      this.nestResult.UsedSheets[0].MinY.Should().BeApproximately(0, 0.1);
    }
  }
}
