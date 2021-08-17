namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullNestGravityFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";
    private const double ExpectedFitness = 518947;
    private const double ExpectedFitnessTolerance = 10000;

    private static volatile object testSyncLock = new object();
    private DefaultSvgNestConfig config;
    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int terminateNestResultCount = 4;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// Regularly FitnessShouldBeExpected doesn't get down to the expected 518947, only gets to 557767. A Rerun and it always works; from
    /// logs it doesn't seem to be deadlocking because the loop keeps running. Not sure what's going on...
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

          ISheet firstSheet;
          DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(595D, 395D, RectangleType.FileLoad) }).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest();
          int i = 0;
          while (i < 100 && this.nestingContext.State.TopNestResults.Count < terminateNestResultCount)
          {
            i++;
            this.nestingContext.NestIterate(this.config);
            progressCapture.Are.WaitOne(1000);
            if (this.nestingContext.State.TopNestResults.Count >= terminateNestResultCount &&
                this.nestingContext.State.TopNestResults.Top.Fitness <= ExpectedFitness + ExpectedFitnessTolerance)
            {
              break;
            }
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
      this.nestingContext.State.TopNestResults.Top.Fitness.Should().BeApproximately(ExpectedFitness, ExpectedFitnessTolerance);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(config.PlacementType);
    }
  }
}
