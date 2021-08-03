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
      DxfGenerator.GenerateSquare("Sheet", 22D, RectangleType.FileLoad).TryConvertToNfp(0, out sheet).Should().BeTrue();
      INfp part;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryConvertToNfp(0, out part).Should().BeTrue();
      this.nestResult = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>()).PlaceParts(new INfp[] { sheet }, new INfp[] { part }, new SvgNestConfig());
    }

    [Fact]
    public void GivenNullSheetsPassedInThenNullReturned()
    {
      new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>()).PlaceParts(null, new NFP[] { new NFP() }, new SvgNestConfig()).Should().BeNull();
    }

    [Fact]
    private void TestAnActualCallOutToMinkowskiBecauseWhyDoTestsWorkButApplicationCrashes()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      INfp sheet;
      DxfGenerator.GenerateSquare("Sheet", 22D, RectangleType.FileLoad).TryConvertToNfp(0, out sheet).Should().BeTrue();
      INfp part;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryConvertToNfp(0, out part).Should().BeTrue();

      new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>()).ExecuteDllImportMinkowski(sheet, part, MinkowskiCache.Cache).Should().NotBeNull();
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
      this.nestResult.Fitness.Should().BeApproximately(2199, 1);
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
      this.nestResult.UsedSheets[0].PartPlacements[0].X.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacementWithExpectedY()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].Y.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveOnePartOnOnePlacementWithExpectedRotation()
    {
      this.nestResult.UsedSheets[0].PartPlacements[0].Rotation.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestResult.UnplacedParts.Should().BeEmpty();
    }
  }
}
