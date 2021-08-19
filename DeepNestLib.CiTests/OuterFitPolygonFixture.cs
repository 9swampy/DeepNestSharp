namespace DeepNestLib.CiTests
{
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
      var nfp = A.Dummy<NfpHelper>()
        .GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Inner);
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpInsideHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>()
        .GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Inner);
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpOutsideHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToNfp();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>()
        .GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Outer);
      nfp.Area.Should().Be(400);
    }
  }
}
