namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class NfpIsClosedFixture
  {
    [Fact]
    public void GivenUnclosedRectangleThenShouldNotBeClosed()
    {
      NfpGenerator generator = new NfpGenerator();
      Random random = new Random();
      generator.GenerateRectangle(A.Dummy<string>(), random.NextDouble(), random.NextDouble(), RectangleType.TopLeftClockwise, false).IsClosed.Should().BeFalse();
    }

    [Fact]
    public void GivenUnclosedRectangleWhenPointsRotatedThenShouldNotBeClosed()
    {
      NfpGenerator generator = new NfpGenerator();
      Random random = new Random();
      Queue<SvgPoint> points = new Queue<SvgPoint>(generator.GenerateRectangle(A.Dummy<string>(), random.NextDouble(), random.NextDouble(), RectangleType.TopLeftClockwise, false).Points);
      for (int i = 0; i < random.Next(points.Count); i++)
      {
        points.Enqueue(points.Dequeue());
      }

      NoFitPolygon rotatedPointNfp = new NoFitPolygon(points);

      rotatedPointNfp.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void GivenClosedRectangleThenShouldBeClosed()
    {
      NfpGenerator generator = new NfpGenerator();
      Random random = new Random();
      generator.GenerateRectangle(A.Dummy<string>(), random.NextDouble(), random.NextDouble(), RectangleType.TopLeftClockwise, true).IsClosed.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void GivenClosedRectangleWhenPointsRotatedThenShouldBeClosed(int rotations)
    {
      NfpGenerator generator = new NfpGenerator();
      Random random = new Random();
      Queue<SvgPoint> points = new Queue<SvgPoint>(generator.GenerateRectangle(A.Dummy<string>(), random.NextDouble(), random.NextDouble(), RectangleType.TopLeftClockwise, true).Points);
      for (int i = 0; i < rotations; i++)
      {
        points.Enqueue(points.Dequeue());
      }

      NoFitPolygon rotatedPointNfp = new NoFitPolygon(points);

      rotatedPointNfp.IsClosed.Should().BeTrue();
    }
  }
}
