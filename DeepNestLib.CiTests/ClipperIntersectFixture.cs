namespace DeepNestLib.CiTests
{
  using System.Collections.Generic;
  using DeepNestLib.Geometry;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class ClipperIntersectFixture
  {
    [Fact]
    public void GivenSquareWhenClipToTriangleThenShouldMatchTriangle()
    {
      var square = SvgNest.CleanPolygon2(GetNfp(new List<DxfEntity>() { new DxfGenerator().Rectangle(10) }));
      var triangle = SvgNest.CleanPolygon2(GetNfp(new List<DxfEntity>() { new DxfGenerator().IsoscelesTriangle(10) }));
      var clip = SvgNest.ClipSubject(triangle, square, new TestSvgNestConfig().ClipperScale);

      clip.Should().NotBeNull();
      clip.Area.Should().Be(triangle.Area);
      clip.EnsureIsClosed();
      clip.Should().BeEquivalentTo(triangle, "it's been forced by EnsureIsClosed.");
    }

    private static INfp GetNfp(List<DxfEntity> dxfEntities)
    {
      RawDetail raw;
      raw = DxfParser.ConvertDxfToRawDetail(string.Empty, dxfEntities);
      INfp result;
      raw.TryConvertToNfp(0, out result);
      return result;
    }

    [Fact]
    public void AreaShouldMatchGeometryUtil()
    {
      // I don't understand why one's a negative, the other positive. Flipping all to positive causes polygons to fit inside other polygons; needs investigation.
      var square = SvgNest.CleanPolygon2(GetNfp(new List<DxfEntity>() { new DxfGenerator().Rectangle(10) }));
      ((double)square.Area).Should().BeApproximately(-GeometryUtil.PolygonArea(square), 1);
    }
  }
}
