namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FluentAssertions;
  using Xunit;

  public class PolygonSimplificationDictionaryFixture
  {
    [Fact]
    public void GivenAddedWhenAddedAgainThenThrowArgumentException()
    {
      var sut = new PolygonSimplificationDictionary();
      var item = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[0], null, true, true);
      sut.Add(item, item.Item1);
      Action act = () => sut.Add(item, item.Item1);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenEmptyAddedWhenAnotherEmptyAddedAgainThenThrowArgumentException()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[0], null, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[0], null, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAddedWhenAnotherAddedThenShouldNotThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(2, 2) }, null, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherSameAddedThenShouldThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyNullToleranceToSameByDefaultDifferenceAddedThenShouldNotThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 1F, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().Throw<ArgumentException>("null Tolerance defaults to 1");
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyNullToleranceDifferenceAddedThenShouldNotThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyNonNullToleranceDifferenceAddedThenShouldNotThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 1F, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyRadialDifferenceAddedThenShouldNotThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, false, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyPDDifferenceAddedThenShouldNotThrow()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, false);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyPDDifferenceCheckedThenNotContains()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, false);
      sut.ContainsKey(second).Should().BeFalse();
    }

    [Fact]
    public void GivenAddedWhenAnotherSameCheckedThenContains()
    {
      var sut = new PolygonSimplificationDictionary();
      var first = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      var second = new Tuple<SvgPoint[], double?, bool, bool>(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.ContainsKey(second).Should().BeTrue();
    }

    [Fact]
    public void GivenSameSvgPointsWhenComparedShouldBeEqual()
    {
      var first = new SvgPoint(1, 1);
      var second = new SvgPoint(1, 1);

      EqualityComparer<SvgPoint>.Default.Equals(first, second).Should().BeTrue();
    }

    [Fact]
    public void GivenDifferentSvgPointsWhenComparedShouldNotBeEqual()
    {
      var first = new SvgPoint(1, 1);
      var second = new SvgPoint(1, 2);

      EqualityComparer<SvgPoint>.Default.Equals(first, second).Should().BeFalse();
    }
  }
}
