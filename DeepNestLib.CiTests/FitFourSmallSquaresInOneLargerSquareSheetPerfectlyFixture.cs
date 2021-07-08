namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
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
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(23D, RectangleType.FileLoad) }), firstSheetIdSrc, out firstSheet).Should().BeTrue();
      NFP firstPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("firstPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), firstPartIdSrc, out firstPart).Should().BeTrue();
      // firstPart = firstPart.Rotate(180);
      firstPart.Rotation = 180;
      NFP secondPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("secondPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), secondPartIdSrc, out secondPart).Should().BeTrue();
      // secondPart = secondPart.Rotate(180);
      secondPart.Rotation = 180;
      NFP thirdPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("thirdPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), thirdPartIdSrc, out thirdPart).Should().BeTrue();
      // thirdPart = thirdPart.Rotate(180);
      thirdPart.Rotation = 180;
      NFP fourthPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("fourthPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), fourthPartIdSrc, out fourthPart).Should().BeTrue();
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
      this.nestResult.mergedLength.Should().Be(0, "there was only one part on each sheet; no lines to merge possible.");
    }

    [Fact]
    [Obsolete]
    public void ShouldHaveExpectedFitness()
    {
      this.nestResult.fitness.Should().BeApproximately(617.04158790170129, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessAsOriginal()
    {
      this.nestResult.Fitness.Should().Be(this.nestResult.FitnessAlt, "fitness local field maintains the difference, not the exposed property.");
    }

    [Fact]
    public void ShouldHaveSameFitnessBoundsAsOriginal()
    {
      this.nestResult.FitnessBounds.Should().BeApproximately(88, 10);
    }

    [Fact]
    public void ShouldHaveExpectedFitnessBounds()
    {
      OriginalFitness.FitnessBounds(this.nestResult).Should().BeApproximately(968, 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessUnplacedAsOriginal()
    {
      this.nestResult.FitnessUnplaced.Should().BeApproximately(OriginalFitness.FitnessUnplaced(this.nestResult), 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessSheetsAsOriginal()
    {
      this.nestResult.FitnessSheets.Should().BeApproximately(OriginalFitness.FitnessSheets(this.nestResult), 10);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.nestResult.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      this.nestResult.area.Should().Be(529);
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

    //[Fact]
    //public void ShouldHavePartLeftOver()
    //{ 
    //this.sheetPlacement
    //}
  }
}
