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

  public class DxfParserFixture
  {
    private const string DxfTestFilename = "OneSquare.dxf";

    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();

    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private NFP loadedNfp;
    private bool hasImportedRawDetail;

    public DxfParserFixture()
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
