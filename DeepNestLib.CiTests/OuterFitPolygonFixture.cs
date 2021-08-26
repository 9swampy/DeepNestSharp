namespace DeepNestLib.CiTests
{
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class OuterFitPolygonFixture
  {
    [Fact]
    public void GivenSmallerSheetWhenGetInnerNfpThenHasNoChildren()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 9, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Inner);
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenSameSheetWhenSamePartThenGetInnerNfpHasNoChildren()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Inner);
      nfp.Children.Should().BeEmpty("couldn't fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Inner);
      nfp.Children.Should().NotBeEmpty("could fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpOutsideHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, NoFitPolygonType.Outer);
      nfp.Area.Should().Be(400);
    }
  }
}
