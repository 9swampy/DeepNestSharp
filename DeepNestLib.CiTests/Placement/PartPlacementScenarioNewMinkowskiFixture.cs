namespace DeepNestLib.CiTests.Placement
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PartPlacementScenarioNewMinkowskiFixture
  {
    private static void SetupPartPlacementWorker(bool useDllImport, out PartPlacementWorker sut, out IPlacementWorker placementWorker)
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementScenarioNewMinkowski.json"))
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
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(config, A.Fake<INestStateMinkowski>());
    }

    [Fact]
    public void GivenMinkowskiDllImportWhenProcessPartProcessToAddPlacement()
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupPartPlacementWorker(true, out sut, out placementWorker);

      sut.ProcessPart(sut.InputPart, 0);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    [Fact]
    public void GivenSamePartPlacementWorkerSetupWhenProcessPartProcessThenSheetNfpShouldBeEquivalent()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2B.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      NFP.FromJson(json).WidthCalculated.Should().BeApproximately(159.34, 0.01);
      NFP.FromJson(json).HeightCalculated.Should().BeApproximately(156.23, 0.01);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2A.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      var aSheet = Sheet.FromJson(json);
      aSheet.Children[0].WidthCalculated.Should().BeApproximately(160, 0.01);
      aSheet.Children[0].HeightCalculated.Should().BeApproximately(196, 0.01);

      PartPlacementWorker sutDll;
      SetupPartPlacementWorker(true, out sutDll, out _);
      sutDll.ProcessPart(sutDll.InputPart, 0);

      PartPlacementWorker sutNewClipper;
      SetupPartPlacementWorker(false, out sutNewClipper, out _);
      sutNewClipper.ProcessPart(sutNewClipper.InputPart, 0);

      sutNewClipper.InputPart.WidthCalculated.Should().BeApproximately(159.34, 0.01);
      sutNewClipper.InputPart.HeightCalculated.Should().BeApproximately(156.23, 0.01);

      sutNewClipper.Sheet.WidthCalculated.Should().BeApproximately(160, 0.01);
      sutNewClipper.Sheet.HeightCalculated.Should().BeApproximately(196, 0.01);

      sutNewClipper.InputPart.Should().BeEquivalentTo(sutDll.InputPart, "forced right even when SheetNfp.CanPlacePart fails by Rotations=1");
      sutNewClipper.Sheet.Should().BeEquivalentTo(sutDll.Sheet);
      sutNewClipper.SheetNfp.Sheet.Should().BeEquivalentTo(sutDll.SheetNfp.Sheet);
      sutNewClipper.SheetNfp.NumberOfNfps.Should().Be(sutDll.SheetNfp.NumberOfNfps);
      sutNewClipper.SheetNfp.Part.Should().BeEquivalentTo(sutDll.SheetNfp.Part);
    }

    [Fact]
    public void GivenNewClipperMinkowskiWhenProcessPartProcessToAddPlacement()
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupPartPlacementWorker(false, out sut, out placementWorker);

      sut.ProcessPart(sut.InputPart, 0);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    [Fact]
    public void GivenSheetAndPartWhenNfpHelperExecuteDllImportMinkowskiThenExpectedResponseFromDllImport()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementScenarioNewMinkowski.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var scenarioIn = PartPlacementWorker.FromJson(json);
      var sut = MinkowskiSum.CreateInstance(scenarioIn.Config, A.Fake<INestStateMinkowski>());
      var executor = new NfpHelper(sut, A.Fake<IWindowUnk>());
      var result = executor.ExecuteDllImportMinkowski(scenarioIn.Sheet, scenarioIn.InputPart, MinkowskiCache.NoCache);
      result.Length.Should().Be(1);
      json = result[0].ToJson();
      result[0].HeightCalculated.Should().BeApproximately(352.23, 0.01);
      result[0].WidthCalculated.Should().BeApproximately(319.33, 0.01);
      result[0][0].X.Should().BeApproximately(217.57, 0.01);
      result[0][0].Y.Should().BeApproximately(274.08, 0.01);
      result[0].Length.Should().Be(29);
      result[0].Children.Count().Should().Be(0);
      //result[0].Points[0].Should().BeEquivalentTo()
    }

    [Fact]
    public void GivenSheetAndPartWhenNfpHelperExecuteDllImportMinkowskiThenExpectedResponseFromNewMinkowski()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementScenarioNewMinkowski.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var scenarioIn = PartPlacementWorker.FromJson(json);
      var sut = MinkowskiSum.CreateInstance(scenarioIn.Config, A.Fake<INestStateMinkowski>());
      var executor = new NfpHelper(sut, A.Fake<IWindowUnk>());
      var result = executor.ExecuteDllImportMinkowski(scenarioIn.Sheet, scenarioIn.InputPart, MinkowskiCache.NoCache);
      result.Length.Should().Be(1);
      json = result[0].ToJson();
      result[0].HeightCalculated.Should().BeApproximately(352.23, 0.01);
      result[0].WidthCalculated.Should().BeApproximately(319.33, 0.01);
      //result[0][0].X.Should().BeApproximately(-77.34, 0.01);
      //result[0][0].Y.Should().BeApproximately(19.15, 0.01);
      result[0].Length.Should().Be(26);
      result[0].Children.Count().Should().Be(0);
      //result[0].Points[0].Should().BeEquivalentTo()
    }
  }
}
