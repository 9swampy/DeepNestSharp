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
    private IRawDetail loadedRawDetail;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// Initializes a new instance of the <see cref="RunFullNestBoundingBoxFixture"/> class.
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    public RunFullNestBoundingBoxFixture()
      : base(PlacementTypeEnum.BoundingBox, 494512, 10000, 50)
    {
      lock (testSyncLock)
      {
        while (!HasAchievedExpectedFitness && !HasRetriedMaxRuns)
        {
          if (!this.hasImportedRawDetail)
          {
            this.loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
            this.loadedRawDetail.Should().NotBeNull();
            this.hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          }

          ResetIteration();
          this.nestingContext.Polygons.Add(this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp.Clone());
          this.nestingContext.Polygons.Count.Should().Be(2);

          ISheet firstSheet;
          dxfGenerator.GenerateRectangle("Sheet", 595D, 395D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
          firstSheet.WidthCalculated.Should().Be(595D);
          firstSheet.HeightCalculated.Should().Be(395D);
          firstSheet.Area.Should().Be(595D * 395D);
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
      this.Config.PlacementType.Should().Be(PlacementTypeEnum.BoundingBox);
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
    public void FitnessBoundsShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.FitnessBounds.Should().BeApproximately(4000, 1000);
    }

    [Fact]
    public void FitnessSheetsShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.FitnessSheets.Should().BeApproximately(235025, 10000);
    }

    [Fact]
    public void FitnessUnplacedShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.FitnessUnplaced.Should().Be(0);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      this.nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(Config.PlacementType);
    }
  }
}
