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
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out _);

      sut.ProcessPart(sut.InputPart, 1);

      sut.SheetNfp.CanAcceptPart.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSheetNfpComparesToEmptySheetWhenProcessPartThenOnlyOneNfp(bool useDllImport)
    {
      PartPlacementWorker sut;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out _);

      sut.ProcessPart(sut.InputPart, 1);

      sut.SheetNfp.NumberOfNfps.Should().Be(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSheetNfpComparesToEmptySheetWhenProcessPartThenOnlyOneNfpCLosedWithExpectedPointsAndNoChildren(bool useDllImport)
    {
      PartPlacementWorker sut;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out _);

      sut.ProcessPart(sut.InputPart, 1);

      sut.SheetNfp[0].Length.Should().Be(5, "it's closed");
      sut.SheetNfp[0].IsClosed.Should().BeTrue();
      sut.SheetNfp[0][0].X.Should().BeApproximately(121.36, 0.01);
      sut.SheetNfp[0][0].Y.Should().BeApproximately(88.27, 0.01);
      sut.SheetNfp[0][1].X.Should().BeApproximately(1.08, 0.01);
      sut.SheetNfp[0][1].Y.Should().BeApproximately(88.27, 0.01);
      sut.SheetNfp[0][2].X.Should().BeApproximately(1.08, 0.01);
      sut.SheetNfp[0][2].Y.Should().BeApproximately(17.27, 0.01);
      sut.SheetNfp[0][3].X.Should().BeApproximately(121.36, 0.01);
      sut.SheetNfp[0][3].Y.Should().BeApproximately(17.27, 0.01);
      sut.SheetNfp[0][4].X.Should().BeApproximately(121.36, 0.01);
      sut.SheetNfp[0][4].Y.Should().BeApproximately(88.27, 0.01);
      sut.SheetNfp[0].Children.Count.Should().Be(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenFinalNfpComparesToSheetWithDonutWhenProcessPartThenFinalNfpTotalExpected(bool useDllImport)
    {
      PartPlacementWorker sut;
      SetupFit12InDonutHolePartPlacementWorker(useDllImport, out sut, out _);

      sut.ProcessPart(sut.InputPart, 1);
      //File.WriteAllText($"C:\\Temp\\FinalNfp{useDllImport}.dnnfps", sut.FinalNfp.ToJson());

      sut.FinalNfp.Items.Sum(o => o.Area).Should().BeApproximately(255.54, 0.3);

      sut.FinalNfp.Items[0].IsClosed.Should().BeFalse();
      sut.FinalNfp.Items[1].IsClosed.Should().BeFalse();
      if (useDllImport)
      {
        sut.FinalNfp.NumberOfNfps.Should().Be(2, "we get one circle and one right hand contigious NFP");
        sut.FinalNfp.Items[0].Length.Should().Be(19);
        sut.FinalNfp.Items[1].Length.Should().Be(16);
      }
      else
      {
        sut.FinalNfp.NumberOfNfps.Should().Be(3, "we get one circle but the right hand NFP is equivalent but split.");
        sut.FinalNfp.Items[0].Length.Should().Be(16);
        sut.FinalNfp.Items[1].Length.Should().Be(15);
        sut.FinalNfp.Items[2].Length.Should().Be(7);
        sut.FinalNfp.Items[2].IsClosed.Should().BeFalse();
      }
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
      var partPlacement = GetPartPlacementForFit12InDonutHole(useDllImport, out _, out _);

      partPlacement.X.Should().BeApproximately(55.79, 0.11);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenPartFitsWhenProcessPartThenShouldPlaceAtExpectedPositionY(bool useDllImport)
    {
      var partPlacement = GetPartPlacementForFit12InDonutHole(useDllImport, out _, out _);

      partPlacement.Y.Should().BeApproximately(59.56, 1);
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
      var config = sut.Config as ISvgNestConfig;
      config.Rotations = 1;
      config.UseDllImport = useDllImport;
      config.ExportExecutions = false;
      var dispatcherService = A.Fake<IDispatcherService>();
      A.CallTo(() => dispatcherService.InvokeRequired).Returns(false);

      sut.Config.ClipperScale.Should().Be(10000000);
      sut.Config.Rotations.Should().Be(1);
      sut.Config.PlacementType.Should().Be(PlacementTypeEnum.Gravity);
      sut.Config.ExportExecutions.Should().BeFalse();
      sut.Config.Scale.Should().Be(25);
      sut.Config.MergeLines.Should().BeFalse();
      sut.Config.UsePriority.Should().BeFalse();
      sut.Config.UseDllImport = true;
      sut.Config.UseDllImport.Should().BeTrue();
      sut.Config.UseDllImport = useDllImport;
      sut.Config.UseDllImport.Should().Be(useDllImport);

      ((ITestPartPlacementWorker)sut).State = new NestState(config, dispatcherService);
      
      placementWorker = A.Fake<IPlacementWorker>();
      ((ITestPartPlacementWorker)sut).PlacementWorker = placementWorker;

      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(false, A.Fake<INestStateMinkowski>());
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
    public void GivenSerialisedScenariosWhenClipperMinkowskiSumExpect2Solutions()
    {
      string pathScaledUpResourcePath;
      string patternScaledUpResourcePath;
      pathScaledUpResourcePath = "Placement.pathScaledUpListOrig.json";
      patternScaledUpResourcePath = "Placement.patternScaledUpOrig.json";

      var options = new System.Text.Json.JsonSerializerOptions();
      options.IncludeFields = true;
      var pathScaledUpList = System.Text.Json.JsonSerializer.Deserialize<List<List<IntPoint>>>(LoadJson(pathScaledUpResourcePath), options);
      var patternScaledUp = System.Text.Json.JsonSerializer.Deserialize<List<IntPoint>>(LoadJson(patternScaledUpResourcePath), options);

      var solution = ClipperLib.Clipper.MinkowskiSum(new List<IntPoint>(patternScaledUp), new List<List<IntPoint>>(pathScaledUpList.Select(pointsArray => pointsArray.ToList())), true);

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
