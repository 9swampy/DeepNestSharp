namespace DeepNestLib.CiTests.Placement
{
  using System;
  using DeepNestLib;
  using DeepNestLib.CiTests;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class RunFullNestBoundingBoxFixture : TerminatingRunFullFixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private readonly DxfGenerator dxfGenerator = new DxfGenerator();
    private IRawDetail loadedRawDetail;
    private INfp loadedNfp;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// Initializes a new instance of the <see cref="RunFullNestBoundingBoxFixture"/> class.
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    public RunFullNestBoundingBoxFixture()
      : base(PlacementTypeEnum.BoundingBox, 494512, 10000, 50)
    {
      ExecuteTest();
    }

    protected override void PrepIteration()
    {
      nestingContext.Polygons.Add(loadedNfp);
      nestingContext.Polygons.Add(loadedNfp.Clone());
      nestingContext.Polygons.Count.Should().Be(2);

      ISheet firstSheet;
      dxfGenerator.GenerateRectangle("Sheet", 595D, 395D, RectangleType.FileLoad).TryConvertToSheet(firstSheetIdSrc, out firstSheet).Should().BeTrue();
      firstSheet.WidthCalculated.Should().Be(595D);
      firstSheet.HeightCalculated.Should().Be(395D);
      firstSheet.Area.Should().Be(595D * 395D);
      nestingContext.Sheets.Add(firstSheet);
    }

    protected override bool LoadRawDetail()
    {
      loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
      loadedRawDetail.Should().NotBeNull();
      return loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out loadedNfp);
    }

    [Fact]
    public void NestingContextShouldHaveExpectedPolygons()
    {
      nestingContext.Polygons.Count.Should().Be(2);
    }

    [Fact]
    public void PlacementTypeMustBeBoundingBox()
    {
      Config.PlacementType.Should().Be(PlacementTypeEnum.BoundingBox);
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
    public void FitnessBoundsShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.FitnessBounds.Should().BeApproximately(4000, 1000);
    }

    [Fact]
    public void FitnessSheetsShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.FitnessSheets.Should().BeApproximately(235025, 10000);
    }

    [Fact]
    public void FitnessUnplacedShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.FitnessUnplaced.Should().Be(0);
    }

    [Fact]
    public void PlacementTypeShouldBeExpected()
    {
      nestingContext.State.TopNestResults.Top.PlacementType.Should().Be(Config.PlacementType);
    }
  }
}
