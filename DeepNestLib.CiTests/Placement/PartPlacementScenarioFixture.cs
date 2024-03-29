﻿namespace DeepNestLib.CiTests.Placement
{
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using Xunit;

  public class PartPlacementScenarioFixture
  {
    [Fact]
    public void ShouldProcessToEndAndCallOutToAddPlacement()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementScenarioPartPlacementWorker.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var sut = PartPlacementWorker.FromJson(json);
      var placementWorker = A.Fake<IPlacementWorker>();
      ((ITestPartPlacementWorker)sut).PlacementWorker = placementWorker;
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>());

      sut.ProcessPart(sut.InputPart, 0);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    [Fact]
    public void ShouldProcessToEndButNotCallOutToAddPlacement()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementScenarioDoNotOverlap.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var sut = PartPlacementWorker.FromJson(json);
      var placementWorker = A.Fake<IPlacementWorker>();
      ((ITestPartPlacementWorker)sut).PlacementWorker = placementWorker;
      ((ITestPartPlacementWorker)sut).ExportExecutions = false;
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>());

      sut.ProcessPart(sut.InputPart, 0);

      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustNotHaveHappened();
    }
  }
}
