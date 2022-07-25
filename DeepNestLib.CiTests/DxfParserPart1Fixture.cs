namespace DeepNestLib.CiTests
{
  using System;
  using System.Drawing;
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfParserPart1Fixture
  {
    private const string DxfTestFilename = "Dxfs._1.dxf";

    private RawDetail<DxfEntity> loadedRawDetail;
    private DxfFile loadedDxfFile;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserPart1Fixture()
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
    public void SquareContourShouldHave5Points()
    {
      this.loadedRawDetail.Outers.Skip(1).First().Points.Count().Should().Be(5, "it's a closed square 4 + 1 to close");
    }

    [Fact]
    public void OutersShouldBeLocalContourDxfEntity()
    {
      this.loadedRawDetail.Outers.Should().AllBeOfType<LocalContour<DxfEntity>>();
    }

    [Fact]
    public void SquareEntitiesShouldBeDxfLine()
    {
      if (this.loadedRawDetail.Outers.Skip(1).First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Should().AllBeOfType<DxfLine>();
      };
    }

    [Fact]
    public void SquareEntitiesShouldBe4()
    {
      if (this.loadedRawDetail.Outers.Skip(1).First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Count.Should().Be(4);
      };
    }

    [Fact]
    public void CircleEntitiesShouldBeDxfCircle()
    {
      if (this.loadedRawDetail.Outers.First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Single().Should().BeOfType<DxfCircle>();
      };
    }

    [Fact]
    public void CircleEntitiesShouldBeASingleCircle()
    {
      if (this.loadedRawDetail.Outers.First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Count.Should().Be(1);
      };
    }

    [Fact]
    public void SquareContourShouldHave4ExpectedPoints()
    {
      this.loadedRawDetail.Outers.Skip(1).First().Points.Should().BeEquivalentTo(
        new PointF[]
        {
          new PointF(33.7844f,-9.918432f),
          new PointF(-16.2155972f , -9.918432f),
          new PointF(-16.2155972f, 40.0815659f),
          new PointF(33.7844f ,40.0815659f),
          new PointF(33.7844f, -9.918432f),
        }, opt => opt.WithStrictOrdering());
    }

    [Fact]
    public void SquareContourShouldNotBeChild()
    {
      this.loadedRawDetail.Outers.Skip(1).First().IsChild.Should().Be(false);
    }

    [Fact]
    public void CircleContourShouldHave24Points()
    {
      this.loadedRawDetail.Outers.First().Points.Count().Should().Be(25, "it's a circle represented by a line each 15 degrees + 1 to close");
    }

    [Fact]
    public void CircleContourShouldBeChild()
    {

      this.loadedRawDetail.Outers.First().IsChild.Should().Be(true);
    }

    [Fact]
    public void DxfFileShouldHaveTwoEntities()
    {
      this.loadedDxfFile.Entities.Count.Should().Be(5, "four lines making up the square, plus the inner circle.");
    }

    [Fact]
    public void GivenLoadedDxfFileWhenGetOuterThenExpectSquare()
    { }

    [Fact]
    public void GivenLoadedDxfFileWhenGetOuterThenExpectCircle()
    { }

    [Fact]
    public void GivenLoadedDxfFileWhenGetPolygonsThenExpectBothSquareAndChildCircle()
    { }
  }
}
