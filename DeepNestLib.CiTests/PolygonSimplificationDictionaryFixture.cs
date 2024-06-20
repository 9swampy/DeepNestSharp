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
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey item = new PolygonSimplificationKey(new SvgPoint[0], null, true, true);
      sut.Add(item, item.Item1);
      Action act = () => sut.Add(item, item.Item1);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenEmptyAddedWhenAnotherEmptyAddedAgainThenThrowArgumentException()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[0], null, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[0], null, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAddedWhenAnotherAddedThenShouldNotThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(2, 2) }, null, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherSameAddedThenShouldThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyNullToleranceToSameByDefaultDifferenceAddedThenShouldNotThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 1F, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().Throw<ArgumentException>("null Tolerance defaults to 1");
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyNullToleranceDifferenceAddedThenShouldNotThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, null, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyNonNullToleranceDifferenceAddedThenShouldNotThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 1F, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyRadialDifferenceAddedThenShouldNotThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, false, true);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyPDDifferenceAddedThenShouldNotThrow()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, false);
      Action act = () => sut.Add(second, second.Item1);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenAddedWhenAnotherOnlyPDDifferenceCheckedThenNotContains()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, false);
      sut.ContainsKey(second).Should().BeFalse();
    }

    [Fact]
    public void GivenAddedWhenAnotherSameCheckedThenContains()
    {
      PolygonSimplificationDictionary sut = new PolygonSimplificationDictionary();
      PolygonSimplificationKey first = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.Add(first, first.Item1);
      PolygonSimplificationKey second = new PolygonSimplificationKey(new SvgPoint[] { new SvgPoint(1, 1) }, 0F, true, true);
      sut.ContainsKey(second).Should().BeTrue();
    }

    [Fact]
    public void GivenSameSvgPointsWhenComparedShouldBeEqual()
    {
      SvgPoint first = new SvgPoint(1, 1);
      SvgPoint second = new SvgPoint(1, 1);

      EqualityComparer<SvgPoint>.Default.Equals(first, second).Should().BeTrue();
    }

    [Fact]
    public void GivenDifferentSvgPointsWhenComparedShouldNotBeEqual()
    {
      SvgPoint first = new SvgPoint(1, 1);
      SvgPoint second = new SvgPoint(1, 2);

      EqualityComparer<SvgPoint>.Default.Equals(first, second).Should().BeFalse();
    }
  }
}
