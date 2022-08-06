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
    private const double DoublePrecision = SingleSimpleSquareOnSingleSheetFixture.DoublePrecision;

    [Theory]
    //Doesn't overlap proofed in real execution
    [InlineData(150, 100, 0, 50, 50, 270)]

    //Zeroed
    [InlineData(150, 100, 0, 50, 50, 0)]
    [InlineData(150, 100, 0, 50, 50, -270)]
    [InlineData(150, 100, 0, 50, 50, -360)]
    [InlineData(150, 100, 0, 50, 50, 360)]

    //Not Zeroed
    [InlineData(150, 100, 180, 50, 50, 0)]
    [InlineData(150, 100, 180, 50, 50, -270)]
    [InlineData(150, 100, 180, 50, 50, -360)]
    [InlineData(150, 100, 180, 50, 50, 270)]

    //Why did these have to be reversed to pass?
    //Zeroed
    [InlineData(150, 100, 0, 50, 50, 90)]

    //Not Zeroed
    [InlineData(150, 100, 180, 50, 50, 90)]
    public void TwoSimpleSquaresOnSingleSheetShouldNotOverlap(double firstWidth, double firstHeight, double firstRotation, double secondWidth, double secondHeight, double secondRotation)
    {
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
      SvgPoint[] expectationFirstPart = new SvgPoint[] {
      new SvgPoint(0,100),
      new SvgPoint(150,100),
      new SvgPoint(150,0),
      new SvgPoint(0,0),
      new SvgPoint(0,100),
      };
      SvgPoint[] expectationSecondPart = new SvgPoint[] {
      new SvgPoint(0,50),
      new SvgPoint(50,50),
      new SvgPoint(50,0),
      new SvgPoint(0,0),
      new SvgPoint(0,50),
      };
      SvgPoint[] expectationShiftedSecondPart = new SvgPoint[] {
      new SvgPoint(50,0),
      new SvgPoint(50,-50),
      new SvgPoint(0,-50),
      new SvgPoint(0,0),
      new SvgPoint(50,0),
      };
      firstPart.Points.Should().BeEquivalentTo(expectationFirstPart, opt => opt.WithStrictOrdering());
      var firstPartOriginal = firstPart.Clone();
      firstPartOriginal.Should().BeEquivalentTo(firstPart, opt => opt.WithStrictOrdering(), "only the placement clone should be altered.");

      secondPart = DxfGenerator.GenerateRectangle($"{secondWidth}x{secondHeight}second", secondWidth, secondHeight, RectangleType.FileLoad, true).ToNfp();
      secondPart.Id = 2;
      secondPart.Source = 2;
      secondPart.Points.Should().BeEquivalentTo(expectationSecondPart, opt => opt.WithStrictOrdering());
      var secondPartOriginal = secondPart.Clone();
      secondPartOriginal.Should().BeEquivalentTo(secondPart, opt => opt.WithStrictOrdering(), "only the placement clone should be altered.");
      var gene = new Gene(new Chromosome[] { firstPart.ToChromosome(firstRotation), secondPart.ToChromosome(secondRotation) }.ApplyIndex());
      VerifyPartIsNotMutated(firstPart, firstPartOriginal, expectationFirstPart);
      VerifyPartIsNotMutated(secondPart, secondPartOriginal, expectationSecondPart);

      nfpHelper = A.Dummy<NfpHelper>();
      var placementWorker = new PlacementWorker(
        nfpHelper,
        new ISheet[] { firstSheet },
        gene,
        StableButIrrelevantConfig(new Random().NextBool()),
        A.Dummy<Stopwatch>(),
        A.Fake<INestState>());
      ITestPlacementWorker sut = placementWorker;
      nestResult = placementWorker.PlaceParts();
      VerifyPartIsNotMutated(firstPart, firstPartOriginal, expectationFirstPart);
      VerifyPartIsNotMutated(secondPart, secondPartOriginal, expectationSecondPart);

      nestResult.UnplacedParts.Count().Should().Be(0);
      ValidateFirstPlacement(firstRotation, nestResult.UsedSheets[0].PartPlacements[0], firstPart);
      ValidateSecondPlacement(secondRotation, nestResult.UsedSheets[0].PartPlacements[1], firstPart, secondPart);

      //firstPart = firstPart.Shift(nestResult.UsedSheets[0].PartPlacements[0]);
      //secondPart = secondPart.Shift(nestResult.UsedSheets[0].PartPlacements[1]);

      var lastPartPlacementWorker = ((ITestPlacementWorker)placementWorker).LastPartPlacementWorker;
      VerifyLastSheetNfp(secondPartOriginal, lastPartPlacementWorker.SheetNfp);

      nestResult.UsedSheets[0].PartPlacements[1].PlacedPart.Overlaps(nestResult.UsedSheets[0].PartPlacements[0].PlacedPart)
                .Should()
                .BeFalse("parts should not overlay each other (even if nesting in a hole; placement should not have given this result)");
    }

    private static void VerifyLastSheetNfp(INfp partOriginal, SheetNfp sheetNfp)
    {
      SingleSimpleSquareOnSingleSheetFixture.VerifyLastSheetNfp(partOriginal, sheetNfp);
    }

    internal static void ValidateSecondPlacement(double secondRotation, IPartPlacement partPlacement, INfp firstPart, INfp secondPart)
    {
      partPlacement.Part.Should().NotBe(secondPart);
      partPlacement.Part.Id.Should().Be(1);
      secondPart.Id.Should().Be(2);
      partPlacement.Part.Should().BeEquivalentTo(secondPart.Rotate(secondRotation),
        opt => opt.Excluding(o=>o.Id)
                  .WithStrictOrdering()
                  .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, DoublePrecision))
                  .WhenTypeIs<double>(),
        "may have been rotated but won't be shifted because placed at origin on sheet");
      if (firstPart.Children.Count == 0)
      {
        //The original usage of this placed the second next to the first; there was no hole
        partPlacement.MinX.Should().BeApproximately(firstPart.WidthCalculated, DoublePrecision);
        partPlacement.MinY.Should().BeApproximately(0, DoublePrecision);
        partPlacement.MaxX.Should().BeApproximately(firstPart.WidthCalculated + secondPart.Rotate(secondRotation).WidthCalculated, DoublePrecision);
        partPlacement.MaxY.Should().BeApproximately(secondPart.Rotate(secondRotation).HeightCalculated, DoublePrecision);
      }
      else
      {
        //The second usage of this placed the second in the hole in the first
        partPlacement.MinX.Should().BeApproximately(firstPart.WidthCalculated - firstPart.Children[0].WidthCalculated, DoublePrecision, "not convinced; shouldn't it be only offset half the difference?");
        //Hmm... how to determine this? partPlacement.MinY.Should().BeApproximately(firstPart.HeightCalculated - firstPart.Children[0].HeightCalculated, DoublePrecision);
        partPlacement.MaxX.Should().BeApproximately(partPlacement.MinX + secondPart.Rotate(secondRotation).WidthCalculated, DoublePrecision);
        partPlacement.MaxY.Should().BeApproximately(partPlacement.MinY + secondPart.Rotate(secondRotation).HeightCalculated, DoublePrecision);
      }

      partPlacement.Rotation.Should().Be(secondRotation % 360);
    }

    private static void ValidateFirstPlacement(double firstRotation, IPartPlacement partPlacement, INfp firstPart)
    {
      SingleSimpleSquareOnSingleSheetFixture.ValidateFirstPlacement(firstRotation, partPlacement, firstPart);
    }

    private static ISvgNestConfig StableButIrrelevantConfig(bool useDllImport)
    {
      return SingleSimpleSquareOnSingleSheetFixture.StableButIrrelevantConfig(useDllImport);
    }

    private void VerifyPartIsNotMutated(INfp part, INfp original, SvgPoint[] expectationFirstPart)
    {
      SingleSimpleSquareOnSingleSheetFixture.VerifyPartIsNotMutated(part, original, expectationFirstPart);
    }
  }
}
