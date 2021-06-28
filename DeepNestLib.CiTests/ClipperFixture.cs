namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using FluentAssertions;
  using Xunit;

  public partial class DxfParserPart5Fixture
  {
    public class ClipperFixture
    {
      [Fact]
      public void ArrayHashCodeShouldMatchDeprecated()
      {
        var source = new SvgPoint[] { new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()) };
        var original = new NFP(source);
        var alternate = new NFP(source);
        var sut = new _Clipper() as IDeprecatedClipper;
        sut.ScaleUpPaths(original, 1D).Should().BeEquivalentTo(_Clipper.ScaleUpPaths(alternate.Points, 1D));
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
        long deprecated = 0;
        long current = 0;
        long alt = 0;
        for (int i = 0; i < 100; i++)
        {
          sw.Start();
          _ = sut.ScaleUpPaths(original, 1D);
          deprecated += sw.ElapsedTicks;
          sw.Reset();
          sw.Start();
          _ = _Clipper.ScaleUpPaths(original.Points, 1D);
          current += sw.ElapsedTicks;
          sw.Reset();
          sw.Start();
          _ = _Clipper.ScaleUpPathsAlt(original.Points, 1D);
          alt += sw.ElapsedTicks;
          sw.Reset();
        }

        deprecated.Should().BeGreaterThan(current);
        Debug.Print($"{deprecated}>{current}");
        alt.Should().BeGreaterThan(current);
        Debug.Print($"{alt}>{current}");
      }
    }
  }
}
