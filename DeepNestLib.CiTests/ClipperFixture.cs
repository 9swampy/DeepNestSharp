namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using ClipperLib;
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

  public class ClipperFixture
  {
    [Fact]
    public void ArrayHashCodeShouldMatchDeprecated()
    {
      var source = new SvgPoint[] { new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()) };
      var original = new NFP(source);
      var alternate = new NFP(source);
      var sut = new _Clipper() as IDeprecatedClipper;
      sut.ScaleUpPathsOriginal(original, 1D).Should().BeEquivalentTo(_Clipper.ScaleUpPaths(alternate.Points, 1D));
    }

    [Fact]
    public void CurrentImplementationOfScaleUpPathsShouldBeTheFastest()
    {
      int points = 2000;
      var source = new SvgPoint[points];
      for (int i = 0; i < points; i++)
      {
        source[i] = new SvgPoint(new Random().Next(), new Random().Next());
      }

      var original = new NFP(source);
      var sut = new _Clipper() as IDeprecatedClipper;
      var sw = new Stopwatch();
      long deprecatedOriginal = 0;
      long current = 0;
      long deprecatedSlowerParallel = 0;
      for (int i = 0; i < 100; i++)
      {
        sw.Start();
        _ = sut.ScaleUpPathsOriginal(original, 1D);
        deprecatedOriginal += sw.ElapsedTicks;
        sw.Reset();
        sw.Start();
        _ = _Clipper.ScaleUpPaths(original.Points, 1D);
        current += sw.ElapsedTicks;
        sw.Reset();
        sw.Start();
        _ = sut.ScaleUpPathsSlowerParallel(original.Points, 1D);
        deprecatedSlowerParallel += sw.ElapsedTicks;
        sw.Reset();
      }

      deprecatedOriginal.Should().BeGreaterThan(current);
      Debug.Print($"{deprecatedOriginal}>{current}");
      deprecatedSlowerParallel.Should().BeGreaterThan(current);
      Debug.Print($"{deprecatedSlowerParallel}>{current}");
    }
  }
}
