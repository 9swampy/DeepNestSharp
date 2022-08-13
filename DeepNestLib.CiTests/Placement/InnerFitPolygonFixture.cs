namespace DeepNestLib.CiTests.Placement
{
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class InnerFitPolygonFixture
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
  }
}
