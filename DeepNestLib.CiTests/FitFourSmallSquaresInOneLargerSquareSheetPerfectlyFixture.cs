namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class FitFourSmallSquaresInOneLargerSquareSheetPerfectlyFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private SheetPlacement sheetPlacement;
    private int firstSheetIdSrc = new Random().Next();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();
    private int thirdPartIdSrc = new Random().Next();
    private int fourthPartIdSrc = new Random().Next();

    public FitFourSmallSquaresInOneLargerSquareSheetPerfectlyFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>());
      NFP firstSheet;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(23D, RectangleType.FileLoad) }), firstSheetIdSrc, out firstSheet).Should().BeTrue();
      NFP firstPart = new NFP();
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("firstPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), firstPartIdSrc, out firstPart).Should().BeTrue();
      firstPart.Rotation = 180;
      NFP secondPart = new NFP();
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("firstPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), secondPartIdSrc, out secondPart).Should().BeTrue();
      secondPart.Rotation = 180;
      NFP thirdPart = new NFP();
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("firstPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), thirdPartIdSrc, out thirdPart).Should().BeTrue();
      thirdPart.Rotation = 180;
      NFP fourthPart = new NFP();
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("firstPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FitFour) }), fourthPartIdSrc, out fourthPart).Should().BeTrue();
      fourthPart.Rotation = 180;
      var config = new DefaultSvgNestConfig();
      this.sheetPlacement = new Background().PlaceParts(new NFP[] { firstSheet }, new NFP[] { firstPart, secondPart, thirdPart, fourthPart }, config, 0);
    }

    [Fact]
    public void ShouldHaveReturnedASheetPlacement()
    {
      this.sheetPlacement.Should().NotBeNull();
    }

    [Fact]
    public void GivenOnePartOnlyThenShouldBeNoMergedLines()
    {
      this.sheetPlacement.mergedLength.Should().Be(0, "there was only one part on each sheet; no lines to merge possible.");
    }

    [Fact]
    public void ShouldHaveExpectedFitness()
    {
      this.sheetPlacement.fitness.Should().BeApproximately(617.04158790170129, 10);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.sheetPlacement.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      this.sheetPlacement.area.Should().Be(529);
    }

    [Fact]
    public void ShouldHaveOnePlacement()
    {
      this.sheetPlacement.placements.Length.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOneNestResultWithOneSheet()
    {
      this.sheetPlacement.placements[0].Count.Should().Be(1);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedId()
    {
      this.sheetPlacement.placements[0][0].sheetId.Should().Be(0);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedSource()
    {
      this.sheetPlacement.placements[0][0].sheetSource.Should().Be(firstSheetIdSrc, "this is the first sheet in the single nest result");
    }

    [Fact]
    public void ShouldHaveNoPlacementsOnFirstSheet()
    {
      this.sheetPlacement.placements[0][0].placements.Should().BeEmpty("not sure why there would be placements on the sheet; inners maybe?");
    }

    [Fact]
    public void ShouldHaveFirstSheet()
    {
      this.sheetPlacement.placements[0][0].sheetplacements.Count.Should().Be(4, "they all fit on one sheet");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedX()
    {
      this.sheetPlacement.placements[0][0].sheetplacements[0].x.Should().Be(10.999999933643268, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedY()
    {
      this.sheetPlacement.placements[0][0].sheetplacements[0].y.Should().Be(10.999999933643265, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedRotation()
    {
      this.sheetPlacement.placements[0][0].sheetplacements[0].rotation.Should().Be(180);
    }

    //[Fact]
    //public void ShouldHavePartLeftOver()
    //{ 
    //this.sheetPlacement
    //}
  }
}
