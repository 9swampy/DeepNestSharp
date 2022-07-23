namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
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
      firstPart.Rotation = firstRotation;

      secondPart = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
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
      nfpHelper = A.Dummy<NfpHelper>();
      var placementWorker = new PlacementWorker(nfpHelper, new ISheet[] { firstSheet }, new Chromosome[] { firstPart.ToChromosome(), secondPart.ToChromosome() }.ApplyIndex(), config, A.Dummy<Stopwatch>(), A.Fake<INestState>());
      ITestPlacementWorker sut = placementWorker;
      nestResult = placementWorker.PlaceParts();

      nestResult.UnplacedParts.Count().Should().Be(0);
      nestResult.UsedSheets[0].PartPlacements.Count().Should().Be(2);
      nestResult.UsedSheets[0].PartPlacements[0].Part.Should().NotBe(firstPart);
      nestResult.UsedSheets[0].PartPlacements[0].Part.Id.Should().Be(firstPart.Id);
      nestResult.UsedSheets[0].PartPlacements[0].Rotation.Should().Be(firstPart.Rotation);
      nestResult.UsedSheets[0].PartPlacements[0].X.Should().BeApproximately(firstX, 0.01);
      nestResult.UsedSheets[0].PartPlacements[0].Y.Should().BeApproximately(firstY, 0.01);
      nestResult.UsedSheets[0].PartPlacements[0].Part.Should().BeEquivalentTo(firstPart.Rotate(nestResult.UsedSheets[0].PartPlacements[0].Rotation)
                                                                                       //.Shift(nestResult.UsedSheets[0].PartPlacements[0])
                                                                                       , opt =>
        opt.Excluding(o => o.Rotation)
           .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.01))
           .WhenTypeIs<double>());
      nestResult.UsedSheets[0].PartPlacements[1].Part.Id.Should().Be(secondPart.Id);
      nestResult.UsedSheets[0].PartPlacements[1].X.Should().BeApproximately(secondX, 0.01);
      nestResult.UsedSheets[0].PartPlacements[1].Y.Should().BeApproximately(secondY, 0.01);
      nestResult.UsedSheets[0].PartPlacements[1].Part.Should().BeEquivalentTo(secondPart.Rotate(nestResult.UsedSheets[0].PartPlacements[1].Rotation)
                                                                                        //.Shift(nestResult.UsedSheets[0].PartPlacements[1])
                                                                                        , opt =>
        opt.Excluding(o => o.Rotation)
           .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.01))
           .WhenTypeIs<double>());
      nestResult.IsValid.Should().BeTrue();
    }
  }
}
