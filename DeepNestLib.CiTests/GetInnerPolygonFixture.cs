namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class GetInnerPolygonFixture
  {
    [Fact]
    public void GivenSameSheetWhenSamePartThenGetInnerNfpShouldBeNull()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000, A.Dummy<bool>());
      nfp.Should().BeNull("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldNotBeNull()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000, A.Dummy<bool>());
      nfp.Should().NotBeNull("could fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldNotHaveOneNfp()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000, A.Dummy<bool>());
      nfp.Length.Should().Be(1);
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldHaveExpectedArea()
    {
      var generator = new DxfGenerator();
      var smaller = new Random().NextDouble() * 200;
      var larger = smaller + (new Random().NextDouble() * 200);
      var sheet = generator.GenerateSquare("sheet", larger, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", smaller, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000, A.Dummy<bool>());
      nfp[0].Area.Should().BeApproximately((float)Math.Pow(larger - smaller, 2), 0.01f);
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldHaveExpectedPoints()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.BottomLeftClockwise).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.BottomLeftClockwise).ToNfp();
      var innerNfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000, A.Dummy<bool>());
      innerNfp.Length.Should().Be(1);
      innerNfp[0].Length.Should().Be(5, "we're expecting a closed square origin 0,0 10x10 which represents all the points the origin (0,0) of where the part could fit on the sheet");
      innerNfp[0][0].X.Should().Be(10);
      innerNfp[0][0].Y.Should().Be(0);
      innerNfp[0][1].X.Should().Be(0);
      innerNfp[0][1].Y.Should().Be(0);
      innerNfp[0][2].X.Should().Be(0);
      innerNfp[0][2].Y.Should().Be(10);
      innerNfp[0][3].X.Should().Be(10);
      innerNfp[0][3].Y.Should().Be(10);
      innerNfp[0][4].X.Should().Be(10);
      innerNfp[0][4].Y.Should().Be(0);
    }
  }
}
