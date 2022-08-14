namespace DeepNestLib.CiTests.Placement.OverlappingPlacement
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.CiTests.GeneticAlgorithm;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class FixBothPartsFitOnASingleSheetFixture
  {
    private readonly ISvgNestConfig config;
    private readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private readonly NestResult nestResult;
    private readonly INfp firstPart;
    private readonly INfp secondPart;
    private readonly int firstSheetIdSrc = new Random().Next();
    private readonly int secondSheetIdSrc = new Random().Next();
    private readonly int firstPartIdSrc = new Random().Next();
    private readonly int secondPartIdSrc = new Random().Next();
    private readonly ITestPlacementWorker sut;
    private readonly NfpHelper nfpHelper;

    public FixBothPartsFitOnASingleSheetFixture()
    {
      ISheet firstSheet;
      DxfGenerator.GenerateRectangle("Sheet", 160D, 110D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      firstSheet.Id = 0;
      ISheet secondSheet;
      DxfGenerator.GenerateRectangle("Sheet", 160D, 110D, RectangleType.FileLoad).TryConvertToSheet(secondSheetIdSrc, out secondSheet).Should().BeTrue();
      secondSheet.Id = 1;
      firstPart = DxfParser.LoadDxfFileStreamAsNfp("Dxfs._9.dxf");
      firstPart.Source = firstPartIdSrc;
      //secondPart = DxfParser.LoadDxfStream("Dxfs._1.dxf").ToNfp();
      //var dxfFile = DxfGenerator.GenerateRectangle(150, 100, RectangleType.FileLoad, true);
      //dxfFile.Save(@"C:\temp\150x100.dxf");
      secondPart = DxfGenerator.GenerateSquare("50x50gen", 50, RectangleType.FileLoad, true).ToNfp();
      secondPart.Source = secondPartIdSrc;
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
      var placementWorker = new PlacementWorker(nfpHelper, new ISheet[] { firstSheet, secondSheet }, new DeepNestGene(new Chromosome[] { firstPart.ToChromosome(0), secondPart.ToChromosome(-90) }.ApplyIndex()), config, A.Dummy<Stopwatch>(), A.Fake<INestState>());
      sut = placementWorker;
      nestResult = placementWorker.PlaceParts();
      //config.Should().BeEquivalentTo(new TestSvgNestConfig());
    }

    #region WithoutPartPlacementWorkerIsPositionValidCheck
    //[Fact]
    //public void LastPartPlacementWorkerSheetNfpShouldBeExpected()
    //{
    //  //System.IO.File.WriteAllText(@"C:\Temp\FixBothPartsFitOnASingleSheetFixtureSheetNfp.dnsnfp", sut.LastPartPlacementWorker.SheetNfp.ToJson());
    //  using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.OverlappingPlacement.FixBothPartsFitOnASingleSheetFixtureSheetNfp.dnsnfp"))
    //  {
    //    sut.LastPartPlacementWorker.SheetNfp.Should().BeEquivalentTo(SheetNfp.LoadFromStream(stream), opt =>
    //    opt.Excluding(o => o.Name == "Source")
    //       .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
    //       .WhenTypeIs<double>());
    //  }

    //  //Part has about 24,85 offset from origin?
    //  //SheetNfp is for the second part 50x50 on a clear sheet, 50 gap from left and top => 50,50 -> 160,110 vertices

    //  //ClipperSheetNfp is just the same again in clipper coords: "polygon ([\r\n[1600000000,1100000000],\r\n[499999981,1100000000],\r\n[499999981,499999981],\r\n[1600000000,499999981],\r\n[1600000000,1100000000],\r\n]);\r\n"
    //}

    //[Fact]
    //public void LastPartPlacementWorkerClipperSheetNfpShouldBeExpected()
    //{
    //  var outerNfp = nfpHelper.GetOuterNfp(firstPart, secondPart, MinkowskiCache.Cache, config.UseDllImport);
    //  System.IO.File.WriteAllText(@"C:\Temp\FixBothPartsFitOnASingleSheetFixtureOuterNfp0AddedToCombinedNfp.dnpoly", outerNfp.ToJson());
    //  // This is just exactly the first part placed; 100 tall 150 wide; don't see what the second part has to do with what's generated.
    //  // It'll get shifted 0,0 by the placement.X and Y and has no children. If it had children they'd get shifted same amount, then whole
    //  // Nfp is converted to Clipper Coords and added to the clipper already passed in (as a subject) with sheetnfp on it (as a subject)
    //}

    //[Fact]
    //public void LastPartPlacementWorkerCombinedNfpShouldBeExpected()
    //{
    //  //System.IO.File.WriteAllText(@"C:\Temp\FixBothPartsFitOnASingleSheetFixtureCombinedNfp.scad", sut.LastPartPlacementWorker.CombinedNfp.ToOpenScadPolygon());
    //  using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.OverlappingPlacement.FixBothPartsFitOnASingleSheetFixtureCombinedNfp.scad"))
    //  {
    //    using (StreamReader reader = new StreamReader(stream))
    //    {
    //      var openScad = reader.ReadToEnd();
    //      sut.LastPartPlacementWorker.CombinedNfp.ToOpenScadPolygon().Should().BeEquivalentTo(openScad);
    //    }
    //  }

    //  // If I generate without the AllPoints intersect check in then two parts end up placing on the first sheet.
    //  // This is the CombinedNfp added to the sln and it shows same as SheetNfp... in Clipper coords ofc

    //  // If I run with the AllPoints intersect check in then it gets blocked from the first sheet but loads on the
    //  // second as a first part so no CombinedNfp is generated.
    //}

    //[Fact]
    //public void LastPartPlacementWorkerFinalNfpShouldBeExpected()
    //{
    //  System.IO.File.WriteAllText(@"C:\Temp\FixBothPartsFitOnASingleSheetFixtureFinalNfp.scad", sut.LastPartPlacementWorker.FinalNfp.Items.ToOpenScadPolygon());
    //  System.IO.File.WriteAllText(@"C:\Temp\FixBothPartsFitOnASingleSheetFixtureSheetPlacement.dnsp", nestResult.UsedSheets[0].ToJson());
    //  using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.OverlappingPlacement.FixBothPartsFitOnASingleSheetFixtureFinalNfp.scad"))
    //  {
    //    using (StreamReader reader = new StreamReader(stream))
    //    {
    //      var openScad = reader.ReadToEnd();
    //      sut.LastPartPlacementWorker.FinalNfp.Items.ToOpenScadPolygon().Should().BeEquivalentTo(openScad);
    //    }
    //  }

    //  // If I generate without the AllPoints intersect check in then two parts end up placing on the first sheet.
    //  // This is the FinalNfp added to the sln and it is the union of the two Nfps...
    //  //        - the SheetNfp (inner nfp of secondPart on the empty sheet) and
    //  //        - the CombinedNfp

    //  // If I run with the AllPoints intersect check in then it gets blocked from the first sheet but loads on the
    //  // second as a first part so no FinalNfp is generated.
    //}
    #endregion

    [Fact]
    public void ShouldHaveReturnedASheetPlacement()
    {
      nestResult.Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveExpectedFitness()
    {
      nestResult.FitnessSheets.Should().BeApproximately(35200, 1);
      nestResult.FitnessWastage.Should().BeApproximately(9250, 750);
      nestResult.MaterialUtilization.Should().BeApproximately(1, 1);
      nestResult.UsedSheets.Count.Should().Be(2);
      nestResult.UsedSheets[0].PlacementType.Should().Be(PlacementTypeEnum.BoundingBox);
      nestResult.UsedSheets[0].RectBounds.X.Should().Be(0);
      nestResult.UsedSheets[0].RectBounds.Y.Should().BeApproximately(0, 1);
      nestResult.UsedSheets[0].Hull.Area.Should().BeApproximately(firstPart.WidthCalculated * firstPart.HeightCalculated, 100);
      nestResult.UsedSheets[0].RectBounds.Width.Should().BeApproximately(firstPart.WidthCalculated, 10);
      nestResult.UsedSheets[0].RectBounds.Height.Should().BeApproximately(firstPart.HeightCalculated, 10);
      nestResult.UsedSheets[0].Fitness.Bounds.Should().BeApproximately(5000, 100);
      nestResult.FitnessTotal.Should().BeGreaterThan(65000);
    }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      nestResult.Rotation.Should().BeNull();
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedId()
    {
      nestResult.UsedSheets[0].SheetId.Should().Be(0);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedSource()
    {
      nestResult.UsedSheets[0].SheetSource.Should().Be(firstSheetIdSrc, "this is the first sheet in the single nest result");
    }

    [Fact]
    public void ShouldHaveFirstPartOnFirstPlacementWithExpectedX()
    {
      nestResult.UsedSheets[0].PartPlacements[0].X.Should().BeApproximately(0, 0.01, "bottom left");
    }

    [Fact]
    public void ShouldHaveFirstPartOnFirstPlacementWithExpectedY()
    {
      nestResult.UsedSheets[0].PartPlacements[0].Y.Should().BeApproximately(0, 0.01, "bottom left");
    }

    [Fact]
    public void ShouldHaveFirstPartOnFirstPlacementWithExpectedRotation()
    {
      nestResult.UsedSheets[0].PartPlacements[0].Rotation.Should().Be(0);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      nestResult.UnplacedParts.Count().Should().Be(0);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMaxXThenShouldBeExpected()
    {
      nestResult.UsedSheets[0].MaxX.Should().BeApproximately(firstPart.WidthCalculated, 0.1);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMaxYThenShouldBeExpected()
    {
      nestResult.UsedSheets[0].MaxY.Should().BeApproximately(firstPart.HeightCalculated, 0.1);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMinXThenShouldBeExpected()
    {
      nestResult.UsedSheets[0].MinX.Should().BeApproximately(0, 0.1);
    }

    [Fact]
    public void GivenSimpleSheetPlacementWhenGetMinYThenShouldBeExpected()
    {
      nestResult.UsedSheets[0].MinY.Should().BeApproximately(0, 0.1);
    }

    [Fact]
    public void ShouldHaveFirstSheetWithOneOnIt()
    {
      //This should be the first test to go wrong when an overlay occurs.
      nestResult.UsedSheets[0].PartPlacements.Count.Should().Be(1, "there is one part on each sheet");
    }

    [Fact]
    public void ShouldHaveOneNestResultWithTwoSheets()
    {
      nestResult.UsedSheets.Count.Should().Be(2);
    }

    [Fact]
    public void ShouldHaveSecondSheetWithOneOnIt()
    {
      nestResult.UsedSheets[1].PartPlacements.Count.Should().Be(1, "there is one part on each sheet");
    }

    [Fact]
    public void SecondSheetShouldHaveExpectedId()
    {
      nestResult.UsedSheets[1].SheetId.Should().Be(1);
    }

    [Fact]
    public void SecondSheetShouldHaveExpectedSource()
    {
      nestResult.UsedSheets[1].SheetSource.Should().Be(secondSheetIdSrc, "this is the second sheet in the single nest result");
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedRotation()
    {
      nestResult.UsedSheets[1].PartPlacements[0].Rotation.Should().Be(-90);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedX()
    {
      nestResult.UsedSheets[1].PartPlacements[0].X.Should().BeApproximately(0, 0.01);
    }

    [Fact]
    public void ShouldHaveOnePartOnSecondPlacementWithExpectedY()
    {
      nestResult.UsedSheets[1].PartPlacements[0].Y.Should().BeApproximately(50, 0.01, "the part origin is offset?");
    }
  }
}
