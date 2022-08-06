namespace DeepNestLib.CiTests.IO
{
  using System;
  using System.Drawing;
  using System.Linq;
  using DeepNestLib.IO;
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
      loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
      loadedDxfFile = DxfParser.LoadDxfFileStream(DxfTestFilename);
      hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out loadedNfp);
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
      loadedNfp.Area.Should().BeApproximately(2500, 1);
    }

    [Fact]
    public void ShouldHaveExpectedRotation()
    {
      loadedNfp.Rotation.Should().Be(0F);
    }

    [Fact]
    public void ShouldHaveExpectedPointsCount()
    {
      loadedNfp.Points.Count().Should().Be(5);
    }

    [Fact]
    public void ShouldHaveNoNullPoints()
    {
      loadedNfp.Points.Where(o => o == null).Should().BeEmpty();
    }

    [Fact]
    public void ShouldHaveOneChild()
    {
      loadedNfp.Children.Count.Should().Be(1);
    }

    [Fact]
    public void RawDetailShouldHaveTwoContours()
    {
      loadedRawDetail.Outers.Count.Should().Be(2);
    }

    [Fact]
    public void SquareContourShouldHave5Points()
    {
      loadedRawDetail.Outers.Skip(1).First().Points.Count().Should().Be(5, "it's a closed square 4 + 1 to close");
    }

    [Fact]
    public void OutersShouldBeLocalContourDxfEntity()
    {
      loadedRawDetail.Outers.Should().AllBeOfType<LocalContour<DxfEntity>>();
    }

    [Fact]
    public void SquareEntitiesShouldBeDxfLine()
    {
      if (loadedRawDetail.Outers.Skip(1).First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Should().AllBeOfType<DxfLine>();
      };
    }

    [Fact]
    public void SquareEntitiesShouldBe4()
    {
      if (loadedRawDetail.Outers.Skip(1).First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Count.Should().Be(4);
      };
    }

    [Fact]
    public void CircleEntitiesShouldBeDxfCircle()
    {
      if (loadedRawDetail.Outers.First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Single().Should().BeOfType<DxfCircle>();
      };
    }

    [Fact]
    public void CircleEntitiesShouldBeASingleCircle()
    {
      if (loadedRawDetail.Outers.First() is LocalContour<DxfEntity> cast)
      {
        cast.Entities.Count.Should().Be(1);
      };
    }

    [Fact]
    public void SquareContourShouldHave4ExpectedPoints()
    {
      loadedRawDetail.Outers.Skip(1).First().Points.Should().BeEquivalentTo(
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
      loadedRawDetail.Outers.Skip(1).First().IsChild.Should().Be(false);
    }

    [Fact]
    public void CircleContourShouldHave24Points()
    {
      loadedRawDetail.Outers.First().Points.Count().Should().Be(25, "it's a circle represented by a line each 15 degrees + 1 to close");
    }

    [Fact]
    public void CircleContourShouldBeChild()
    {

      loadedRawDetail.Outers.First().IsChild.Should().Be(true);
    }

    [Fact]
    public void DxfFileShouldHaveTwoEntities()
    {
      loadedDxfFile.Entities.Count.Should().Be(5, "four lines making up the square, plus the inner circle.");
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
