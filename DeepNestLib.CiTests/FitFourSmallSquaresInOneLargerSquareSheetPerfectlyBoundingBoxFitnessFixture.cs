namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using DeepNestLib.CiTests.GeneticAlgorithm;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.GeneticAlgorithm;
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
      Chromosome firstPart;
      DxfGenerator.GenerateSquare("firstPart", 11D, RectangleType.FitFour).TryConvertToNfp(firstPartIdSrc, 180, out firstPart).Should().BeTrue();
      Chromosome secondPart;
      DxfGenerator.GenerateSquare("secondPart", 11D, RectangleType.FitFour).TryConvertToNfp(secondPartIdSrc, 180, out secondPart).Should().BeTrue();
      Chromosome thirdPart;
      DxfGenerator.GenerateSquare("thirdPart", 11D, RectangleType.FitFour).TryConvertToNfp(thirdPartIdSrc, 180, out thirdPart).Should().BeTrue();
      Chromosome fourthPart;
      DxfGenerator.GenerateSquare("fourthPart", 11D, RectangleType.FitFour).TryConvertToNfp(fourthPartIdSrc, 180, out fourthPart).Should().BeTrue();
      var config = new TestSvgNestConfig();
      config.PlacementType = PlacementTypeEnum.BoundingBox;
      this.nestResult = new PlacementWorker(A.Dummy<NfpHelper>(), new ISheet[] { firstSheet }, new DeepNestGene(new Chromosome[] { firstPart, secondPart, thirdPart, fourthPart }.ApplyIndex()), config, A.Dummy<Stopwatch>(), A.Fake<INestState>()).PlaceParts();
    }

    [Fact]
    public void ShouldHaveSameFitnessBoundsAsOriginal()
    {
      this.nestResult.FitnessBounds.Should().BeApproximately(414, 10);
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
      this.nestResult.Fitness.Should().BeApproximately(1024, 100);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenScenario1BoundsShouldBeComingCloseToSheets()
    {
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(nestResult, FitnessRange.Upper);
    }

    [Fact]
    public void GivenMaterialUtilizationPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      this.nestResult.MaterialUtilization.Should().BeApproximately(this.nestResult.FitnessSheets - (this.nestResult.FitnessSheets / 4), 3 * this.nestResult.FitnessSheets / 4);
    }

    [Fact]
    public void GivenMaterialUtilizationHighThenPenaltyShouldBeASmallFractionOfSheets()
    {
      this.nestResult.MaterialUtilization.Should().BeLessThan(this.nestResult.FitnessSheets / 100);
    }

    [Fact]
    public void GivenMaterialWastedPenaltyShouldBeInLineWithSheetsPenaltyThenScenarioBestShouldBeComingCloseToSheets()
    {
      this.nestResult.MaterialWasted.Should().BeApproximately(this.nestResult.FitnessSheets - (this.nestResult.FitnessSheets / 4), 3 * this.nestResult.FitnessSheets / 4);
    }

    [Fact]
    public void GivenMaterialWastedLowThenPenaltyShouldBeASmallFractionOfSheets()
    {
      this.nestResult.MaterialWasted.Should().BeLessThan(this.nestResult.FitnessSheets / 10);
    }
  }
}
