﻿namespace DeepNestLib.CiTests
{
  using System.Diagnostics;
  using DeepNestLib.CiTests.IO;
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
      ISheet sheet;
      DxfGenerator.GenerateSquare("Sheet", 22D, RectangleType.FileLoad).TryConvertToSheet(0, out sheet).Should().BeTrue();
      Chromosome part;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryConvertToNfp(0, out part).Should().BeTrue();
      this.nestResult = new PlacementWorker(A.Dummy<NfpHelper>(), new ISheet[] { sheet }, new DeepNestGene(new Chromosome[] { part }), new TestSvgNestConfig(), A.Dummy<Stopwatch>(), A.Fake<INestState>()).PlaceParts();
    }

    [Fact]
    public void GivenNullSheetsPassedInThenNullReturned()
    {
      new PlacementWorker(A.Dummy<NfpHelper>(), null, new DeepNestGene(new Chromosome[] { new Chromosome(new NoFitPolygon(), 0) }), new TestSvgNestConfig(), A.Dummy<Stopwatch>(), A.Fake<INestState>()).PlaceParts().Should().BeNull();
    }

    [Fact]
    private void TestAnActualCallOutToMinkowskiBecauseWhyDoTestsWorkButApplicationCrashes()
    {
      ISheet sheet;
      DxfGenerator.GenerateSquare("Sheet", 22D, RectangleType.FileLoad).TryConvertToSheet(0, out sheet).Should().BeTrue();
      INfp part;
      DxfGenerator.GenerateSquare("Part", 11D, RectangleType.FileLoad).TryConvertToNfp(0, out part).Should().BeTrue();

      A.Dummy<NfpHelper>().ExecuteDllImportMinkowski(sheet, part, MinkowskiCache.Cache, A.Dummy<bool>()).Should().NotBeNull();
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
      this.nestResult.Fitness.Should().BeApproximately(1558, 100);
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
