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
      config.PlacementType = PlacementTypeEnum.BoundingBox;
      this.nestResult = new Background(A.Fake<IProgressDisplayer>()).PlaceParts(new NFP[] { firstSheet }, new NFP[] { firstPart, secondPart, thirdPart, fourthPart }, config, 0);
    }

    [Fact]
    public void ShouldHaveSameFitnessBoundsAsOriginal()
    {
      this.nestResult.FitnessBounds.Should().BeApproximately(OriginalFitness.FitnessBounds(this.nestResult), 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessSheetsAsOriginal()
    {
      this.nestResult.FitnessSheets.Should().BeApproximately(OriginalFitness.FitnessSheets(this.nestResult), 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessUnplacedAsOriginal()
    {
      this.nestResult.FitnessUnplaced.Should().BeApproximately(OriginalFitness.FitnessUnplaced(this.nestResult), 10);
    }

    [Fact]
    public void ShouldHaveSameFitnessAsOriginal()
    {
      this.nestResult.fitness.Should().BeApproximately(this.nestResult.FitnessAlt, 10);
    }
  }
}
