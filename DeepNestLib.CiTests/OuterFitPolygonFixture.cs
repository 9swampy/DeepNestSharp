namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class OuterFitPolygonFixture
  {
    [Fact]
    public void GivenSameSheetWhenSAmePartThenGetOuterNfpInsideHasNoChildren()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).GetOuterNfp(sheet, part, MinkowskiCache.NoCache, true);
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpInsideHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).GetOuterNfp(sheet, part, MinkowskiCache.NoCache, true);
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpOutsideHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).GetOuterNfp(sheet, part, MinkowskiCache.NoCache, false);
      nfp.Area.Should().Be(400);
    }
  }
}
