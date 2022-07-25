namespace DeepNestLib.CiTests
{
  using System;
  using System.Linq;
  using DeepNestLib.Geometry;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfParserPart2Fixture
  {
    private const string DxfTestFilename = "Dxfs._2.dxf";

    private RawDetail<DxfEntity> loadedRawDetail;
    private DxfFile loadedDxfFile;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserPart2Fixture()
    {
      this.loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
      this.loadedDxfFile = DxfParser.LoadDxfFileStream(DxfTestFilename);
      this.hasImportedRawDetail = this.loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out this.loadedNfp);
    }

    [Fact]
    public void ShouldLoadDxf()
    {
      Action act = () => _ = DxfParser.LoadDxfFile(DxfTestFilename);
      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldLoadDxfToRawDetail()
    {
      var rawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);

      rawDetail.Should().NotBeNull();
    }

    [Fact]
    public void ShouldHaveOneChild()
    {
      this.loadedNfp.Children.Count.Should().Be(1);
    }

    [Fact]
    public void RawDetailShouldHaveTwoContours()
    {
      this.loadedRawDetail.Outers.Count.Should().Be(2);
    }

    [Fact]
    public void RawDetailShouldHaveOneChildContour()
    {
      this.loadedRawDetail.Outers.Where(o => o.IsChild).Count().Should().Be(1);
    }

    [Fact]
    public void RawDetailShouldHaveOneNotChildContour()
    {
      this.loadedRawDetail.Outers.Where(o => !o.IsChild).Count().Should().Be(1);
    }

    [Fact]
    public void ChildAreasShouldBeSame()
    {
      Math.Abs(GeometryUtil.PolygonArea(this.loadedRawDetail.Outers.Where(o => o.IsChild).Single().Points)).Should().Be(loadedNfp.Children.Single().Area);
    }
  }
}
