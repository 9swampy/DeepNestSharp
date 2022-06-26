namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class DxfLineMergerFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new DxfLineMerger();

      act.Should().NotThrow();
    }

    [Fact]
    public async Task GivenDxfLineWhenMergedThenExpectLine()
    {
      var entity = new DxfGenerator().Line(10);

      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(entity);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(1);
    }

    [Fact]
    public async Task GivenDxfRectangleWhenMergedThenExpectFourLines()
    {
      var closedRectangle = new DxfGenerator().Rectangle(10, RectangleType.Normal, true);

      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(4);
    }

    [Fact]
    public void GivenDxfRectangleTwiceWhenMergedThenExpectFourLines()
    {
      var closedRectangle = new DxfGenerator().Rectangle(10, RectangleType.Normal, true);

      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle);
      dxfFile.Entities.Add(closedRectangle);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(4);
    }

    [Fact]
    public void GivenIntersectingDxfRectanglesWhenMergedThenExpectSixLines()
    {
      var closedRectangle10 = new DxfGenerator().Rectangle(10, RectangleType.Normal, true);
      var closedRectangle20 = new DxfGenerator().Rectangle(20, RectangleType.Normal, true);

      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(6);
    }

    [Fact]
    public void GivenLinesWhenSameThenExpectCoaligned()
    {
      var line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      DxfLineMerger.Coaligned(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenCoalignedThenExpectCoaligned()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coaligned(left, right).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenNotCoincidentThenExpectFalse()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coincident(left, right).Should().BeFalse();
    }

    [Fact]
    public void GivenLinesWhenCoincidentThenExpectTrue()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(20, 0, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coincident(left, right).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenOverlappingThenExpectCoincident()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(25, 0, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coincident(left, right).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenSameThenExpectCoincident()
    {
      var line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      DxfLineMerger.Coincident(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenSingleLineWhenMergedThenExpectOneLine()
    {
      var line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      var list = new List<DxfLine>() { line };

      var sut = new DxfLineMerger();
      sut.MergeLines(list).Count().Should().Be(1);
    }

    [Fact]
    public void GivenSingleLineWhenMergedThenExpectSameLine()
    {
      var line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      var list = new List<DxfLine>() { line };

      var sut = new DxfLineMerger();
      sut.MergeLines(list).Single().Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenHorizontalDxfLineTwiceWhenMergedThenExpectOneLine()
    {
      var line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      var list = new List<DxfLine>() { line, line };

      var sut = new DxfLineMerger();
      sut.MergeLines(list).Count().Should().Be(1);
    }

    [Fact]
    public void GivenHorizontalDxfLineTwiceWhenMergedThenExpectSameLine()
    {
      var line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      var list = new List<DxfLine>() { line, line };

      var sut = new DxfLineMerger();
      sut.MergeLines(list).Single().Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenVerticalWhenCoalignedThenExpectCoaligned()
    {
      var line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));

      DxfLineMerger.Coaligned(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenVerticalWhenCoincidentThenExpectCoincident()
    {
      var line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));

      DxfLineMerger.Coincident(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenVerticalOverlappingThenExpectSame()
    {
      var line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));

      DxfLineMerger.GetCombined(line, line).Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenVerticalCoalignedWhenNotOverlappingThenExpectFalse()
    {
      var a = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));
      var b = new MergeLine(new DxfLine(new DxfPoint(0, 20, 0), new DxfPoint(0, 30, 0)));

      DxfLineMerger.Coincident(a, b).Should().BeFalse();
    }

    [Fact]
    public void GivenVerticalCoalignedWhenReverseNotOverlappingThenExpectFalse()
    {
      var a = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));
      var b = new MergeLine(new DxfLine(new DxfPoint(0, 30, 0), new DxfPoint(0, 20, 0)));

      DxfLineMerger.Coincident(a, b).Should().BeFalse();
    }

    [Fact]
    public void GivenVerticalDxfLineTwiceWhenMergedThenExpectOneLine()
    {
      var line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0));
      var list = new List<DxfLine>() { line, line };

      var sut = new DxfLineMerger();
      sut.MergeLines(list).Count().Should().Be(1);
    }

    [Fact]
    public void GivenVerticalDxfLineTwiceWhenMergedThenExpectSameLine()
    {
      var line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0));
      var list = new List<DxfLine>() { line, line };

      var sut = new DxfLineMerger();
      sut.MergeLines(list).Single().Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenHorizontalLinesWhenOverlappingWhenGetCombinedThenExpect()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(25, 0, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(30, 0, 0)));
    }

    [Fact]
    public void GivenVerticalLinesWhenOverlappingWhenGetCombinedThenExpect()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 25, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(0, 20, 0), new DxfPoint(0, 30, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 30, 0)));
    }


    [Fact]
    public void GivenVerticalLinesWhenReversedOverlappingWhenGetCombinedThenExpect()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 25, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(0, 30, 0), new DxfPoint(0, 20, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 30, 0)));
    }

    [Fact]
    public void GivenImpreciseOverlapWhenMergeLinesThenShouldMerge()
    {
      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(new DxfLine(new DxfPoint(-1.7000000021027972E-06, 49.9999963, 0), new DxfPoint(49.9999963, 49.9999963, 0)));
      dxfFile.Entities.Add(new DxfLine(new DxfPoint(49.999996599999996, 49.999996399999986, 0), new DxfPoint(-1.4000000021496817E-06, 49.99999639999999, 0)));
      
      var sut = new DxfLineMerger();
      var actual = sut.MergeLines(dxfFile);
      actual.Entities.Count.Should().Be(dxfFile.Entities.Count - 1);
    }

    [Fact]
    public void GivenImpreciseOverlapWhenMergeLinesThenShouldMergeDetail()
    {
      var a = new MergeLine(new DxfLine(new DxfPoint(-1.7000000021027972E-06, 49.9999963, 0), new DxfPoint(49.9999963, 49.9999963, 0)));
      var b = new MergeLine(new DxfLine(new DxfPoint(49.999996599999996, 49.999996399999986, 0), new DxfPoint(-1.4000000021496817E-06, 49.99999639999999, 0)));

      DxfLineMerger.Coincident(a, b).Should().BeTrue();
      DxfLineMerger.Coaligned(a, b).Should().BeTrue();
      var actual = DxfLineMerger.GetCombined(a, b).Line;
      actual.P1.X.Should().BeApproximately(0, 0.0001);
      actual.P1.Y.Should().BeApproximately(50, 0.0001);
      actual.P2.X.Should().BeApproximately(50, 0.0001);
      actual.P2.Y.Should().BeApproximately(50, 0.0001);

      var dxfFile = new DxfFile();
      var sut = new DxfLineMerger();
      dxfFile.Entities.Add(a.Line);
      dxfFile.Entities.Add(b.Line);
      var mergeFile = sut.MergeLines(dxfFile);
      mergeFile.Entities.Count().Should().Be(1);
    }
  }
}
