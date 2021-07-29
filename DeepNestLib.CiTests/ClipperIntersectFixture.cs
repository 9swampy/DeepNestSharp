namespace DeepNestLib.CiTests
{
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class ClipperIntersectFixture
  {
    [Fact]
    public void GivenSquareWhenClipToTriangleThenShouldMatchTriangle()
    {
      var square = SvgNest.cleanPolygon2(GetNfp(new List<DxfEntity>() { new DxfGenerator().Rectangle(10) }));
      var triangle = SvgNest.cleanPolygon2(GetNfp(new List<DxfEntity>() { new DxfGenerator().IsoscelesTriangle(10) }));
      var clip = SvgNest.ClipSubject(triangle, square, new SvgNestConfig().ClipperScale);

      clip.Should().NotBeNull();
      clip.Area.Should().Be(triangle.Area);
      clip.Should().BeEquivalentTo(triangle);
    }

    private static INfp GetNfp(List<DxfEntity> dxfEntities)
    {
      RawDetail raw;
      NestingContext ctx;
      raw = DxfParser.ConvertDxfToRawDetail(string.Empty, dxfEntities);
      ctx = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      INfp result;
      raw.TryConvertToNfp(0, out result);
      return result;
    }

    [Fact]
    public void AreaShouldMatchGeometryUtil()
    {
      // I don't understand why one's a negative, the other positive. Flipping all to positive causes polygons to fit inside other polygons; needs investigation.
      var square = SvgNest.cleanPolygon2(GetNfp(new List<DxfEntity>() { new DxfGenerator().Rectangle(10) }));
      ((double)square.Area).Should().BeApproximately(-GeometryUtil.polygonArea(square), 1);
    }
  }
}
