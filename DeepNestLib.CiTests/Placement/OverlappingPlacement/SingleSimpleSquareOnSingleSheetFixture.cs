namespace DeepNestLib.CiTests.Placement.OverlappingPlacement
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.CiTests.GeneticAlgorithm;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class SingleSimpleSquareOnSingleSheetFixture
  {
    internal const double DoublePrecision = 0.0001;

    [Theory]
    [InlineData(150, 100, -450, RectangleType.FileLoad)]
    [InlineData(150, 100, -360, RectangleType.FileLoad)]
    [InlineData(150, 100, -270, RectangleType.FileLoad)]
    [InlineData(150, 100, -180, RectangleType.FileLoad)]
    [InlineData(150, 100, -90, RectangleType.FileLoad)]
    [InlineData(150, 100, 0, RectangleType.FileLoad)]
    [InlineData(150, 100, 90, RectangleType.FileLoad)]
    [InlineData(150, 100, 180, RectangleType.FileLoad)]
    [InlineData(150, 100, 270, RectangleType.FileLoad)]
    [InlineData(150, 100, 360, RectangleType.FileLoad)]
    [InlineData(150, 100, 450, RectangleType.FileLoad)]
    [InlineData(150, 100, -450, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, -360, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, -270, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, -180, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, -90, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, 0, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, 90, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, 180, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, 270, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, 360, RectangleType.BottomLeftClockwise)]
    [InlineData(150, 100, 450, RectangleType.BottomLeftClockwise)]
    public void SingleSimpleSquareOnSingleSheet(double firstWidth, double firstHeight, double firstRotation, RectangleType firstType)
    {
      DxfGenerator DxfGenerator = new DxfGenerator();
      NestResult nestResult;
      INfp firstPart;
      int firstSheetIdSrc = new Random().Next();
      NfpHelper nfpHelper;

      ISheet firstSheet;
      DxfGenerator.GenerateRectangle("Sheet", 210D, 210D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      firstSheet.Id = 0;
      firstSheet.Source = 0;
      firstPart = DxfGenerator.GenerateRectangle($"{firstWidth}x{firstHeight}first", firstWidth, firstHeight, firstType, true).ToNfp();
      firstPart.Id = 1;
      firstPart.Source = 1;
      SvgPoint[] expectationFirstPart = firstType == RectangleType.FileLoad ? new SvgPoint[] {
      new SvgPoint(0,100),
      new SvgPoint(150,100),
      new SvgPoint(150,0),
      new SvgPoint(0,0),
      new SvgPoint(0,100),
      } : new SvgPoint[] {
      new SvgPoint(0,0),
      new SvgPoint(0,100),
      new SvgPoint(150,100),
      new SvgPoint(150,0),
      new SvgPoint(0,0),
      };

      var firstPartOriginal = firstPart.Clone();
      firstPartOriginal.Should().BeEquivalentTo(firstPart, opt => opt.WithStrictOrdering(), "only the placement clone should be altered.");
      var gene = new DeepNestGene(new Chromosome[] { firstPart.ToChromosome(firstRotation) }.ApplyIndex());
      VerifyPartIsNotMutated(firstPart, firstPartOriginal, expectationFirstPart);
      nfpHelper = A.Dummy<NfpHelper>();
      var placementWorker = new PlacementWorker(
        nfpHelper,
        new ISheet[] { firstSheet },
        gene,
        StableButIrrelevantConfig(new Random().NextBool()),
        A.Dummy<Stopwatch>(),
        A.Fake<INestState>());
      VerifyPartIsNotMutated(firstPart, firstPartOriginal, expectationFirstPart);
      ITestPlacementWorker sut = placementWorker;
      nestResult = placementWorker.PlaceParts();
      VerifyPartIsNotMutated(firstPart, firstPartOriginal, expectationFirstPart);


      nestResult.UnplacedParts.Count().Should().Be(0);
      ValidateFirstPlacement(firstRotation, nestResult.UsedSheets[0].PartPlacements[0], firstPart);

      var lastPartPlacementWorker = ((ITestPlacementWorker)placementWorker).LastPartPlacementWorker;
      VerifyLastSheetNfp(firstPartOriginal, lastPartPlacementWorker.SheetNfp);

      var firstPartPlaced = nestResult.UsedSheets[0].PartPlacements[0].PlacedPart;
      firstPartPlaced.MinX.Should().BeApproximately(0, DoublePrecision);
      firstPartPlaced.MinY.Should().BeApproximately(0, DoublePrecision);
      firstPartPlaced.MaxX.Should().BeApproximately(firstPart.Rotate(firstRotation).WidthCalculated, DoublePrecision);
      firstPartPlaced.MaxY.Should().BeApproximately(firstPart.Rotate(firstRotation).HeightCalculated, DoublePrecision);
      firstPartPlaced.Should().BeEquivalentTo(nestResult.UsedSheets[0].PartPlacements[0].PlacedPart);
      //firstPartShifted.Points.Should().BeEquivalentTo(firstPart.Points, 
      //  opt => opt.WithoutStrictOrdering()
      //            .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.0001))
      //            .WhenTypeIs<double>());

    }

    internal static void VerifyLastSheetNfp(INfp partOriginal, SheetNfp sheetNfp)
    {
      sheetNfp.Should().NotBeNull();
      sheetNfp.NumberOfNfps.Should().Be(1);
      sheetNfp.Items[0].Length.Should().Be(5, "we expect a closed rectangle inner fit polygon of part on the empty sheet");
      sheetNfp.Items[0].Area.Should().BeApproximately((sheetNfp.Sheet.WidthCalculated - partOriginal.WidthCalculated) * (sheetNfp.Sheet.HeightCalculated - partOriginal.HeightCalculated), DoublePrecision);
      sheetNfp.Items[0][4].X.Should().Be(sheetNfp.Items[0][0].X);
      sheetNfp.Items[0][4].Y.Should().Be(sheetNfp.Items[0][0].Y);
    }

    internal static void ValidateFirstPlacement(double firstRotation, IPartPlacement partPlacement, INfp firstPart)
    {
      partPlacement.Part.Should().NotBe(firstPart);
      partPlacement.Part.Id.Should().Be(0);
      firstPart.Id.Should().Be(1);
      partPlacement.Part.Should().BeEquivalentTo(firstPart.Rotate(firstRotation),
        opt => opt.Excluding(o => o.Id)
                  .WithStrictOrdering()
                  .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, DoublePrecision))
                  .WhenTypeIs<double>(),
        "may have been rotated but won't be shifted because placed at origin on sheet");
      partPlacement.PlacedPart.Should().BeEquivalentTo(
        firstPart.Rotate(firstRotation)
                 .Shift(partPlacement),
        opt => opt.Excluding(o => o.Id)
                  .WithStrictOrdering()
                  .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, DoublePrecision))
                  .WhenTypeIs<double>(),
        "may have been rotated but won't be shifted because placed at origin on sheet");
      partPlacement.MinX.Should().BeApproximately(0, DoublePrecision);
      partPlacement.MinY.Should().BeApproximately(0, DoublePrecision);
      partPlacement.MaxX.Should().BeApproximately(firstPart.Rotate(firstRotation).WidthCalculated, DoublePrecision);
      partPlacement.MaxY.Should().BeApproximately(firstPart.Rotate(firstRotation).HeightCalculated, DoublePrecision);
      partPlacement.Rotation.Should().Be(firstRotation % 360D);
    }

    /// <summary>
    /// useDllImport was added because it was making a difference in TwoSimpleSquaresOnSingleSheetFixture; where it shouldn't be...
    /// </summary>
    internal static ISvgNestConfig StableButIrrelevantConfig(bool useDllImport)
    {
      ISvgNestConfig config = new TestSvgNestConfig();
      //config = A.Fake<ISvgNestConfig>();
      config.Simplify = true;
      config.UseDllImport = useDllImport;
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
      return config;
    }

    /// <summary>
    /// Verifies that the original part is not mutated by the placement process.
    /// </summary>
    /// <param name="part">The part passed in to the placement that should not be mutated.</param>
    /// <param name="original">A clone of the part before it was passed in, verified.</param>
    /// <param name="expectationPoints">The verified points of the part when it was created.</param>
    internal static void VerifyPartIsNotMutated(INfp part, INfp original, SvgPoint[] expectationPoints)
    {
      part.MinX.Should().Be(0);
      part.MinY.Should().Be(0);
      part.MaxX.Should().Be(original.WidthCalculated);
      part.MaxY.Should().Be(original.HeightCalculated);
      part.Rotation.Should().Be(0, "the original part should not carry the instruction, and will not be mutated");
      part.Points.Should().BeEquivalentTo(
        expectationPoints,
        opt => opt.WithStrictOrdering(),
        "original part should not be altered, only ever the clone");
      original.Should().BeEquivalentTo(
              part,
              opt => opt.WithStrictOrdering(),
              "only the placement clone should be altered.");
    }
  }
}
