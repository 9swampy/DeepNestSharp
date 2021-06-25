namespace DeepNestLib.CiTests
{
    using System;
    using System.IO;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class UnitTest1
    {
        const string oneSquareFilename = "SquareInSquare.dxf";
        private NestingContext nestingContext;
        private NFP loadedNfp;

        public UnitTest1()
        {
            var fi = new FileInfo(oneSquareFilename);
            var rawDetail = DxfParser.LoadDxf(oneSquareFilename);
            nestingContext = new NestingContext();
            nestingContext.TryImportFromRawDetail(rawDetail, A.Dummy<int>(), out loadedNfp);
        }

        [Fact]
        public void ShouldFindDxfInBuild()
        {
            var fi = new FileInfo(oneSquareFilename);
            System.Diagnostics.Debug.Print(fi.FullName);
            fi.Exists.Should().BeTrue();
        }

        [Fact]
        public void ShouldLoadDxf()
        {
            var fi = new FileInfo(oneSquareFilename);
            DxfParser.LoadDxf(oneSquareFilename);
        }

        [Fact]
        public void ShouldLoadDxfToRawDetail()
        {
            var fi = new FileInfo(oneSquareFilename);
            var rawDetail = DxfParser.LoadDxf(oneSquareFilename);

            rawDetail.Should().NotBeNull();
        }

        [Fact]
        public void ShouldReturnAParentNfp()
        {
            loadedNfp.Should().NotBeNull();
        }

        [Fact]
        public void ShouldHaveExpectedArea()
        {
            loadedNfp.Area.Should().Be(2500F);
        }

        [Fact]
        public void ShouldHaveExpectedRotation()
        {
            loadedNfp.Rotation.Should().Be(0F);
        }

        [Fact]
        public void ShouldHaveExpectedChildrenCount()
        {
            loadedNfp.children.Count.Should().Be(0);
        }
    }
}
