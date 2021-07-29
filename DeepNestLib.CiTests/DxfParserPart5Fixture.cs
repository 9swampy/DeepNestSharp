namespace DeepNestLib.CiTests
{
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public partial class DxfParserPart5Fixture
  {
    private const string DxfTestFilename = "Dxfs._5.dxf";

    private static volatile object testSyncLock = new object();

    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private INfp loadedNfp;
    private INfp simplifiedNfp;
    private long simplifiedNfpTime;
    private bool hasImportedRawDetail;

    public DxfParserPart5Fixture()
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.loadedRawDetail = DxfParser.LoadDxfStream(DxfTestFilename);
          this.nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
          this.hasImportedRawDetail = this.loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
          var sw = new Stopwatch();
          sw.Start();
          var config = new DefaultSvgNestConfig() { CurveTolerance = 0.72D };
          this.simplifiedNfp = SvgNest.simplifyFunction(this.loadedNfp, false, config.CurveTolerance, config.Simplify);
          sw.Stop();
          this.simplifiedNfpTime = sw.ElapsedMilliseconds;
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
      var rawDetail = DxfParser.LoadDxfStream(DxfTestFilename);

      rawDetail.Should().NotBeNull();
    }

    [Fact]
    public void ShouldLoadThenSetHasImportedRawDetail()
    {
      this.hasImportedRawDetail.Should().BeTrue();
    }

    [Fact]
    public void ShouldReturnAParentNfp()
    {
      this.loadedNfp.Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      this.loadedNfp.Area.Should().BeApproximately(2109, 1d);
    }

    [Fact]
    public void ShouldHaveExpectedRotation()
    {
      this.loadedNfp.Rotation.Should().Be(0F);
    }

    [Fact]
    public void ShouldHaveExpectedPointsCount()
    {
      this.loadedNfp.Points.Count().Should().Be(256);
    }

    [Fact]
    public void ShouldHaveNoNullPoints()
    {
      this.loadedNfp.Points.Where(o => o == null).Should().BeEmpty();
    }

    [Fact]
    public void SimplificationShouldNotTakeLongerThanPreviouslySeen()
    {
      this.simplifiedNfpTime.Should().BeLessThan(6000);
    }

    [Fact]
    public void SimplificationShouldHaveNoMoreThanPreviouslySeenPoints()
    {
      this.simplifiedNfp.Should().NotBeNull();
    }

    [Fact]
    public void SimplificationShouldNotBeTheOriginal()
    {
      this.simplifiedNfp.Should().NotBe(this.loadedNfp);
    }

    [Fact]
    public void SimplificationShouldHaveExpectedPointCount()
    {
      this.simplifiedNfp.Points.Length.Should().Be(46);
    }
  }
}
