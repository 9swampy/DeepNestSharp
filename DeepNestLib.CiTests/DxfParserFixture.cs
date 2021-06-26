namespace DeepNestLib.CiTests
{
    using System;
    using System.IO;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class DxfParserFixture
    {
        private const string DxfTestFilename = "SquareInSquare.dxf";
        private NestingContext nestingContext;
        private NFP loadedNfp;
        private bool hasImportedRawDetail;

        public DxfParserFixture()
        {
            var fi = new FileInfo(DxfTestFilename);
            var rawDetail = DxfParser.LoadDxf(DxfTestFilename);
            this.nestingContext = new NestingContext();
            hasImportedRawDetail = this.nestingContext.TryImportFromRawDetail(rawDetail, A.Dummy<int>(), out this.loadedNfp);
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
            var fi = new FileInfo(DxfTestFilename);
            DxfParser.LoadDxf(DxfTestFilename);
        }

        [Fact]
        public void ShouldLoadDxfToRawDetail()
        {
            var fi = new FileInfo(DxfTestFilename);
            var rawDetail = DxfParser.LoadDxf(DxfTestFilename);

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
        public void ShouldHaveExpectedChildrenCount()
        {
            this.loadedNfp.children.Count.Should().Be(0);
        }
    }
}
