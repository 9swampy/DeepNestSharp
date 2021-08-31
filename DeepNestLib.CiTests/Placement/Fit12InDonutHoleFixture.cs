namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using ClipperLib;
  using DeepNestLib.CiTests;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class Fit12InDonutHoleFixture
  {
    private int scaler = 10000000;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenPartCanBePlacedWhenProcessPartThenSheetNfpCapturesCanBePlaced(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out placementWorker);

      sut.ProcessPart(sut.InputPart, 1);

      sut.SheetNfp.CanAcceptPart.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSheetNfpComparesToEmptySheetWhenProcessPartThenOnlyOneNfp(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out placementWorker);

      sut.ProcessPart(sut.InputPart, 1);

      sut.SheetNfp.NumberOfNfps.Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenFinalNfpComparesToSheetWithDonutWhenProcessPartThenTwoNfp(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out placementWorker);

      sut.ProcessPart(sut.InputPart, 1);

      sut.FinalNfp.NumberOfNfps.Should().Be(2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenPartFitsWhenProcessPartThenShouldCallBackToAddPlacement(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      var partPlacement = GetPartPlacementForFit12InDonutHole(useDllImport, out sut, out placementWorker);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, partPlacement, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenPartFitsWhenProcessPartThenShouldPlaceAtExpectedPositionX(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      var partPlacement = GetPartPlacementForFit12InDonutHole(useDllImport, out sut, out placementWorker);

      partPlacement.X.Should().BeApproximately(55.79, 0.01);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenPartFitsWhenProcessPartThenShouldPlaceAtExpectedPositionY(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      var partPlacement = GetPartPlacementForFit12InDonutHole(useDllImport, out sut, out placementWorker);

      partPlacement.Y.Should().BeApproximately(59.56, 0.01);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenMinkowskiDllImportWhenProcessPartProcessToAddPlacement(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      var partPlacement = GetPartPlacementForFit12InDonutHole(useDllImport, out sut, out placementWorker);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, partPlacement, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    private static PartPlacement GetPartPlacementForFit12InDonutHole(bool useDllImport, out PartPlacementWorker sut, out IPlacementWorker placementWorker)
    {
      IPlacementWorker localPlacementWorker;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out localPlacementWorker);
      var inputPart = sut.InputPart;
      var partPlacementArg = new Capture<PartPlacement>();
      A.CallTo(() => localPlacementWorker.AddPlacement(inputPart, A<List<IPartPlacement>>._, A<INfp>._, partPlacementArg, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).Returns(default);

      sut.ProcessPart(sut.InputPart, 0);

      placementWorker = localPlacementWorker;
      return partPlacementArg.Value;
    }

    private static void SetupFit12InDonutHolePartPlacementWorker(bool useDllImport, out PartPlacementWorker sut, out IPlacementWorker placementWorker)
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.Fit12InDonutHoleIn.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      sut = PartPlacementWorker.FromJson(json);
      var config = sut.Config;
      config.Rotations = 1;
      var dispatcherService = A.Fake<IDispatcherService>();
      A.CallTo(() => dispatcherService.InvokeRequired).Returns(false);

      ((ITestPartPlacementWorker)sut).State = new NestState(config, dispatcherService);
      ((ITestPartPlacementWorker)sut).ExportExecutions = false;

      placementWorker = A.Fake<IPlacementWorker>();
      ((ITestPartPlacementWorker)sut).PlacementWorker = placementWorker;

      // var nfpHelper = new NfpHelper();
      // ((ITestNfpHelper)nfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>());
      // ((ITestNfpHelper)nfpHelper).UseDllImport = useDllImport;

      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(config, A.Fake<INestStateMinkowski>());
      //((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).UseDllImport = useDllImport;
    }

    [Fact]
    public void GivenWorksInPortWhenOrigExecuteMinkowskiThenGet2Solutions()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.pathScaledUpListOrig.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var options = new System.Text.Json.JsonSerializerOptions();
      options.IncludeFields = true;
      var pathScaledUpList = System.Text.Json.JsonSerializer.Deserialize<List<List<IntPoint>>>(json, options);
      pathScaledUpList.Count.Should().Be(2);
      pathScaledUpList[0].Count.Should().Be(4);
      var n = pathScaledUpList[0].ToArray().ToNestCoordinates(scaler);
      var sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().Be(40000);
      n = pathScaledUpList[1].ToArray().ToNestCoordinates(scaler);
      sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().Be(48400);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.patternScaledUpOrig.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var patternScaledUp = System.Text.Json.JsonSerializer.Deserialize<List<IntPoint>>(json, options);
      patternScaledUp.Count.Should().Be(29);
      patternScaledUp[10].X.Should().Be(-173960648L);
      patternScaledUp[10].Y.Should().Be(288437271L);

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(patternScaledUp), new List<List<IntPoint>>(pathScaledUpList.Select(pointsArray => pointsArray.ToList())), true);

      solution.Count.Should().Be(2);
    }

    [Fact]
    public void GivenSharpPartWithOrigSheetWorksExecuteMinkowskiThenGet2Solutions()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.pathScaledUpListOrig.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var options = new System.Text.Json.JsonSerializerOptions();
      options.IncludeFields = true;
      var pathScaledUpList = System.Text.Json.JsonSerializer.Deserialize<List<List<IntPoint>>>(json, options);
      pathScaledUpList.Count.Should().Be(2);
      pathScaledUpList[0].Count.Should().Be(4);
      var n = pathScaledUpList[0].ToArray().ToNestCoordinates(scaler);
      var sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().Be(40000);
      n.Area.Should().Be(40000);
      n.X.Should().Be(0);
      n.Y.Should().Be(0);
      n.WidthCalculated.Should().Be(200);
      n.HeightCalculated.Should().Be(200);
      n = pathScaledUpList[1].ToArray().ToNestCoordinates(scaler);
      sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().Be(48400);
      n.Area.Should().Be(48400);
      n.X.Should().Be(0);
      n.Y.Should().Be(0);
      n.WidthCalculated.Should().Be(220);
      n.HeightCalculated.Should().Be(220);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.patternScaledUp.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var patternScaledUp = System.Text.Json.JsonSerializer.Deserialize<List<IntPoint>>(json, options);
      patternScaledUp.Count.Should().Be(46);
      patternScaledUp[10].X.Should().Be(-220905000L);
      patternScaledUp[10].Y.Should().Be(-765603000L);

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(patternScaledUp), new List<List<IntPoint>>(pathScaledUpList.Select(pointsArray => pointsArray.ToList())), true);

      solution.Count.Should().Be(2);
    }

    [Fact]
    public void GivenSharpPartAndSheetShouldWorkExecuteMinkowskiThenGet2Solutions()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.pathScaledUpList.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var options = new System.Text.Json.JsonSerializerOptions();
      options.IncludeFields = true;
      var pathScaledUpList = System.Text.Json.JsonSerializer.Deserialize<List<List<IntPoint>>>(json, options);
      pathScaledUpList.Count.Should().Be(2);
      pathScaledUpList[0].Count.Should().Be(4);
      var n = pathScaledUpList[0].ToArray().ToNestCoordinates(scaler);
      var sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().Be(31360);
      n.Area.Should().Be(31360);
      n.X.Should().Be(0);
      n.Y.Should().Be(0);
      n.WidthCalculated.Should().Be(160);
      n.HeightCalculated.Should().Be(196);
      n = pathScaledUpList[1].ToArray().ToNestCoordinates(scaler);
      sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().BeApproximately(37945.60, 0.01);
      n.Area.Should().BeApproximately(37945.60, 0.01);
      n.X.Should().Be(0);
      n.Y.Should().Be(0);
      n.WidthCalculated.Should().BeApproximately(176, 0.01);
      n.HeightCalculated.Should().BeApproximately(215.6, 0.01);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.patternScaledUp.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var patternScaledUp = System.Text.Json.JsonSerializer.Deserialize<List<IntPoint>>(json, options);
      patternScaledUp.Count.Should().Be(46);
      patternScaledUp[10].X.Should().Be(-220905000L);
      patternScaledUp[10].Y.Should().Be(-765603000L);
      n = patternScaledUp.ToArray().ToNestCoordinates(scaler);
      sarea = Math.Abs(GeometryUtil.PolygonArea(n));
      sarea.Should().BeApproximately(14750.11638, 0.01);
      n.WidthCalculated.Should().BeApproximately(159.338, 0.01);
      n.HeightCalculated.Should().BeApproximately(156.229, 0.01);

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(patternScaledUp), new List<List<IntPoint>>(pathScaledUpList.Select(pointsArray => pointsArray.ToList())), true);

      solution.Count.Should().Be(2);
    }

    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [Theory]
    public void GivenSerialisedScenariosWhenClipperMinkowskiSumExpect2SolutionsForEach(bool useOriginal, bool pathIsClosed)
    {
      string pathScaledUpResourcePath;
      string patternScaledUpResourcePath;
      if (useOriginal)
      {
        pathScaledUpResourcePath = "Placement.pathScaledUpListOrig.json";
        patternScaledUpResourcePath = "Placement.patternScaledUpOrig.json";
      }
      else
      {
        pathScaledUpResourcePath = "Placement.pathScaledUpList.json";
        patternScaledUpResourcePath = "Placement.patternScaledUp.json";
      }

      var options = new System.Text.Json.JsonSerializerOptions();
      options.IncludeFields = true;
      var pathScaledUpList = System.Text.Json.JsonSerializer.Deserialize<List<List<IntPoint>>>(LoadJson(pathScaledUpResourcePath), options);
      // pathScaledUpList = new List<List<IntPoint>>() { pathScaledUpList.Skip(1).First() };
      var patternScaledUp = System.Text.Json.JsonSerializer.Deserialize<List<IntPoint>>(LoadJson(patternScaledUpResourcePath), options);

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(patternScaledUp), new List<List<IntPoint>>(pathScaledUpList.Select(pointsArray => pointsArray.ToList())), pathIsClosed);

      solution.Count.Should().Be(2);
      //solution[0].Count.Should().Be(18);
      solution[1].Count.Should().Be(4);
      solution[1].Should().BeEquivalentTo(
        new List<IntPoint>()
        {
          new IntPoint(-1203309631L, -1203309631L),
          new IntPoint(-1203309631L, -796690369L),
          new IntPoint(-765603409L, -796690369L),
          new IntPoint(-765603409L, -1203309631L),
        });
    }

    private static string LoadJson(string pathScaledUpResourcePath)
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(pathScaledUpResourcePath))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      return json;
    }
  }
}
