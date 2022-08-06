namespace DeepNestLib.CiTests.IO
{
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.IO;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public partial class DxfParserPart5Fixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private static volatile object testSyncLock = new object();

    private IRawDetail loadedRawDetail;
    private INfp loadedNfp;
    private INfp simplifiedNfp;
    private long simplifiedNfpTime;
    private bool hasImportedRawDetail;

    public DxfParserPart5Fixture()
    {
      lock (testSyncLock)
      {
        if (!hasImportedRawDetail)
        {
          loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
          hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out loadedNfp);
          var sw = new Stopwatch();
          sw.Start();
          var config = new TestSvgNestConfig() { CurveTolerance = 0.72D };
          simplifiedNfp = NfpSimplifier.SimplifyFunction(loadedNfp, false, config.CurveTolerance, config.Simplify);
          sw.Stop();
          simplifiedNfpTime = sw.ElapsedMilliseconds;
        }
      }
    }

    [Fact]
    public void ShouldLoadDxf()
    {
      DxfParser.LoadDxfFile(DxfTestFilename);
    }

    [Fact]
    public void ShouldLoadDxfToRawDetail()
    {
      var rawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);

      rawDetail.Should().NotBeNull();
    }

    [Fact]
    public void ShouldLoadThenSetHasImportedRawDetail()
    {
      hasImportedRawDetail.Should().BeTrue();
    }

    [Fact]
    public void ShouldReturnAParentNfp()
    {
      loadedNfp.Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      loadedNfp.Area.Should().BeApproximately(2109, 1d);
    }

    [Fact]
    public void ShouldHaveExpectedRotation()
    {
      loadedNfp.Rotation.Should().Be(0F);
    }

    [Fact]
    public void ShouldHaveExpectedPointsCount()
    {
      loadedNfp.Points.Count().Should().Be(256);
    }

    [Fact]
    public void ShouldHaveNoNullPoints()
    {
      loadedNfp.Points.Where(o => o == null).Should().BeEmpty();
    }

    [Fact]
    public void SimplificationShouldNotTakeLongerThanPreviouslySeen()
    {
      simplifiedNfpTime.Should().BeLessThan(6000);
    }

    [Fact]
    public void SimplificationShouldHaveNoMoreThanPreviouslySeenPoints()
    {
      simplifiedNfp.Should().NotBeNull();
    }

    [Fact]
    public void SimplificationShouldNotBeTheOriginal()
    {
      simplifiedNfp.Should().NotBe(loadedNfp);
    }

    [Fact]
    public void SimplificationShouldHaveExpectedPointCount()
    {
      simplifiedNfp.Points.Length.Should().Be(46);
    }

    [Fact]
    public void ShouldHaveNoChildren()
    {
      loadedNfp.Children.Count.Should().Be(0);
    }
  }
}
