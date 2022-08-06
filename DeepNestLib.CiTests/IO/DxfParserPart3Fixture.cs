﻿namespace DeepNestLib.CiTests.IO
{
  using System;
  using System.Linq;
  using DeepNestLib.Geometry;
  using DeepNestLib.IO;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfParserPart3Fixture
  {
    private const string DxfTestFilename = "Dxfs._3.dxf";

    private RawDetail<DxfEntity> loadedRawDetail;
    private DxfFile loadedDxfFile;
    private INfp loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserPart3Fixture()
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
    public void ChildAreasShouldBeSame()
    {
      GeometryUtil.PolygonArea(loadedRawDetail.Outers.Where(o => o.IsChild).Single().Points).Should().Be(loadedNfp.Children.Single().Area);
    }
  }
}
