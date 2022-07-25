namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
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
      : base(PlacementTypeEnum.Gravity, 504731, 10000 * 3, 20, 50)
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
          this.hasImportedRawDetail = this.loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp.Clone());

          ISheet firstSheet;
          ((IRawDetail)DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(595D, 395D, RectangleType.FileLoad) })).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest().Wait();
          while (!HasMetTerminationConditions)
          {
            AwaitIterate();
          }
        }
      }
    }

    [Fact]
    public void PlacementTypeMustBeGravity()
    {
      this.config.PlacementType.Should().Be(PlacementTypeEnum.Gravity);
    }

    [Fact]
    public void ShouldHaveReturnedNestResults()
    {
      this.nestingContext.State.TopNestResults.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestingContext.State.TopNestResults.Top.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void FitnessShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.Fitness.Should().BeApproximately(ExpectedFitness, ExpectedFitnessTolerance);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(config.PlacementType);
    }
  }
}
