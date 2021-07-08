namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class ClipperFixture
  {
    [Fact]
    public void ArrayHashCodeShouldMatchDeprecated()
    {
      var source = new SvgPoint[] { new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()), new SvgPoint(new Random().Next(), new Random().Next()) };
      var original = new NFP(source);
      var alternate = new NFP(source);
      var sut = new DeepNestClipper() as IDeprecatedClipper;
      sut.ScaleUpPathsOriginal(original, 1D).Should().BeEquivalentTo(DeepNestClipper.ScaleUpPaths(alternate.Points, 1D));
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
      var sut = new DeepNestClipper() as IDeprecatedClipper;
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
        _ = DeepNestClipper.ScaleUpPaths(original.Points, 1D);
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
