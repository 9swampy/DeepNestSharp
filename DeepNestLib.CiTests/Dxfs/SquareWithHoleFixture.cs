namespace DeepNestLib.CiTests.Dxfs
{
  using System.IO;
  using System.Reflection;
  using FluentAssertions;
  using Xunit;

  public class SquareWithHoleFixture
  {
    INfp squareWithHole;

    public SquareWithHoleFixture()
    {
      var generator = new DxfGenerator();
      var nfp = generator.GenerateSquare("outer", 20, RectangleType.TopRightAntiClockwise, true).ToNfp();
      var hole = generator.GenerateSquare("hole", 10, RectangleType.BottomLeftClockwise).ToNfp();
      hole = hole.Shift(5, 5);
      hole.Name = "hole";
      hole.X = 0;
      hole.Y = 0;
      nfp.Children.Add(hole);
      squareWithHole = nfp;
    }

    [Fact]
    public void GivenSquareWithHoleWhenAreaThenShouldBeExpected()
    {
      squareWithHole.Area.Should().Be(20 * 20, "the hole doesn't get discounted.");
    }

    [Fact]
    public void GivenGeneratedDxfWhenLoadedThenAreaExpected()
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Dxfs.20x20outer10x10hole.dxf"))
      {
        var sut = DxfParser.LoadDxfStream("loaded", inputStream).ToNfp();
        sut.Area.Should().Be(20 * 20);
      }
    }

    [Fact]
    public void GivenGeneratedDxfWhenComparedToGeneratedNfpThenExpectEquivalent()
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Dxfs.20x20outer10x10hole.dxf"))
      {
        var actual = DxfParser.LoadDxfStream("outer", inputStream).ToNfp();
        actual.Children[0].Name = "hole";
        actual.Should().BeEquivalentTo(squareWithHole);
      }
    }

    [Fact]
    public void GivenGeneratedDxfWhenToOpenScadPolygonThenExpectedValue()
    {
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Dxfs.20x20outer10x10hole.scad"))
      using (StreamReader reader = new StreamReader(inputStream))
      {
        var openScad = squareWithHole.ToOpenScadPolygon();
        openScad.Should().Be(reader.ReadToEnd());
      }
    }

    [Fact]
    public void GivenSimpleSquareWhenGetMaxXThenShouldBeExpected()
    {
      squareWithHole.MaxX.Should().Be(20);
    }

    [Fact]
    public void GivenSimpleSquareWhenGetMinXThenShouldBeExpected()
    {
      squareWithHole.MinX.Should().Be(0);
    }

    [Fact]
    public void GivenSimpleSquareWhenGetMaxYThenShouldBeExpected()
    {
      squareWithHole.MaxY.Should().Be(20);
    }

    [Fact]
    public void GivenSimpleSquareWhenGetMinYThenShouldBeExpected()
    {
      squareWithHole.MinY.Should().Be(0);
    }
  }
}
