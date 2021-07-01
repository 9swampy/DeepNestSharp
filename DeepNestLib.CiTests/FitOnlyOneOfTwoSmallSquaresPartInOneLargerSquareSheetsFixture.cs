namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class FitOnlyOneOfTwoSmallSquaresPartInOneLargerSquareSheetsFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private SheetPlacement sheetPlacement;
    private int firstSheetIdSrc = new Random().Next();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();

    public FitOnlyOneOfTwoSmallSquaresPartInOneLargerSquareSheetsFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>());
      NFP firstSheet;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(20D) }), firstSheetIdSrc, out firstSheet).Should().BeTrue();
      NFP firstPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), firstPartIdSrc, out firstPart).Should().BeTrue();
      NFP secondPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), secondPartIdSrc, out secondPart).Should().BeTrue();
      this.sheetPlacement = new Background().PlaceParts(new NFP[] { firstSheet }, new NFP[] { firstPart, secondPart }, new SvgNestConfig(), 0);
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
      this.sheetPlacement.placements[0][0].sheetplacements.Count.Should().Be(1, "there is one part on each sheet");
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

    //[Fact]
    //public void ShouldHavePartLeftOver()
    //{ 
    //this.sheetPlacement
    //}
  }
}
