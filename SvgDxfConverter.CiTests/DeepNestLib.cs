namespace SvgDxfConverter.CiTests
{
    using System;
    using System.Collections.Generic;
    using global::DeepNestLib;
    using FluentAssertions;
    using Xunit;

    public class DeepNestLib
    {
        [Fact]
        public void GivenSvgFileWhenImportedShouldNotBeNull()
        {
            var raw = SvgParser.LoadSvg(@"D:\kabeja-0.4\ExportedDirect.svg");
            raw.Should().NotBeNull();
        }

        [Fact]
        public void GivenDxfFileWhenImportedShouldNotBeNull()
        {
            var raw = DxfParser.LoadDxf(@"D:\kabeja-0.4\ExportedDirect.dxf");
            raw.Should().NotBeNull();
        }

        [Fact]
        public void GivenSameImageWhenImportedShouldNotBeSame()
        {
            var rawDxf = DxfParser.LoadDxf(@"D:\kabeja-0.4\ExportedDirect.dxf");
            var rawSvg = SvgParser.LoadSvg(@"D:\kabeja-0.4\ExportedDirect.svg");

            rawDxf.Should().BeEquivalentTo(rawSvg);
        }

        [Fact]
        public void GivenSvgFileWhenLoadedToANestedContextShouldNotBeNull()
        {
            var nestingContext = new NestingContext();
            nestingContext.ImportFromRawDetail(SvgParser.LoadSvg(@"SimpleNestExport.svg"), 0).Should().NotBeNull();
        }

        [Fact]
        public void GivenSvgExportDxfTdd()
        {
            var nestingContext = new NestingContext();
            //var nest = nestingContext.ImportFromRawDetail(SvgParser.LoadSvg(@"D:\kabeja-0.4\ExportedDirect.svg"), 0);
            var nest = nestingContext.ImportFromRawDetail(SvgParser.LoadSvg(@"SimpleNestExport.svg"), 0);
            var sheets = new List<NFP>() { nest };
            DxfParser.Export(@"D:\kabeja-0.4\ExportedDirectDeepNestPort.dxf", nest.children, sheets);
        }
    }
}
