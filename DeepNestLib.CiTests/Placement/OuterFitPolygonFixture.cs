namespace DeepNestLib.CiTests.Placement
{
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using System;
  using System.Linq;
  using Xunit;

  public class OuterFitPolygonFixture
  {
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSmallerSheetWhenGetInnerNfpThenHasNoChildren(bool useDllImport)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 9, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, useDllImport);
      nfp.Should().NotBeNull();
      if (useDllImport)
      {
        nfp.Length.Should().Be(13);
      }
      else
      {
        nfp.Length.Should().Be(0, "only adding to make Clipper result same as DllImport, where we never care about the top level NFP only look for children");
      }

      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenSameSheetWhenSamePartThenGetInnerNfpHasNoChildren(bool useDllImport)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, useDllImport);
      nfp.Should().NotBeNull();
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpHasExpectedArea(bool useDllImport)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, useDllImport);
      nfp.Children.Should().NotBeEmpty("could fit part inside sheet.");
    }

    [Theory]
    [InlineData(true, 10, 10)]
    [InlineData(false, 10, 10)]
    [InlineData(true, 20, 20)]
    [InlineData(false, 20, 20)]
    [InlineData(true, 20, 30)]
    [InlineData(false, 20, 30)]
    public void GivenSheetWhenPartThenGetOuterNfpOutsideHasExpectedArea(bool useDllImport, double sheetSize, double partSize)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", sheetSize, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", partSize, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, useDllImport);
      nfp.Area.Should().BeGreaterThan(sheet.Area);
      nfp.Area.Should().Be(Math.Pow(sheetSize + partSize, 2));
    }

    [Theory]
    [InlineData(20, 10)]
    [InlineData(10, 20)]
    [InlineData(10, 30)]
    [InlineData(10, 10)]
    [InlineData(20, 20)]
    [InlineData(30, 30)]
    public void TwoSquareOuterNfp(double sheetSize, double partSize)
    {
      var sheet = new NfpGenerator().GenerateRectangle(A.Dummy<string>(), sheetSize, sheetSize, RectangleType.BottomLeftClockwise, true);
      var part = new NfpGenerator().GenerateRectangle(A.Dummy<string>(), partSize, partSize, RectangleType.BottomLeftClockwise, true);
      var outerNfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, false);
      ((double)outerNfp.Area).Should().BeApproximately(Math.Pow(partSize + sheetSize, 2), 0.01);
      outerNfp.Length.Should().Be(5, "we're expecting a square origin -partSize,-partSize to sheetSize,sheetSize which represents all the points the origin (0,0) of where the part could be placed circumnavigating the sheet");
      outerNfp.Points.Min(o => o.X).Should().Be(-partSize);
      outerNfp.Points.Min(o => o.Y).Should().Be(-partSize);
      outerNfp.Points.Max(o => o.X).Should().Be(sheetSize);
      outerNfp.Points.Max(o => o.Y).Should().Be(sheetSize);
    }

    [Theory]
    [InlineData(20, 20, 10, 10)]
    [InlineData(10, 10, 20, 20)]
    [InlineData(10, 10, 30, 30)]
    [InlineData(10, 10, 10, 10)]
    [InlineData(20, 20, 20, 20)]
    [InlineData(30, 30, 30, 30)]
    [InlineData(160, 110, 50, 50)]
    public void TwoRectangleOuterNfp(double sheetWidth, double sheetHeight, double partWidth, double partHeight)
    {
      var sheet = new NfpGenerator().GenerateRectangle(A.Dummy<string>(), sheetWidth, sheetHeight, RectangleType.BottomLeftClockwise, true);
      var part = new NfpGenerator().GenerateRectangle(A.Dummy<string>(), partWidth, partHeight, RectangleType.BottomLeftClockwise, true);
      var outerNfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, false);
      ((double)outerNfp.Area).Should().BeApproximately((partWidth + sheetWidth) * (partHeight + sheetHeight), 0.01);
      outerNfp.Length.Should().Be(5, "we're expecting a square origin -partSize,-partSize to sheetSize,sheetSize which represents all the points the origin (0,0) of where the part could be placed circumnavigating the sheet");
      outerNfp.Points.Min(o => o.X).Should().Be(-partWidth);
      outerNfp.Points.Min(o => o.Y).Should().Be(-partHeight);
      outerNfp.Points.Max(o => o.X).Should().Be(sheetWidth);
      outerNfp.Points.Max(o => o.Y).Should().Be(sheetHeight);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Given20x20AWhen10x10BThenGetOuterNfpABShouldHaveExpectedPoints(bool useDllImport)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.BottomLeftClockwise).ToSheet();
      sheet.Clean();
      var part = generator.GenerateSquare("part", 10, RectangleType.BottomLeftClockwise).ToNfp();
      var outerNfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, useDllImport);
      outerNfp.Length.Should().Be(5);
      outerNfp.Points.Min(o => o.X).Should().Be(-10);
      outerNfp.Points.Min(o => o.Y).Should().Be(-10);
      outerNfp.Points.Max(o => o.X).Should().Be(20);
      outerNfp.Points.Max(o => o.Y).Should().Be(20);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpShouldHaveExpectedPoints(bool useDllImport)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.BottomLeftClockwise).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.BottomLeftClockwise).ToNfp();
      var outerNfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, useDllImport);
      outerNfp.Length.Should().Be(5, "we're expecting a square origin -10,-10 20x20=> 10,10 which represents all the points the origin (0,0) of where the part could be placed circumnavigating the sheet");
      outerNfp.Points.Min(o => o.X).Should().Be(-10);
      outerNfp.Points.Min(o => o.Y).Should().Be(-10);
      outerNfp.Points.Max(o => o.X).Should().Be(10);
      outerNfp.Points.Max(o => o.Y).Should().Be(10);
    }
  }
}
