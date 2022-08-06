namespace DeepNestLib.CiTests.IO
{
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib.IO;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfParserFixture
  {
    private const string DxfTestFilename = "Dxfs.OneSquare.dxf";

    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();

    private IRawDetail loadedRawDetail;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserFixture()
    {
      loadedRawDetail = DxfParser.LoadDxfFileStreamAsRawDetail(DxfTestFilename);
      hasImportedRawDetail = loadedRawDetail.TryConvertToNfp(A.Dummy<int>(), out loadedNfp);
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
      loadedNfp.Area.Should().Be(121F);
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
    public void GivenCreatedOneSquareWhenComparedToLoadedOneSquareThenShouldBeEquivalent()
    {
      var created = DxfParser.ConvertDxfToRawDetail(loadedNfp.Name, new List<DxfEntity>() { DxfGenerator.Square(11D) });
      created.Should().BeEquivalentTo(loadedRawDetail);
    }
  }
}
