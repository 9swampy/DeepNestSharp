namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class FitFourSmallSquaresInOneLargerSquareSheetPerfectlyBoundingBoxFitnessFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private NestResult nestResult;
    private int firstSheetIdSrc = new Random().Next();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();
    private int thirdPartIdSrc = new Random().Next();
    private int fourthPartIdSrc = new Random().Next();

    public FitFourSmallSquaresInOneLargerSquareSheetPerfectlyBoundingBoxFitnessFixture()
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
      config.PlacementType = PlacementTypeEnum.BoundingBox;
      this.nestResult = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).PlaceParts(new ISheet[] { firstSheet }, new INfp[] { firstPart, secondPart, thirdPart, fourthPart }, config, A.Dummy<Stopwatch>());
    }

    [Fact]
    public void ShouldHaveSameFitnessBoundsAsOriginal()
    {
      this.nestResult.FitnessBounds.Should().BeApproximately(645, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessSheetsAsOriginal()
    {
      this.nestResult.FitnessSheets.Should().BeApproximately(529, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessUnplacedAsOriginal()
    {
      this.nestResult.FitnessUnplaced.Should().BeApproximately(0, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessAsOriginal()
    {
      this.nestResult.Fitness.Should().BeApproximately(1299, 10);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }


    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1BoundsShouldBeComingCloseToSheets()
    {
      this.nestResult.FitnessSheets.Should().BeApproximately(this.nestResult.FitnessBounds, this.nestResult.FitnessBounds / 2);
    }
  }
}
