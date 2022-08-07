namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullNestSqueezeFixture : TerminatingRunFullFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private static volatile object testSyncLock = new object();
    private IRawDetail loadedRawDetail;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    public RunFullNestSqueezeFixture()
      : base(PlacementTypeEnum.Squeeze, 468000, 10000 * 3, 50, 10)
    {
      lock (testSyncLock)
      {
        if (!hasImportedRawDetail)
        {
          loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
          hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out loadedNfp);
          nestingContext.Polygons.Add(loadedNfp);
          nestingContext.Polygons.Add(loadedNfp.Clone());

          ISheet firstSheet;
          ((IRawDetail)DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(595D, 395D, RectangleType.FileLoad) })).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          nestingContext.Sheets.Add(firstSheet);

          nestingContext.StartNest().Wait();
          while (!HasMetTerminationConditions)
          {
            AwaitIterate();
          }
        }
      }
    }

    [Fact]
    public void PlacementTypeMustBeSqueeze()
    {
      Config.PlacementType.Should().Be(PlacementTypeEnum.Squeeze);
    }

    [Fact]
    public void ShouldHaveReturnedNestResults()
    {
      nestingContext.State.TopNestResults.Any().Should().BeTrue();
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      nestingContext.State.TopNestResults.Top.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void FitnessShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.FitnessTotal.Should().BeApproximately(ExpectedFitness, ExpectedFitnessTolerance);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(Config.PlacementType);
    }
  }
}
