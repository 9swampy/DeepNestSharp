namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class FitTwoSmallSquaresPartInTwoLargerSquareSheetsFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private SheetPlacement sheetPlacement;
    private int firstSheetIdSrc = new Random().Next();
    private int secondSheetIdSrc = new Random().Next();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();

    public FitTwoSmallSquaresPartInTwoLargerSquareSheetsFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      NFP firstSheet;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(20D) }), firstSheetIdSrc, out firstSheet).Should().BeTrue();
      NFP secondSheet;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(20D) }), secondSheetIdSrc, out secondSheet).Should().BeTrue();
      NFP firstPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), firstPartIdSrc, out firstPart).Should().BeTrue();
      NFP secondPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), secondPartIdSrc, out secondPart).Should().BeTrue();
      this.sheetPlacement = new Background(A.Fake<IProgressDisplayer>()).PlaceParts(new NFP[] { firstSheet, secondSheet }, new NFP[] { firstPart, secondPart }, new SvgNestConfig(), 0);
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
      this.sheetPlacement.fitness.Should().Be(double.NaN);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.sheetPlacement.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      this.sheetPlacement.area.Should().Be(400);
    }

    [Fact]
    public void ShouldHaveOnePlacement()
    {
      this.sheetPlacement.placements.Length.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOneNestResultWithTwoSheets()
    {
      this.sheetPlacement.placements[0].Count.Should().Be(2);
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
    public void SecondSheetShouldHaveExpectedSource()
    {
      this.sheetPlacement.placements[0][1].sheetSource.Should().Be(secondSheetIdSrc, "this is the second sheet in the single nest result");
    }

    [Fact]
    public void ShouldHaveNoPlacementsOnFirstSheet()
    {
      this.sheetPlacement.placements[0][0].placements.Should().BeEmpty("not sure why there would be placements on the sheet; inners maybe?");
    }

    [Fact]
    public void ShouldHaveNoPlacementsOnSecondSheet()
    {
      this.sheetPlacement.placements[0][1].placements.Should().BeEmpty("not sure why there would be placements on the sheet; inners maybe?");
    }

    [Fact]
    public void ShouldHaveFirstSheet()
    {
      this.sheetPlacement.placements[0][0].sheetplacements.Count.Should().Be(1, "there is one part on each sheet");
    }

    [Fact]
    public void ShouldHaveSecondSheet()
    {
      this.sheetPlacement.placements[0][1].sheetplacements.Count.Should().Be(1, "there is one part on each sheet");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedX()
    {
      this.sheetPlacement.placements[0][0].sheetplacements[0].x.Should().Be(0, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedY()
    {
      this.sheetPlacement.placements[0][0].sheetplacements[0].y.Should().Be(0, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedRotation()
    {
      this.sheetPlacement.placements[0][0].sheetplacements[0].rotation.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedX()
    {
      this.sheetPlacement.placements[0][1].sheetplacements[0].x.Should().Be(-11, "both sheet 1 and 1 part should be bottom left, not sure why this isn't 0 too");
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedY()
    {
      this.sheetPlacement.placements[0][1].sheetplacements[0].y.Should().Be(-11, "both sheet 1 and 1 part should be bottom left, not sure why this isn't 0 too");
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedRotation()
    {
      this.sheetPlacement.placements[0][1].sheetplacements[0].rotation.Should().Be(0);
    }
  }
}
