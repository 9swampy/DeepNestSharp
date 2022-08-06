namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.CiTests.Placement.OverlappingPlacement;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class NestSquareInSquareHoleFixture
  {
    [Theory]
    [InlineData(0, 0, 0, 0, 20, 20)]
    [InlineData(0, 0, 0, -360, 20, 20)]
    [InlineData(0, 0, 0, -90, 20, 30)]
    [InlineData(0, 0, 0, -270, 30, 20)]

    [InlineData(-180, 40, 40, 0, 20, 20)]
    [InlineData(-180, 40, 40, -90, 20, 30)]
    [InlineData(-180, 40, 40, -270, 30, 20)]
    [InlineData(-180, 40, 40, -360, 20, 20)]

    [InlineData(-270, 40, 0, 0, 20, 20)]
    [InlineData(-270, 40, 0, -90, 20, 30)]
    [InlineData(-270, 40, 0, -270, 30, 20)]
    [InlineData(-270, 40, 0, -360, 20, 20)]

    [InlineData(-360, 0, 0, 0, 20, 20)]
    [InlineData(-360, 0, 0, -360, 20, 20)]
    [InlineData(-360, 0, 0, -90, 20, 30)]
    [InlineData(-360, 0, 0, -270, 30, 20)]

    //Invalid results; second placements should only ever be 20 or 30?
    [InlineData(-90, 0, 40, -0, 20, 14)]
    [InlineData(-90, 0, 40, -90, 20, 24)]
    [InlineData(-90, 0, 40, -270, 30, 14)]
    [InlineData(-90, 0, 40, -360, 20, 14)]
    public void NestSquareInSquareHoleShouldBeObserved(double firstRotation, double firstX, double firstY, double secondRotation, double secondX, double secondY)
    {
      ISvgNestConfig config;
      DxfGenerator DxfGenerator = new DxfGenerator();
      NestResult nestResult;
      INfp firstPart;
      INfp secondPart;
      int firstSheetIdSrc = new Random().Next();
      NfpHelper nfpHelper;
      ISheet firstSheet;
      DxfGenerator.GenerateRectangle("Sheet", 60D, 60D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      firstSheet.Id = 0;
      firstSheet.Source = 0;

      firstPart = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(40) }));
      firstPart.Children.Add(SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(20) })).Shift(10, 10));
      firstPart.Id = 1;
      firstPart.Source = 1;

      secondPart = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
      secondPart.Id = 2;
      secondPart.Source = 2;

      config = SingleSimpleSquareOnSingleSheetFixture.StableButIrrelevantConfig(false); //new Random().NextBool()); //Using DllImport fixes the odd 6es
      nfpHelper = A.Dummy<NfpHelper>();
      var placementWorker = new PlacementWorker(nfpHelper, new ISheet[] { firstSheet }, new Gene(new Chromosome[] { firstPart.ToChromosome(firstRotation), secondPart.ToChromosome(secondRotation) }.ApplyIndex()), config, A.Dummy<Stopwatch>(), A.Fake<INestState>());
      ITestPlacementWorker sut = placementWorker;
      nestResult = placementWorker.PlaceParts();

      nestResult.UnplacedParts.Count().Should().Be(0);
      nestResult.UsedSheets[0].PartPlacements.Count().Should().Be(2);
      SingleSimpleSquareOnSingleSheetFixture.ValidateFirstPlacement(firstRotation, nestResult.UsedSheets[0].PartPlacements[0], firstPart);
      nestResult.UsedSheets[0].PartPlacements[0].X.Should().BeApproximately(firstX, 0.01);
      nestResult.UsedSheets[0].PartPlacements[0].Y.Should().BeApproximately(firstY, 0.01);

      TwoSimpleSquaresOnSingleSheetShouldNotOverlapFixture.ValidateSecondPlacement(secondRotation, nestResult.UsedSheets[0].PartPlacements[1], firstPart, secondPart);
      nestResult.UsedSheets[0].PartPlacements[1].X.Should().BeApproximately(secondX, 0.01);
      nestResult.UsedSheets[0].PartPlacements[1].Y.Should().BeApproximately(secondY, 0.01);
      nestResult.IsValid.Should().BeTrue();
    }
  }
}
