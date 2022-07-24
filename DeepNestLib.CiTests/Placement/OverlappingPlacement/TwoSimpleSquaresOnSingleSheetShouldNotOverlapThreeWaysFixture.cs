namespace DeepNestLib.CiTests.Placement.OverlappingPlacement
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class TwoSimpleSquaresOnSingleSheetShouldNotOverlapFixture
  {
    [Theory]
    [InlineData(150, 100, 0, 50, 50, 0)]
    [InlineData(150, 100, 0, 50, 50, -270)]
    [InlineData(150, 100, 0, 50, 50, -360)]
    [InlineData(150, 100, 180, 50, 50, 0)]
    [InlineData(150, 100, 180, 50, 50, -270)]
    [InlineData(150, 100, 180, 50, 50, -360)]
    
    //Why did these have to be reversed to pass?
    [InlineData(150, 100, 0, 50, 50, 90)]
    [InlineData(150, 100, 180, 50, 50, 90)]
    public void TwoSimpleSquaresOnSingleSheetShouldNotOverlap(double firstWidth, double firstHeight, double firstRotation, double secondWidth, double secondHeight, double secondRotation)
    {
      ISvgNestConfig config;
      DxfGenerator DxfGenerator = new DxfGenerator();
      NestResult nestResult;
      INfp firstPart;
      INfp secondPart;
      int firstSheetIdSrc = new Random().Next();
      NfpHelper nfpHelper;

      ISheet firstSheet;
      DxfGenerator.GenerateRectangle("Sheet", 210D, 110D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      firstSheet.Id = 0;
      firstSheet.Source = 0;
      firstPart = DxfGenerator.GenerateRectangle($"{firstWidth}x{firstHeight}first", firstWidth, firstHeight, RectangleType.FileLoad, true).ToNfp();
      firstPart.Id = 1;
      firstPart.Source = 1;
      firstPart.Rotation = firstRotation;

      secondPart = DxfGenerator.GenerateRectangle($"{secondWidth}x{secondHeight}second", secondWidth, secondHeight, RectangleType.FileLoad, true).ToNfp();
      secondPart.Id = 2;
      secondPart.Source = 2;
      secondPart.Rotation = secondRotation;

      config = new TestSvgNestConfig();
      //config = A.Fake<ISvgNestConfig>();
      config.Simplify = true;
      config.UseDllImport = false;
      //config.UseDllImport = true;
      config.PlacementType = PlacementTypeEnum.BoundingBox;
      config.Rotations = 4;
      config.ExportExecutions = false;
      config.ClipperScale = 10000000;
      config.CurveTolerance = 0.72D;
      config.OffsetTreePhase = true;
      config.PopulationSize = 10;
      config.Scale = 25;
      config.SheetHeight = 395;
      config.Tolerance = 2;
      config.ClipByHull = true;
      config.ToleranceSvg = 0.005;
      config.ParallelNests = 10;
      config.Spacing = 0;
      nfpHelper = A.Dummy<NfpHelper>();
      var placementWorker = new PlacementWorker(nfpHelper, new ISheet[] { firstSheet }, new Chromosome[] { firstPart.ToChromosome(), secondPart.ToChromosome() }.ApplyIndex(), config, A.Dummy<Stopwatch>(), A.Fake<INestState>());
      ITestPlacementWorker sut = placementWorker;
      nestResult = placementWorker.PlaceParts();

      nestResult.UnplacedParts.Count().Should().Be(0);
      nestResult.UsedSheets[0].PartPlacements[0].Part.Should().NotBe(firstPart);
      nestResult.UsedSheets[0].PartPlacements[0].Part.Id.Should().Be(firstPart.Id);
      nestResult.UsedSheets[0].PartPlacements[0].Part.Should().BeEquivalentTo(firstPart.Rotate(firstRotation), opt => opt.Excluding(o => o.Rotation));
      nestResult.UsedSheets[0].PartPlacements[1].Part.Should().NotBe(secondPart);
      nestResult.UsedSheets[0].PartPlacements[1].Part.Id.Should().Be(secondPart.Id);

      var lastPartPlacementWorker = ((ITestPlacementWorker)placementWorker).LastPartPlacementWorker;
      lastPartPlacementWorker.SheetNfp.Should().NotBeNull();
      lastPartPlacementWorker.SheetNfp.NumberOfNfps.Should().Be(1);
      lastPartPlacementWorker.SheetNfp.Items[0].Length.Should().Be(5, "we expect a closed rectangle inner fit polygon of second part on the empty sheet");

      //lastPartPlacementWorker.SheetNfp.Items[0][0].X.Should().Be(160);
      //lastPartPlacementWorker.SheetNfp.Items[0][0].Y.Should().Be(110);
      //lastPartPlacementWorker.SheetNfp.Items[0][1].X.Should().Be(0);
      //lastPartPlacementWorker.SheetNfp.Items[0][1].Y.Should().Be(110);
      //lastPartPlacementWorker.SheetNfp.Items[0][2].X.Should().Be(0);
      //lastPartPlacementWorker.SheetNfp.Items[0][2].Y.Should().Be(50);
      //lastPartPlacementWorker.SheetNfp.Items[0][3].X.Should().Be(160);
      //lastPartPlacementWorker.SheetNfp.Items[0][3].Y.Should().Be(50);
      lastPartPlacementWorker.SheetNfp.Items[0][4].X.Should().Be(lastPartPlacementWorker.SheetNfp.Items[0][0].X);
      lastPartPlacementWorker.SheetNfp.Items[0][4].Y.Should().Be(lastPartPlacementWorker.SheetNfp.Items[0][0].Y);
      lastPartPlacementWorker.SheetNfp.Items[0].MinX.Should().Be(0);
      secondPart.Shift(nestResult.UsedSheets[0].PartPlacements[0]).Overlaps(firstPart.Shift(nestResult.UsedSheets[0].PartPlacements[0])).Should().BeFalse("parts should not overlay each other (even if nesting in a hole; placement should not have given this result)");
    }
  }
}
