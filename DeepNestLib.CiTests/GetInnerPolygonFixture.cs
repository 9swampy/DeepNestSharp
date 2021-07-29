namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class GetInnerPolygonFixture
  {
    [Fact]
    public void GivenSameSheetWhenSamePartThenGetInnerNfpShouldBeNull()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), MinkowskiSum.CreateInstance()).GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000);
      nfp.Should().BeNull("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldNotBeNull()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), MinkowskiSum.CreateInstance()).GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000);
      nfp.Should().NotBeNull("could fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldNotHaveOneNfp()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), MinkowskiSum.CreateInstance()).GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000);
      nfp.Length.Should().Be(1);
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpShouldNotHaveExpectedArea()
    {
      var generator = new DxfGenerator();
      var smaller = new Random().NextDouble() * 200;
      var larger = smaller + (new Random().NextDouble() * 200);
      var sheet = generator.GenerateSquare("sheet", larger, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", smaller, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), MinkowskiSum.CreateInstance()).GetInnerNfp(sheet, part, MinkowskiCache.NoCache, 100000);
      nfp[0].Area.Should().BeApproximately((float)Math.Pow(larger - smaller, 2), 0.01f);
    }
  }
}
