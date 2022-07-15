namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullOverlapNestGravityFixture
  {
    private const string Dxf2TestFilename = "Dxfs._2.dxf";
    private const string Dxf4TestFilename = "Dxfs._4.dxf";
    private const double ExpectedFitness = 68814;
    private const double ExpectedFitnessTolerance = 10000 * 2;

    private static volatile object testSyncLock = new object();
    private TestSvgNestConfig config;
    private RawDetail loaded2RawDetail;
    private RawDetail loaded4RawDetail;
    private NestingContext nestingContext;
    private INfp loadedNfp2;
    private INfp loadedNfp4;
    private bool hasImported2RawDetail;
    private bool hasImported4RawDetail;
    private int terminateNestResultCount = 4;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// We should never get an overlapping nest, but this scenario reliably overlaps inappropriately.
    /// 
    /// Two parts that shouldn't be able to fit on a single sheet.
    /// </summary>
    public RunFullOverlapNestGravityFixture()
    {
      lock (testSyncLock)
      {
        if (!this.hasImported2RawDetail || !this.hasImported4RawDetail)
        {
          this.config = new TestSvgNestConfig();
          this.config.PlacementType = PlacementTypeEnum.Gravity;
          this.config.UseMinkowskiCache = false;
          this.config.CurveTolerance = 1;
          this.config.UseDllImport = false;
          config.PopulationSize = 40;
          this.loaded2RawDetail = DxfParser.LoadDxfStream(Dxf2TestFilename);
          this.loaded4RawDetail = DxfParser.LoadDxfStream(Dxf4TestFilename);
          var progressCapture = new ProgressTestResponse();
          this.nestingContext = new NestingContext(A.Fake<IMessageService>(), progressCapture, new NestState(config, A.Fake<IDispatcherService>()), this.config);
          this.hasImported2RawDetail = this.loaded2RawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp2);
          this.hasImported4RawDetail = this.loaded4RawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp4);
          this.nestingContext.Polygons.Add(this.loadedNfp2);
          this.nestingContext.Polygons.Add(this.loadedNfp4);

          ISheet firstSheet;
          DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(180D, 100D, RectangleType.FileLoad) }).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest().Wait();
          int i = 0;
          while (
            (i <= 0 ||
             this.nestingContext.State.TopNestResults.Top.UnplacedParts.Count > 0) &&
             this.nestingContext.State.TopNestResults.Count < terminateNestResultCount)
          {
            i++;
            this.nestingContext.NestIterate(this.config);
            progressCapture.Are.WaitOne(100);
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
      this.nestingContext.State.TopNestResults.Should().NotBeEmpty();
    }

    [Fact]
    public void ShouldHaveUnplacedParts()
    {
      this.nestingContext.State.TopNestResults.Top.UnplacedParts.Should().NotBeEmpty("it isn't possible to nest all without overlapping");
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
