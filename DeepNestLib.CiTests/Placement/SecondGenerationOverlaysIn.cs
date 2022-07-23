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

  public class SecondGenerationOverlaysIn
  {
    private PartPlacementWorker sut;
    private PartPlacementWorker sutOutExpected;
    private IPlacementWorker placementWorker;
    private INestStateMinkowski state;

    public SecondGenerationOverlaysIn()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SecondGenerationOverlaysOut.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      sutOutExpected = PartPlacementWorker.FromJson(json);

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SecondGenerationOverlaysIn.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      sut = PartPlacementWorker.FromJson(json);
      placementWorker = A.Fake<IPlacementWorker>();
      ((ITestPartPlacementWorker)sut).PlacementWorker = placementWorker;

      ((MinkowskiSum)((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService).VerboseLogAction = s => placementWorker.VerboseLog(s);
      state = A.Fake<INestStateMinkowski>();
      ((MinkowskiSum)((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService).State = state;
      ((MinkowskiSum)((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).MinkowskiSumService).UseMinkowskiCache = false;

      sut.ProcessPart(sut.InputPart, 1);
    }

    [Fact]
    public void LogEntriesHaveBeenSameUpTo()
    {
      //This brittle tests breaks when the Logging changes.
      //If you've updated logging then add new entries to the Out.json
      for (int i = 0; i < sutOutExpected.Log.Count; i++)
      {
        sutOutExpected.Log[i].Should().Be(sut.Log[i]);
      }
    }

    [Fact]
    public void ClipCacheShouldBeEquivalent()
    {
      sut.ClipCache.Should().BeEquivalentTo(sutOutExpected.ClipCache);
    }

    [Fact]
    public void FinalNfpShouldBeEquivalent()
    {
      sut.FinalNfp.Should().BeEquivalentTo(sutOutExpected.FinalNfp);
    }

    [Fact]
    public void SheetNfpShouldBeEquivalent()
    {
      sut.SheetNfp.Should().BeEquivalentTo(sutOutExpected.SheetNfp, options => options
                                    .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.002))
                                    .WhenTypeIs<double>());
    }

    [Fact]
    public void CombinedNfpShouldBeEquivalent()
    {
      sut.CombinedNfp.Should().BeEquivalentTo(sutOutExpected.CombinedNfp, options => options
                                    .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
                                    .WhenTypeIs<double>());
    }

    [Fact]
    public void LogEntriesShouldBeTheSameInOrder()
    {
      for (int i = 0; i < sut.Log.Count; i++)
      {
        LogDebug(i);
      }
    }

    [Fact]
    public void LogEntriesShouldBeEquivalent()
    {
      sut.Log.Should().BeEquivalentTo(sutOutExpected.Log);
    }

    [Fact]
    public void ShouldCallBackToAddPlacement()
    {
      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    private void LogDebug(int i)
    {
      sut.Log[i].Should().Be(sutOutExpected.Log[i]);
      System.Diagnostics.Debug.Print($"Matched {sut.Log[i]}");
    }
  }
}
