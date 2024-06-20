namespace DeepNestLib.CiTests
{
  using DeepNestLib.CiTests.IO;
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
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Rotate(45);
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));
      NfpSimplifier.PointInPolygon(small[index], large).Should().Be(expected);
    }

    [Fact]
    public void GivenCommmonOriginSmallFitsInsideLargeThenExteriorShouldBeFalse()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, small).Should().BeFalse();
    }

    [Fact]
    public void GivenCommmonOriginLargeFitsInsideLargeThenExteriorShouldBeFalse()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, large).Should().BeFalse();
    }

    [Fact]
    public void GivenCommmonOriginLargeFitsInsideLargeThenInteriorShouldBeTrue()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Interior(large, large).Should().BeTrue();
    }

    [Fact]
    public void GivenOffsetOriginButSmallStillFitsInsideLargeThenExteriorShouldBeFalse()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Shift(10, 10);
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, small).Should().BeFalse();
    }

    [Fact]
    public void GivenOffsetOriginWhenSmallInsideNonCoincidentThenInteriorShouldBeTrue()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Shift(5, 5);
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Interior(large, small).Should().BeTrue();
    }

    [Fact]
    public void GivenOffsetOriginSoSmallWillNotFitsInsideLargeThenExteriorShouldBeTrue()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      small = small.Shift(15, 15);
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));

      NfpSimplifier.Exterior(large, small).Should().BeTrue();
    }

    [Fact]
    public void GivenLargeCannotFitInsideSmallThenShouldBeExterior()
    {
      DxfGenerator dxfGenerator = new DxfGenerator();
      INfp small = NoFitPolygon.FromDxf(dxfGenerator.Square(10, RectangleType.BottomLeftClockwise, true));
      INfp large = NoFitPolygon.FromDxf(dxfGenerator.Square(20, RectangleType.BottomLeftClockwise, true));
      NfpSimplifier.Exterior(small, large).Should().BeTrue();
    }
  }
}
