namespace DeepNestLib.CiTests
{
  using System.Diagnostics;
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class NfpSimplifierFixture
  {
    [Theory]
    [InlineData(0, Found.OnPolygon)]
    [InlineData(1, Found.Outside)]
    [InlineData(2, Found.OnPolygon)]
    [InlineData(3, Found.Inside)]
    [InlineData(4, Found.OnPolygon)]
    internal void GivenSmallWhenPointInPolygonExpectFound(int index, Found expected)
    {
      var dxfGenerator = new DxfGenerator();
      var small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Rotate(45);
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));
      NfpSimplifier.PointInPolygon(small[index], large).Should().Be(expected);
    }

    [Fact]
    public void GivenCommmonOriginSmallFitsInsideLargeThenExteriorShouldBeFalse()
    {
      var dxfGenerator = new DxfGenerator();
      var small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, small).Should().BeFalse();
    }

    [Fact]
    public void GivenCommmonOriginLargeFitsInsideLargeThenExteriorShouldBeFalse()
    {
      var dxfGenerator = new DxfGenerator();
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, large).Should().BeFalse();
    }

    [Fact]
    public void GivenCommmonOriginLargeFitsInsideLargeThenInteriorShouldBeTrue()
    {
      var dxfGenerator = new DxfGenerator();
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Interior(large, large).Should().BeTrue();
    }

    [Fact]
    public void GivenOffsetOriginButSmallStillFitsInsideLargeThenExteriorShouldBeFalse()
    {
      var dxfGenerator = new DxfGenerator();
      var small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Shift(10, 10);
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, small).Should().BeFalse();
    }

    [Fact]
    public void GivenOffsetOriginWhenSmallInsideNonCoincidentThenInteriorShouldBeTrue()
    {
      var dxfGenerator = new DxfGenerator();
      var small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Shift(5, 5);
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Interior(large, small).Should().BeTrue();
    }

    [Fact]
    public void GivenOffsetOriginSoSmallWillNotFitsInsideLargeThenExteriorShouldBeTrue()
    {
      var dxfGenerator = new DxfGenerator();
      var small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Shift(15, 15);
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, small).Should().BeTrue();
    }

    [Fact]
    public void GivenLargeCannotFitInsideSmallThenShouldBeExterior()
    {
      var dxfGenerator = new DxfGenerator();
      var small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      var large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));
      NfpSimplifier.Exterior(small, large).Should().BeTrue();
    }
  }
}
