namespace DeepNestLib.CiTests
{
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class DxfParserPart1Fixture
  {
    private const string DxfTestFilename = "Dxfs._1.dxf";

    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserPart1Fixture()
    {
      this.loadedRawDetail = DxfParser.LoadDxfStream(DxfTestFilename);
      this.nestingContext = A.Dummy<NestingContext>();
      this.hasImportedRawDetail = this.loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
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
      this.loadedNfp.Area.Should().BeApproximately(2500, 1);
    }

    [Fact]
    public void ShouldHaveExpectedRotation()
    {
      this.loadedNfp.Rotation.Should().Be(0F);
    }

    [Fact]
    public void ShouldHaveExpectedPointsCount()
    {
      this.loadedNfp.Points.Count().Should().Be(5);
    }

    [Fact]
    public void ShouldHaveNoNullPoints()
    {
      this.loadedNfp.Points.Where(o => o == null).Should().BeEmpty();
    }
  }
}
