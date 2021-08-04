namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullNestGravityFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private static volatile object testSyncLock = new object();
    private DefaultSvgNestConfig config;
    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int terminateNestResultCount = 2;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    public RunFullNestGravityFixture()
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.config = new DefaultSvgNestConfig();
          this.config.PlacementType = PlacementTypeEnum.Gravity;
          config.PopulationSize = 40;
          this.loadedRawDetail = DxfParser.LoadDxfStream(DxfTestFilename);
          var progressCapture = new ProgressTestResponse();
          this.nestingContext = new NestingContext(A.Fake<IMessageService>(), progressCapture, new NestState(config, A.Fake<IDispatcherService>()));
          this.hasImportedRawDetail = this.loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp.Clone());

          INfp firstSheet;
          DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(595D, 395D, RectangleType.FileLoad) }).TryConvertToNfp(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest();
          int i = 0;
          while (i < 100 && this.nestingContext.State.TopNestResults.Count < terminateNestResultCount)
          {
            i++;
            this.nestingContext.NestIterate(this.config);
            progressCapture.Are.WaitOne(1000);
          }
        }
      }
    }

    [Fact]
    public void PlacementTypeMustBeSqueeze()
    {
      this.config.PlacementType.Should().Be(PlacementTypeEnum.Gravity);
    }

    [Fact]
    public void ShouldHaveReturnedNestResults()
    {
      this.nestingContext.State.TopNestResults.Count.Should().BeGreaterOrEqualTo(terminateNestResultCount);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestingContext.State.TopNestResults.Top.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void FitnessShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.Fitness.Should().BeApproximately(515620, 1000);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(config.PlacementType);
    }
  }
}
