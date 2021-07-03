namespace DeepNestLib.CiTests
{
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public partial class DxfParserPart5Fixture
  {
    private const string DxfTestFilename = "_5.dxf";

    private static volatile object testSyncLock = new object();

    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private NFP loadedNfp;
    private NFP simplifiedNfp;
    private long simplifiedNfpTime;
    private bool hasImportedRawDetail;

    public DxfParserPart5Fixture()
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.loadedRawDetail = DxfParser.LoadDxf(DxfTestFilename);
          this.nestingContext = new NestingContext(A.Fake<IMessageService>());
          this.hasImportedRawDetail = this.nestingContext.TryImportFromRawDetail(this.loadedRawDetail, A.Dummy<int>(), out this.loadedNfp);
          var sw = new Stopwatch();
          sw.Start();
          this.simplifiedNfp = SvgNest.simplifyFunction(this.loadedNfp, false);
          sw.Stop();
          this.simplifiedNfpTime = sw.ElapsedMilliseconds;
        }
      }
    }

    [Fact]
    public void ShouldFindDxfInBuild()
    {
      var fi = new FileInfo(DxfTestFilename);
      System.Diagnostics.Debug.Print(fi.FullName);
      fi.Exists.Should().BeTrue();
    }

    [Fact]
    public void ShouldLoadDxf()
    {
      DxfParser.LoadDxf(DxfTestFilename);
    }

    [Fact]
    public void ShouldLoadDxfToRawDetail()
    {
      var rawDetail = DxfParser.LoadDxf(DxfTestFilename);

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
      this.loadedNfp.Area.Should().Be(2109.70581F);
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
