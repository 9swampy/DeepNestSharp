namespace DeepNestLib.CiTests.GeneticAlgorithm
{
  using System;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class GravityBoundsFixture
  {
    [Fact]
    public void Test()
    {
      var small = 10;
      var large = 20;
      var sheetPlacementVertical = A.Fake<ISheetPlacement>();
      var sutVertical = new OriginalFitnessSheet(sheetPlacementVertical);
      A.CallTo(() => sheetPlacementVertical.RectBounds).Returns(new PolygonBounds(0, 0, small, large));
      A.CallTo(() => sheetPlacementVertical.PlacementType).Returns(PlacementTypeEnum.Gravity);
      var sheetSquare = A.Fake<ISheet>();
      A.CallTo(() => sheetSquare.Area).Returns(Math.Pow(Math.Max(small, large), 2));
      A.CallTo(() => sheetSquare.Width).Returns(Math.Max(small, large));
      A.CallTo(() => sheetPlacementVertical.Sheet).Returns(sheetSquare);
      var hull = A.Fake<INfp>();
      A.CallTo(() => hull.Area).Returns(small * large);
      A.CallTo(() => sheetPlacementVertical.Hull).Returns(hull);

      var sheetPlacementHorizontal = A.Fake<ISheetPlacement>();
      var sutHorizontal = new OriginalFitnessSheet(sheetPlacementHorizontal);
      A.CallTo(() => sheetPlacementHorizontal.RectBounds).Returns(new PolygonBounds(0, 0, large, small));
      A.CallTo(() => sheetPlacementHorizontal.PlacementType).Returns(PlacementTypeEnum.Gravity);
      A.CallTo(() => sheetPlacementHorizontal.Sheet).Returns(sheetSquare);
      A.CallTo(() => sheetPlacementHorizontal.Hull).Returns(hull);

      //sutVertical.Bounds.Should().Be(2000);
      //sutHorizontal.Bounds.Should().Be(8000);
      System.Diagnostics.Debug.Print($"Vertical {sutVertical.Bounds} < Horizontal {sutHorizontal.Bounds}");
      sutVertical.Bounds.Should().BeLessThan(sutHorizontal.Bounds);
    }
  }
}
