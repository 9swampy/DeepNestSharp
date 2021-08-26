namespace DeepNestLib.CiTests.Placement
{
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class Fit12InDonutHoleFixture
  {
    [Fact]
    public void Test()
    {
      var sut = A.Fake<INfp>();
      sut.Should().NotBeNull();
    }

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
  }
}
