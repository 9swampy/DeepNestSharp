namespace DeepNestLib.CiTests
{
  using FakeItEasy;
  using FluentAssertions;
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

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetInnerNfpHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 20, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetInnerNfp(sheet, part, MinkowskiCache.NoCache, A.Dummy<bool>());
      nfp.Children.Should().NotBeEmpty("could fit part inside sheet.");
    }

    [Fact]
    public void GivenLargerSheetWhenSmallerPartThenGetOuterNfpOutsideHasExpectedArea()
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("sheet", 10, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("part", 10, RectangleType.FileLoad).ToNfp();
      var nfp = A.Dummy<NfpHelper>().GetOuterNfp(sheet, part, MinkowskiCache.NoCache, A.Dummy<bool>());
      nfp.Area.Should().Be(400);
    }
  }
}
