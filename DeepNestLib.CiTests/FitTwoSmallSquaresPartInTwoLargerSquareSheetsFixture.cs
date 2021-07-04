namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class FitTwoSmallSquaresPartInTwoLargerSquareSheetsFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private NestResult nestResult;
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
      this.nestResult = new Background(A.Fake<IProgressDisplayer>()).PlaceParts(new NFP[] { firstSheet, secondSheet }, new NFP[] { firstPart, secondPart }, new SvgNestConfig(), 0);
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
    public void ShouldHaveExpectedFitness()
    {
      this.nestResult.fitness.Should().Be(double.NaN);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.nestResult.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      this.nestResult.area.Should().Be(400);
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
      this.nestResult.UsedSheets[0].PartPlacements[0].x.Should().Be(0, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].y.Should().Be(0, "bottom left");
    }

    [Fact]
    public void ShouldHaveOnePartOnFirstPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].rotation.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[1].PartPlacements[0].x.Should().Be(-11, "both sheet 1 and 1 part should be bottom left, not sure why this isn't 0 too");
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[1].PartPlacements[0].y.Should().Be(-11, "both sheet 1 and 1 part should be bottom left, not sure why this isn't 0 too");
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[1].PartPlacements[0].rotation.Should().Be(0);
    }
  }
}
