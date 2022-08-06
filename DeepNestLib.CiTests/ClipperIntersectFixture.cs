namespace DeepNestLib.CiTests
{
  using System.Collections.Generic;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.Geometry;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class ClipperIntersectFixture
  {
    [Fact]
    public void GivenSquareWhenClipToTriangleThenShouldMatchTriangle()
    {
      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
      var triangle = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().IsoscelesTriangle(10) }));
      var clip = NfpSimplifier.ClipSubject(triangle, square, new TestSvgNestConfig().ClipperScale);

      clip.Should().NotBeNull();
      clip.Area.Should().Be(triangle.Area);
      clip.EnsureIsClosed();
      clip.Should().BeEquivalentTo(triangle, "it's been forced by EnsureIsClosed.");
    }

    [Fact]
    public void AreaShouldMatchGeometryUtil()
    {
      // I don't understand why one's a negative, the other positive. Flipping all to positive causes polygons to fit inside other polygons; needs investigation.
      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
      ((double)square.Area).Should().BeApproximately(-GeometryUtil.PolygonArea(square), 1);
    }

    [Fact]
    public void GivenSquareWhenNotOverlappedThenShouldIndicateNoIntersect()
    {
      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
      var shiftedsquare = square.Shift(20, 20);
      NfpSimplifier.IsIntersect(square, shiftedsquare, new TestSvgNestConfig().ClipperScale).Should().BeFalse();
    }

    [Fact]
    public void GivenSquareWhenFullOverlappedThenShouldIndicateIntersect()
    {
      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
      NfpSimplifier.IsIntersect(square, square, new TestSvgNestConfig().ClipperScale).Should().BeTrue();
    }

    [Fact]
    public void GivenSquareWhenPartOverlappedThenShouldIndicateIntersect()
    {
      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) }));
      var shiftedsquare = square.Shift(5, 5);
      NfpSimplifier.IsIntersect(square, shiftedsquare, new TestSvgNestConfig().ClipperScale).Should().BeTrue();
    }

    [Fact]
    public void GivenSquareWithHoleWhenSquareInHoleThenShouldNotIndicateIntersect()
    {
      var squareWithHole = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(40) }));
      var hole = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(20) })).Shift(10, 10);
      squareWithHole.Children.Add(hole);

      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(10) })).Shift(15, 15);

      square.Overlaps(squareWithHole).Should().BeFalse();
    }

    [Fact]
    public void GivenSquareWithHoleWhenExactSquareInHoleThenShouldNotIndicateIntersect()
    {
      var squareWithHole = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(40) }));
      var hole = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(20) })).Shift(10, 10);
      squareWithHole.Children.Add(hole);

      var square = SvgNest.CleanPolygon2(NoFitPolygon.FromDxf(new List<DxfEntity>() { new DxfGenerator().Square(20) })).Shift(10, 10);

      square.Overlaps(squareWithHole).Should().BeFalse();
    }
  }
}
