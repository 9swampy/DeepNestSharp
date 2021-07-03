namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
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

    private static NFP GetNfp(List<DxfEntity> dxfEntities)
    {
      RawDetail raw;
      NestingContext ctx;
      raw = DxfParser.ConvertDxfToRawDetail(string.Empty, dxfEntities);
      ctx = new NestingContext(A.Fake<IMessageService>());
      NFP result;
      _ = ctx.TryImportFromRawDetail(raw, 0, out result);
      return result;
    }
  }
}
