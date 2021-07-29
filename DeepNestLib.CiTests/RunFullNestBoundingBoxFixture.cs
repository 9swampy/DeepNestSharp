namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullNestBoundingBoxFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";
    private readonly DxfGenerator dxfGenerator = new DxfGenerator();
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
          this.nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
          this.hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp.Clone());
          this.nestingContext.Polygons.Count.Should().Be(2);

          INfp firstSheet;
          dxfGenerator.GenerateRectangle("Sheet", 595D, 395D, RectangleType.FileLoad).TryConvertToNfp(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest();
          int i = 0;
          while (i < 100 && this.nestingContext.Nest.TopNestResults.Count < terminateNestResultCount)
          {
            i++;
            this.nestingContext.NestIterate(this.config);
            Thread.Sleep(100);
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
      this.nestingContext.Nest.TopNestResults.Count.Should().BeGreaterOrEqualTo(terminateNestResultCount);
    }

    [Fact]
    public void ShouldHaveNoUnplacedParts()
    {
      this.nestingContext.Nest.TopNestResults.Top.UnplacedParts.Should().BeEmpty();
    }

    [Fact]
    public void FitnessShouldBeExpected()
    {
      this.nestingContext.Nest.TopNestResults.Top.Fitness.Should().BeLessThan(600000);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.Nest.TopNestResults.Top.PlacementType.Should().Be(config.PlacementType);
    }
  }
}
