namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class FitSmallSquarePartInLargerSquareSheetFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private NestResult nestResult;

    public FitSmallSquarePartInLargerSquareSheetFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      INfp sheet;
      DxfGenerator.GenerateSquare("Sheet", 22D, RectangleType.FileLoad).TryImportFromRawDetail(0, out sheet).Should().BeTrue();
      INfp part;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryImportFromRawDetail(0, out part).Should().BeTrue();
      this.nestResult = new Background(A.Fake<IProgressDisplayer>(), null).PlaceParts(new INfp[] { sheet }, new INfp[] { part }, new SvgNestConfig());
    }

    [Fact]
    public void GivenNullSheetsPassedInThenNullReturned()
    {
      new Background(A.Fake<IProgressDisplayer>(), null).PlaceParts(null, new NFP[] { new NFP() }, new SvgNestConfig()).Should().BeNull();
    }

    [Fact]
    private void TestAnActualCallOutToMinkowskiBecauseWhyDoTestsWorkButApplicationCrashes()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      INfp sheet;
      DxfGenerator.GenerateSquare("Sheet", 22D, RectangleType.FileLoad).TryImportFromRawDetail(0, out sheet).Should().BeTrue();
      INfp part;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryImportFromRawDetail(0, out part).Should().BeTrue();

      var frame = Background.getFrame(sheet);

      new Background(A.Fake<IProgressDisplayer>(), null).ExecuteDllImportMinkowski(frame, part, MinkowskiCache.Cache).Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveReturnedASheetPlacement()
    {
      this.nestResult.Should().NotBeNull();
    }

    [Fact]
    public void GivenOnePartOnlyThenShouldBeNoMergedLines()
    {
      this.nestResult.MergedLength.Should().Be(0, "there was only one part; no lines to merge possible.");
    }

    [Fact]
    public void ShouldHaveExpectedFitness()
    {
      this.nestResult.Fitness.Should().BeApproximately(1838, 1);
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

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }
  }

  public class PerfectFitnessFixture
  {
    float width;
    float height;
    float area;
    INfp sheet;
    ISheetPlacement sp;

    public PerfectFitnessFixture()
    {
      width = new Random().Next(50, 1200);
      height = new Random().Next(50, 900);
      area = width * height;
      sheet = A.Fake<INfp>();
      A.CallTo(() => sheet.Area).Returns(area);
      sp = A.Fake<ISheetPlacement>();
      A.CallTo(() => sp.Sheet).Returns(sheet);
      A.CallTo(() => sp.MaterialUtilization).Returns(1);
      A.CallTo(() => sp.RectBounds).Returns(new PolygonBounds(0, 0, width, height));
      A.CallTo(() => sp.Hull).Returns(sheet);
      A.CallTo(() => sp.TotalPartsArea).Returns(area);
    }

    [Fact]
    public void GivenPerfectFitThenFitnessShouldBeApproximatelyExpected()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Evaluate().Should().BeLessThan(area * 2);
    }

    [Fact]
    public void GivenPerfectFitThenMaterialUtilizationShouldBeZero()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.MaterialUtilization.Should().Be(0);
    }

    [Fact]
    public void GivenPerfectFitWhenNoPrimaryThenSheetsShouldBeArea()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Sheets.Should().Be(area);
    }

    [Fact]
    public void GivenPerfectFitThenMaterialWastedShouldBeZero()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.MaterialWasted.Should().BeApproximately(0, 1);
    }

    [Fact]
    public void GivenPerfectFitThenBoundsShouldBeExpected()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Bounds.Should().BeApproximately(area / 3, area / 6);
    }
  }
}
