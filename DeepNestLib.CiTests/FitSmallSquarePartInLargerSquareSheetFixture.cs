namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class FitSmallSquarePartInLargerSquareSheetFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private NestResult nestResult;

    public FitSmallSquarePartInLargerSquareSheetFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      NFP sheet;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(22D) }), 0, out sheet).Should().BeTrue();
      NFP part;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), 0, out part).Should().BeTrue();
      this.nestResult = new Background(A.Fake<IProgressDisplayer>()).PlaceParts(new NFP[] { sheet }, new NFP[] { part }, new SvgNestConfig(), 0);
    }

    [Fact]
    public void GivenNullSheetsPassedInThenNullReturned()
    {
      new Background(A.Fake<IProgressDisplayer>()).PlaceParts(null, new NFP[] { new NFP() }, new SvgNestConfig(), 0).Should().BeNull();
    }

    [Fact]
    private void TestAnActualCallOutToMinkowskiBecauseWhyDoTestsWorkButApplicationCrashes()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      NFP sheet;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(22D) }), 0, out sheet).Should().BeTrue();
      NFP part;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), 0, out part).Should().BeTrue();

      var frame = Background.getFrame(sheet);

      new Background(A.Fake<IProgressDisplayer>()).Process2(frame, part, MinkowskiCache.Cache).Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveReturnedASheetPlacement()
    {
      this.nestResult.Should().NotBeNull();
    }

    [Fact]
    public void GivenOnePartOnlyThenShouldBeNoMergedLines()
    {
      this.nestResult.mergedLength.Should().Be(0, "there was only one part; no lines to merge possible.");
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
    public void ShouldHaveOnePlacement()
    {
      this.nestResult.UsedSheets.Count.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacement()
    {
      this.nestResult.UsedSheets[0].PartPlacements.Count.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacement2()
    {
      this.nestResult.UsedSheets[0].SheetId.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacement4()
    {
      this.nestResult.UsedSheets[0].PartPlacements.Count.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacementWithExpectedX()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].x.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].y.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].rotation.Should().Be(0);
    }

    //[Fact]
    //public void Should()
    //{ }
  }
}
