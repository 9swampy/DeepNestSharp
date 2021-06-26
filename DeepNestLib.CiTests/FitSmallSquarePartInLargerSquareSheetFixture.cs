namespace DeepNestLib.CiTests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using IxMilia.Dxf.Entities;
    using Xunit;

    public class FitSmallSquarePartInLargerSquareSheetFixture
    {
        private static readonly DxfGenerator DxfGenerator = new DxfGenerator();

        [Fact]
        private void TesT()
        {
            var nestingContext = new NestingContext();
            NFP sheet;
            nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(22D) }), 0, out sheet).Should().BeTrue();
            NFP part;
            nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }), 0, out part).Should().BeTrue();

            var frame = Background.getFrame(sheet);

            Background.Process2(frame, part, 0).Should().NotBeNull();
        }
    }
}
