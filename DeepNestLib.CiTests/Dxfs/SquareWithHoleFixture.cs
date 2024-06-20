namespace DeepNestLib.CiTests.Dxfs
{
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using FluentAssertions;
  using Xunit;

  public class SquareWithHoleFixture
  {
    private readonly INfp squareWithHole;

    public SquareWithHoleFixture()
    {
      DxfGenerator generator = new DxfGenerator();
      INfp nfp = generator.GenerateSquare("outer", 20, RectangleType.TopRightAntiClockwise, true).ToNfp();
      INfp hole = generator.GenerateSquare("hole", 10, RectangleType.BottomLeftClockwise).ToNfp();
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
    public void GivenSquareWithHoleWhenNetAreaThenShouldBeExpected()
    {
      squareWithHole.NetArea.Should().Be(20 * 20 - 10 * 10, "calculation discounts for the hole.");
    }

    [Fact]
    public void GivenGeneratedDxfWhenLoadedThenAreaExpected()
    {
      using (Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Dxfs.20x20outer10x10hole.dxf"))
      {
        INfp sut = DxfParser.LoadDxfStream("loaded", inputStream).ToNfp();
        sut.Area.Should().Be(20 * 20);
      }
    }

    [Fact]
    public void GivenGeneratedDxfWhenComparedToGeneratedNfpThenExpectEquivalent()
    {
      using (Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Dxfs.20x20outer10x10hole.dxf"))
      {
        INfp actual = DxfParser.LoadDxfStream("outer", inputStream).ToNfp();
        actual.Children[0].Name = "hole";
        actual.Should().BeEquivalentTo(squareWithHole);
      }
    }

    [Fact]
    public void GivenGeneratedDxfWhenToOpenScadPolygonThenExpectedValue()
    {
      using (Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Dxfs.20x20outer10x10hole.scad"))
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

    [Fact]
    public void GivenSquareWithHoleWhenSelfOverlappedThenShouldIndicateIntersect()
    {
      NfpSimplifier.IsIntersect(squareWithHole, squareWithHole, new TestSvgNestConfig().ClipperScale).Should().BeTrue();
    }

    [Fact]
    public void GivenSquareWithHoleWhenOverlappedThenShouldIndicateIntersect()
    {
      INfp second = new DxfGenerator().GenerateSquare("hole", 20, RectangleType.BottomLeftClockwise).ToNfp();

      NfpSimplifier.IsIntersect(squareWithHole, second, new TestSvgNestConfig().ClipperScale).Should().BeTrue();
    }

    [Fact]
    public void GivenSquareWhenOverlappedWithHoleThenShouldIndicateIntersect()
    {
      INfp second = new DxfGenerator().GenerateSquare("hole", 20, RectangleType.BottomLeftClockwise).ToNfp();

      NfpSimplifier.IsIntersect(second, squareWithHole, new TestSvgNestConfig().ClipperScale).Should().BeTrue();
    }

    [Fact]
    public void GivenSquareWithHoleWhenHoleOverlappedThenShouldIndicateNoIntersect()
    {
      INfp second = new DxfGenerator().GenerateSquare("hole", 10, RectangleType.BottomLeftClockwise).ToNfp();
      NfpSimplifier.IsIntersect(squareWithHole, second.Shift(5, 5), new TestSvgNestConfig().ClipperScale).Should().BeFalse();
    }

    [Fact]
    public void GivenSquareWithHoleWhenHoleOnlyPartOverlappedThenShouldIndicateIntersect()
    {
      INfp second = new DxfGenerator().GenerateSquare("hole", 10, RectangleType.BottomLeftClockwise).ToNfp();
      NfpSimplifier.IsIntersect(squareWithHole, second.Shift(4, 4), new TestSvgNestConfig().ClipperScale).Should().BeTrue();
    }
  }
}
