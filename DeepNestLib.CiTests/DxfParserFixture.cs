namespace DeepNestLib.CiTests
{
  using System.Collections.Generic;
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfParserFixture
  {
    private const string DxfTestFilename = "Dxfs.OneSquare.dxf";

    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();

    private RawDetail loadedRawDetail;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserFixture()
    {
      this.loadedRawDetail = DxfParser.LoadDxfStream(DxfTestFilename);
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
      this.loadedNfp.Area.Should().Be(121F);
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

    [Fact]
    public void GivenCreatedOneSquareWhenComparedToLoadedOneSquareThenShouldBeEquivalent()
    {
      var created = DxfParser.ConvertDxfToRawDetail(this.loadedNfp.Name, new List<DxfEntity>() { DxfGenerator.Rectangle(11D) });
      created.Should().BeEquivalentTo(this.loadedRawDetail);
    }
  }
}
