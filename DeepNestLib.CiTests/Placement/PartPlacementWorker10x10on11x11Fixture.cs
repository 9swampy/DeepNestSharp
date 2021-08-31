namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PartPlacementWorker10x10on11x11Fixture
  {
    private IPlacementWorker placementWorker;
    private PartPlacement partPlacement;
    private PartPlacementWorker sut;
    private NFP inputPartClone;

    public PartPlacementWorker10x10on11x11Fixture()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementWorker10x10on11x11In.json"))
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
      ((ITestNfpHelper)((ITestPartPlacementWorker)sut).NfpHelper).UseDllImport = true;

      var partPlacementArg = new Capture<PartPlacement>();
      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, partPlacementArg, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).Returns(default);

      inputPartClone = new NFP(sut.InputPart, WithChildren.Included);
      sut.ProcessPart(sut.InputPart, 0);

      if (partPlacementArg.HasValues)
      {
        this.partPlacement = partPlacementArg.Value;
      }
    }

    [Fact]
    public void GivenThePartIs10x10ThenShouldHaveExpectedWidth()
    {
      sut.InputPart.WidthCalculated.Should().Be(10);
    }

    [Fact]
    public void GivenThePartIs10x10ThenShouldHaveExpectedHeight()
    {
      sut.InputPart.HeightCalculated.Should().Be(10);
    }

    [Fact]
    public void GivenTheSheetIs11x11ThenShouldHaveExpectedHeight()
    {
      sut.Sheet.HeightCalculated.Should().Be(11);
    }

    [Fact]
    public void GivenTheSheetIs11x11ThenShouldHaveExpectedWidth()
    {
      sut.Sheet.WidthCalculated.Should().Be(11);
    }

    [Fact]
    public void GivenTheSheetIsCleanThenShouldHaveNoChildren()
    {
      sut.Sheet.Children.Should().BeEmpty();
    }

    [Fact]
    public void GivenTheSheetNfpIsASnapshotWhenSheetPersistedThenShouldNotBeSameReference()
    {
      sut.SheetNfp.Sheet.Should().NotBe(sut.Sheet);
    }

    [Fact]
    public void GivenTheSheetNfpIsASnapshotWhenSheetStillNotChangedThenShouldBeEquivalent()
    {
      sut.SheetNfp.Sheet.Should().BeEquivalentTo(sut.Sheet);
    }

    [Fact]
    public void GivenTheSheetNfpIsASnapshotWhenPartPersistedThenShouldNotBeSameReference()
    {
      sut.SheetNfp.Part.Should().NotBe(sut.InputPart);
    }

    [Fact]
    public void GivenTheSheetNfpIsASnapshotOfFirstUnrotatedShiftedPartWhenBothShiftToOriginThenShouldBeEquivalentToOriginalInputPartCloneShiftedToOrigin()
    {
      sut.SheetNfp.Part.ShiftToOrigin().Should().BeEquivalentTo(inputPartClone.ShiftToOrigin(), "forced even when CanBePlaced is wrong by config.Rotations=1");
    }

    [Fact]
    public void GivenPartFitsOnEmptySheetThenSheetNfpShouldHaveOneNfpReturned()
    {
      sut.SheetNfp.Items.Length.Should().Be(1);
    }

    [Fact]
    public void GivenPartFitsOnEmptySheetThenSheetNfpItemsShouldHaveOneNfpReturnedWithExpectedWidth()
    {
      sut.SheetNfp.Items[0].WidthCalculated.Should().Be(1);
    }

    [Fact]
    public void GivenPartFitsOnEmptyThenSheetNfpShouldHaveLengthRelayingItemsLength()
    {
      sut.SheetNfp.NumberOfNfps.Should().Be(1);
    }

    [Fact]
    public void GivenSingleSheetNfpThenShouldBeSheetHeightLessPartHeight()
    {
      sut.SheetNfp[0].HeightCalculated.Should().BeApproximately(sut.Sheet.HeightCalculated - sut.InputPart.HeightCalculated, 0.001);
    }

    [Fact]
    public void GivenSingleSheetNfpThenShouldBeSheetWidthLessPartWidth()
    {
      sut.SheetNfp[0].WidthCalculated.Should().BeApproximately(sut.Sheet.WidthCalculated - sut.InputPart.WidthCalculated, 0.001);
    }

    [Fact]
    public void GivenPartFitsThenSheetNfpShouldAcceptPart()
    {
      sut.SheetNfp.CanAcceptPart.Should().BeTrue();
    }

    [Fact]
    public void GivenThePartFitsThenCallBackToAddPlacementMustHaveHappened()
    {
      A.CallTo(() => placementWorker.AddPlacement(sut.InputPart, A<List<IPartPlacement>>._, A<INfp>._, A<PartPlacement>._, A<PlacementTypeEnum>._, A<ISheet>._, A<double>._)).MustHaveHappened();
    }

    [Fact]
    public void GivenThatAddPlacementCallbackHappenedThenPartPlacementShouldHaveBeenCaptured()
    {
      this.partPlacement.Should().NotBeNull();
    }

    [Fact]
    public void GivenThatPartFitsThenShouldBeOnPlacementAsClone()
    {
      partPlacement.Part.Should().BeEquivalentTo(sut.InputPart);
    }

    [Fact]
    public void GivenThatPartOnPlacementShouldBeACloneThenNotSameObject()
    {
      partPlacement.Part.Should().NotBe(sut.InputPart);
    }

    [Fact]
    public void GivenPartOnlyPlacementThenShouldBeAtXOrigin()
    {
      partPlacement.X.Should().Be(0);
    }

    [Fact]
    public void GivenPartOnlyPlacementThenShouldBeAtYOriginPlusPartHeight()
    {
      partPlacement.Y.Should().BeApproximately(sut.InputPart.HeightCalculated, 0.01);
    }
  }
}
