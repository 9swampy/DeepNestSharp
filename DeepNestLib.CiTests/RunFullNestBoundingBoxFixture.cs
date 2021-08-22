namespace DeepNestLib.CiTests
{
  using System;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class RunFullNestBoundingBoxFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";
    private const double ExpectedFitness = 494512;
    private const double ExpectedFitnessTolerance = 10000;

    private static volatile object testSyncLock = new object();

    private readonly DxfGenerator dxfGenerator = new DxfGenerator();
    private DefaultSvgNestConfig config;
    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int terminateNestResultCount = 4;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// Initializes a new instance of the <see cref="RunFullNestBoundingBoxFixture"/> class.
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    public RunFullNestBoundingBoxFixture()
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.config = new DefaultSvgNestConfig();
          this.config.PlacementType = PlacementTypeEnum.BoundingBox;
          this.config.UseParallel = false;
          config.PopulationSize = 40;
          this.loadedRawDetail = DxfParser.LoadDxfStream(DxfTestFilename);
          this.loadedRawDetail.Should().NotBeNull();
          var progressCapture = new ProgressTestResponse();
          this.nestingContext = new NestingContext(A.Fake<IMessageService>(), progressCapture, new NestState(config, A.Fake<IDispatcherService>()), this.config);
          this.hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp.Clone());
          this.nestingContext.Polygons.Count.Should().Be(2);

          ISheet firstSheet;
          dxfGenerator.GenerateRectangle("Sheet", 595D, 395D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest();
          int i = 0;
          while (i < 100 && this.nestingContext.State.TopNestResults.Count < terminateNestResultCount)
          {
            i++;
            this.nestingContext.NestIterate(this.config);
            Thread.Sleep(100);
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
    public void NestingContextShouldHaveExpectedPolygons()
    {
      this.nestingContext.Polygons.Count.Should().Be(2);
    }

    [Fact]
    public void PlacementTypeMustBeBoundingBox()
    {
      this.config.PlacementType.Should().Be(PlacementTypeEnum.BoundingBox);
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
      this.nestingContext.State.TopNestResults.Top.Fitness.Should().BeLessThan(ExpectedFitness + ExpectedFitnessTolerance);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(config.PlacementType);
    }
  }
}
