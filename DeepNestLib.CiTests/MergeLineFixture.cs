namespace DeepNestLib.CiTests
{
  using FluentAssertions;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class MergeLineFixture
  {
    [Fact]
    public void GivenVerticalWhenMergedLineCreatedThenExpectOrderedY()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0))).Right.Y.Should().Be(10);
    }

    [Fact]
    public void GivenVerticalWhenMergedLineCreatedThenExpectSlopeInfinity()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0))).Slope.Should().Be(double.PositiveInfinity);
    }

    [Fact]
    public void GivenHorizontalWhenMergeLineCreatedThenExpectLeft()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)))
        .Left
        .Should()
        .BeEquivalentTo(new DxfPoint(0,0,0));
    }

    [Fact]
    public void GivenHorizontalWhenMergeLineCreatedThenExpectRight()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)))
        .Right
        .Should()
        .BeEquivalentTo(new DxfPoint(10, 0, 0));
    }

    [Fact]
    public void GivenHorizontalWhenMergedLineCreatedThenExpectSlope0()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0))).Slope.Should().Be(0);
    }

    [Fact]
    public void GivenReversedVerticalWhenMergedLineCreatedThenExpectOrderedY()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 10, 0), new DxfPoint(0, 0, 0))).Right.Y.Should().Be(10);
    }

    [Fact]
    public void GivenReversedVerticalWhenMergedLinesCreatedThenExpectSameLeft()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0))).Left
        .Should()
        .BeEquivalentTo(new MergeLine(new DxfLine(new DxfPoint(0, 10, 0), new DxfPoint(0, 0, 0))).Left);
    }

    [Fact]
    public void GivenReversedVerticalWhenMergedLinesCreatedThenExpectSameRight()
    {
      new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0))).Right
        .Should()
        .BeEquivalentTo(new MergeLine(new DxfLine(new DxfPoint(0, 10, 0), new DxfPoint(0, 0, 0))).Right);
    }
  }
}
