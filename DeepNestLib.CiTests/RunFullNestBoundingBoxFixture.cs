namespace DeepNestLib.CiTests
{
  using System;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class RunFullNestBoundingBoxFixture : TerminatingRunFullFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private static volatile object testSyncLock = new object();

    private readonly DxfGenerator dxfGenerator = new DxfGenerator();
    private RawDetail loadedRawDetail;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// Initializes a new instance of the <see cref="RunFullNestBoundingBoxFixture"/> class.
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    public RunFullNestBoundingBoxFixture()
      : base(PlacementTypeEnum.BoundingBox, 494512, 10000, 10, 50)
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.loadedRawDetail = DxfParser.LoadDxfStream(DxfTestFilename);
          this.loadedRawDetail.Should().NotBeNull();
          this.hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp.Clone());
          this.nestingContext.Polygons.Count.Should().Be(2);

          ISheet firstSheet;
          dxfGenerator.GenerateRectangle("Sheet", 595D, 395D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
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
      this.nestingContext.State.TopNestResults.Top.Fitness.Should().BeLessThan(ExpectedFitness + ExpectedFitnessTolerance);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(config.PlacementType);
    }
  }
}
