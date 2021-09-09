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
      var config = sut.Config as ISvgNestConfig;
      config.Rotations = 1;
      var dispatcherService = A.Fake<IDispatcherService>();
      A.CallTo(() => dispatcherService.InvokeRequired).Returns(false);
      sut.Config.ClipperScale.Should().Be(10000000);
      sut.Config.Rotations.Should().Be(1);
      sut.Config.PlacementType.Should().Be(PlacementTypeEnum.Gravity);
      sut.Config.ExportExecutions.Should().BeTrue();
      sut.Config.ExportExecutions = false;
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
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(config, A.Fake<INestStateMinkowski>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenMinkowskiDllImportWhenProcessPartProcessToAddPlacement(bool userDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupPartPlacementWorker(userDllImport, out sut, out placementWorker);

      sut.ProcessPart(sut.InputPart, 0);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    [Fact]
    public void GivenSamePartPlacementWorkerSetupWhenProcessPartProcessThenSheetNfpPartShouldBeEquivalent()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2B.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      NoFitPolygon.FromJson(json).WidthCalculated.Should().BeApproximately(159.34, 0.01);
      NoFitPolygon.FromJson(json).HeightCalculated.Should().BeApproximately(156.23, 0.01);

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
      sutNewClipper.SheetNfp.Part.Should().BeEquivalentTo(
        sutDll.SheetNfp.Part,
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                          .WhenTypeIs<double>());
      sutNewClipper.SheetNfp.CanAcceptPart.Should().Be(sutDll.SheetNfp.CanAcceptPart);
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
      var sut = MinkowskiSum.CreateInstance(scenarioIn.Config as ISvgNestConfig, A.Fake<INestStateMinkowski>());
      var executor = new NfpHelper(sut, A.Fake<IWindowUnk>());
      var result = executor.ExecuteDllImportMinkowski(scenarioIn.Sheet, scenarioIn.InputPart, MinkowskiCache.NoCache, true);
      result.Length.Should().Be(1);
      json = result[0].ToJson();
      result[0].HeightCalculated.Should().BeApproximately(352.23, 0.01);
      result[0].WidthCalculated.Should().BeApproximately(319.33, 0.01);
      result[0][0].X.Should().BeApproximately(217.57, 0.01);
      result[0][0].Y.Should().BeApproximately(274.08, 0.01);
      result[0].Length.Should().Be(29);
      result[0].Children.Count().Should().Be(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSheetAndPartFromScenarioWhenCompareDllImportWithNewMinkowskiThenInnerNfpCalculationShouldBeSameSoSheetNfpBehavesSameBothWays(bool useDllImport)
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementScenarioNewMinkowski.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var scenarioIn = PartPlacementWorker.FromJson(json);
      var sut = MinkowskiSum.CreateInstance(scenarioIn.Config as ISvgNestConfig, A.Fake<INestStateMinkowski>());
      var executor = new NfpHelper(sut, A.Fake<IWindowUnk>()) as ITestNfpHelper;

      scenarioIn.Sheet.EnsureIsClosed();
      var result = executor.ExecuteInterchangeableMinkowski(useDllImport, scenarioIn.Sheet, scenarioIn.InputPart);
      result[0].HeightCalculated.Should().BeApproximately(352.23, 0.01);
      result[0].WidthCalculated.Should().BeApproximately(319.33, 0.01);
      result.Length.Should().Be(1);
      result[0].IsClosed.Should().Be(true);
      var cleanedResult = SvgNest.CleanPolygon2(result[0]);
      cleanedResult.Points.Length.Should().Be(27);
      cleanedResult.IsClosed.Should().Be(true);
      if (useDllImport)
      {
        result[0].Children.Count.Should().Be(0);
      }
      else
      {
        result[0].Children.Count.Should().Be(1, "we're looking to prove that both SheetNfp's can place item; ignore the difference for now.");
      }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSheetAndPartFromScenarioWhenCompareDllImportWithNewMinkowskiThenShouldBeSame(bool useDllImport)
    {
      PartPlacementWorker sut;
      IPlacementWorker placementWorker;
      SetupPartPlacementWorker(useDllImport, out sut, out placementWorker);
      sut.Sheet.EnsureIsClosed();

      sut.ProcessPart(sut.InputPart, 0);

      sut.SheetNfp.CanAcceptPart.Should().Be(true);
      ((SheetNfp)sut.SheetNfp).GetCandidatePointClosestToOrigin().X.Should().BeApproximately(57.58, 0.01);
      ((SheetNfp)sut.SheetNfp).GetCandidatePointClosestToOrigin().Y.Should().BeApproximately(153.12, 0.01);

      sut.FinalNfp.Should().BeNull("it's a first placement on an empty sheet; won't get as far as FinalNfp.");
    }
  }
}
