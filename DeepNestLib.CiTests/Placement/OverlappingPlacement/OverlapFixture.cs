namespace DeepNestLib.CiTests.Placement.OverlappingPlacement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class OverlapFixture
  {
    [Fact]
    public void GivenTwoEntryPointsOneThatFailsAndOneThatDidNotOverlayCompareInputs()
    {
      PartPlacement actualPartPlacement16 = null;
      SheetPlacement actualSheetPlacement16 = null;
      (PartPlacementWorker sut16, IPlacementWorker placementWorker16) =
        GetSut("Placement.OverlappingPlacement.N16-S0-5-P1-In.json", false, o => actualPartPlacement16 = o, o => actualSheetPlacement16 = o);

      PartPlacement actualPartPlacement20 = null;
      SheetPlacement actualSheetPlacement20 = null;
      (PartPlacementWorker sut20, IPlacementWorker placementWorker20) =
        GetSut("Placement.OverlappingPlacement.N20-S0-5-P1-In.json", false, o => actualPartPlacement20 = o, o => actualSheetPlacement20 = o);

      sut16.Should().BeEquivalentTo(sut20, opt => opt.Excluding(o => o.Gene[0].Rotation));
      sut16.Gene[0].Rotation.Should().Be(180);
      sut20.Gene[0].Rotation.Should().Be(270);

      sut16.ProcessPart(sut16.InputPart, 0).Should().Be(InnerFlowResult.Success);
      sut20.ProcessPart(sut20.InputPart, 0).Should().Be(InnerFlowResult.Success);

      actualSheetPlacement16.Should().BeEquivalentTo(actualSheetPlacement20);
      actualPartPlacement16.Should().BeEquivalentTo(actualPartPlacement20);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenN16OverlaysWhenProcessPartGoesWrong(bool useCache)
    {
      PartPlacement actualPartPlacement = null;
      SheetPlacement actualSheetPlacement = null;
      (PartPlacementWorker sut, IPlacementWorker placementWorker) =
        GetSut("Placement.OverlappingPlacement.N16-S0-5-P1-In.json", useCache, o => actualPartPlacement = o, o => actualSheetPlacement = o);

      sut.ProcessPart(sut.InputPart, 0).Should().Be(InnerFlowResult.Success);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();

      actualPartPlacement.Id.Should().Be(0);
      actualPartPlacement.Part.Rotation.Should().Be(180);
      actualPartPlacement.Part.X.Should().Be(0);
      actualPartPlacement.Part.Y.Should().Be(0);
      actualPartPlacement.Part.Source.Should().Be(0);
      sut.Config.ClipperScale.Should().Be(10000000);
      sut.CombinedNfp[0].Count.Should().Be(147);
      actualPartPlacement.X.Should().BeApproximately(282.45, 0.01);
      actualPartPlacement.Y.Should().BeApproximately(217.93, 0.01);
      actualPartPlacement.Part.Points[0].X.Should().Be(-99.335);
      actualPartPlacement.Part.Points[0].Y.Should().Be(-118.8241);
      actualPartPlacement.Rotation.Should().Be(180);
      actualPartPlacement.Part.OffsetX = 0;
      actualPartPlacement.Part.OffsetY = 0;
      actualSheetPlacement.Should().NotBeNull();
      sut.Placements[0].Rotation.Should().Be(270);
      sut.Placements[1].Rotation.Should().Be(180);
      actualSheetPlacement.Fitness.Total.Should().BeApproximately(140019, 1, "while it's wrong this is the behaviour we expected to replicate");
    }

    private static (PartPlacementWorker sut, IPlacementWorker placementWorker) GetSut(string jsonSource, bool useCache, Action<PartPlacement> setPartPlacement, Action<SheetPlacement> setSheetPlacement)
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(jsonSource))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var sut = PartPlacementWorker.FromJson(json);
      var placementWorker = A.Fake<IPlacementWorker>();
      ((ITestPartPlacementWorker)sut).PlacementWorker = placementWorker;
      ((ITestPartPlacementWorker)sut).ExportExecutions = false;
      ((ITestPartPlacementWorker)sut).EnableCaches = useCache;
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>());
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).UseCacheProcess = useCache;
      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._))
        .Invokes(o =>
        {
          //0 INfp inputPart,
          //1 List<IPartPlacement> placements,
          //2 INfp processedPart,
          //3 PartPlacement position,
          //4 PlacementTypeEnum placementType,
          //5 ISheet sheet,
          //6 double mergedLength
          if (o.Arguments[1] is List<IPartPlacement> placements &&
              o.Arguments[3] is PartPlacement position &&
              o.Arguments[4] is PlacementTypeEnum placementType &&
              o.Arguments[5] is ISheet sheet &&
              o.Arguments[6] is double mergedLength)
          {
            setPartPlacement(position);
            placements.Add(position);
            setSheetPlacement(new SheetPlacement(placementType, sheet, placements, mergedLength, new TestSvgNestConfig().ClipperScale));
          }
        });
      return (sut, placementWorker);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenN20DoesNotOverlayWhenProcessPartHowIsThisDifferentSoItGoesRight(bool useCache)
    {
      PartPlacement actualPartPlacement = null;
      SheetPlacement actualSheetPlacement = null;
      (PartPlacementWorker sut, IPlacementWorker placementWorker) =
        GetSut("Placement.OverlappingPlacement.N20-S0-5-P1-In.json", useCache, o => actualPartPlacement = o, o => actualSheetPlacement = o);
      try
      {
        sut.ProcessPart(sut.InputPart, 0).Should().Be(InnerFlowResult.Success);

        A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();

        actualPartPlacement.Id.Should().Be(0);
        actualPartPlacement.Part.Rotation.Should().Be(180);
        actualPartPlacement.Part.X.Should().Be(0);
        actualPartPlacement.Part.Y.Should().Be(0);
        actualPartPlacement.Part.Source.Should().Be(0);
        sut.Config.ClipperScale.Should().Be(10000000);
        sut.CombinedNfp[0].Count.Should().Be(147);
        actualPartPlacement.X.Should().BeApproximately(282.4467, 0.0001);
        actualPartPlacement.Y.Should().Be(217.9382);
        actualPartPlacement.Part.Points[0].X.Should().Be(-99.335);
        actualPartPlacement.Part.Points[0].Y.Should().Be(-118.8241);
        actualPartPlacement.Rotation.Should().Be(180);
        actualPartPlacement.Part.OffsetX = 0;
        actualPartPlacement.Part.OffsetY = 0;
        actualSheetPlacement.Should().NotBeNull();
        sut.Placements[0].Rotation.Should().Be(270);
        sut.Placements[1].Rotation.Should().Be(180);
        actualSheetPlacement.Fitness.Total.Should().BeGreaterThan(130000);
      }
      catch
      {
        foreach (var log in sut.Log)
        {
          System.Diagnostics.Debug.Print(log);
        }

        throw;
      }
    }
  }
}
