namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullNestGravityFixture : TerminatingRunFullFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private static volatile object testSyncLock = new object();
    private IRawDetail loadedRawDetail;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// Regularly FitnessShouldBeExpected doesn't get down to the expected 518947, only gets to 557767. A Rerun and it always works; from
    /// logs it doesn't seem to be deadlocking because the loop keeps running. Not sure what's going on...
    /// </summary>
    public RunFullNestGravityFixture()
      : base(PlacementTypeEnum.Gravity, 301890, 50000, 150)
    {
      ExecuteTest();
    }

    protected override void PrepIteration()
    {
      nestingContext.Polygons.Add(loadedNfp);
      nestingContext.Polygons.Add(loadedNfp.Clone());
      ISheet firstSheet;
      ((IRawDetail)DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(595D, 395D, RectangleType.FileLoad) })).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      nestingContext.Sheets.Add(firstSheet);
    }

    protected override bool LoadRawDetail()
    {
      loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
      return loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out loadedNfp);
    }

    [Fact]
    public void PlacementTypeMustBeGravity()
    {
      Config.PlacementType.Should().Be(PlacementTypeEnum.Gravity);
    }

    [Fact]
    public void ShouldHaveReturnedNestResults()
    {
      nestingContext.State.TopNestResults.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      nestingContext.State.TopNestResults.Top.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void FitnessShouldBeExpected()
    {
      FitnessShouldBeExpectedVerification();
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(Config.PlacementType);
    }
  }
}
