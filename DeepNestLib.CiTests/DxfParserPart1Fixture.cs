namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfParserPart1Fixture
  {
    private const string DxfTestFilename = "_1.dxf";

    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private NFP loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserPart1Fixture()
    {
      this.loadedRawDetail = DxfParser.LoadDxf(DxfTestFilename);
      this.nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      this.hasImportedRawDetail = this.nestingContext.TryImportFromRawDetail(this.loadedRawDetail, A.Dummy<int>(), out this.loadedNfp);
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
      this.loadedNfp.Area.Should().Be(2500F);
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
