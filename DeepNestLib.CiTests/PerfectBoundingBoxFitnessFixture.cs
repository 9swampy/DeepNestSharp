namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib.CiTests.GeneticAlgorithm;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PerfectBoundingBoxFitnessFixture
  {
    double width;
    double height;
    double area;
    ISheet sheet;
    ISheetPlacement sp;

    public PerfectBoundingBoxFitnessFixture()
    {
      width = new Random().Next(50, 1200);
      height = new Random().Next(50, 900);
      area = width * height;
      sheet = A.Fake<ISheet>();
      A.CallTo(() => sheet.Area).Returns(area);
      sp = A.Fake<ISheetPlacement>();
      A.CallTo(() => sp.Sheet).Returns(sheet);
      A.CallTo(() => sp.MaterialUtilization).Returns(1);
      A.CallTo(() => sp.RectBounds).Returns(new PolygonBounds(0, 0, width, height));
      A.CallTo(() => sp.Hull).Returns(sheet);
      A.CallTo(() => sp.TotalPartsArea).Returns(area);
    }

    [Fact]
    public void GivenPerfectFitThenFitnessShouldBeApproximatelyExpected()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Total.Should().BeLessThan(area * 3);
    }

    [Fact]
    public void GivenPerfectFitThenMaterialUtilizationShouldBeZero()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Utilization.Should().Be(0);
    }

    [Fact]
    public void GivenPerfectFitWhenNoPrimaryThenSheetsShouldBeArea()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Sheets.Should().Be(area);
    }

    [Fact]
    public void GivenPerfectFitThenMaterialWastedShouldBeZero()
    {
      var sut = new OriginalFitnessSheet(sp);
      sut.Wasted.Should().BeApproximately(0, 1);
    }

    [Fact]
    public void GivenBoundsPenaltyShouldBeInLineWithSheetsPenaltyThenBoundsShouldBeLowerRangeOfSheetsPostScaling()
    {
      var sut = new OriginalFitnessSheet(sp);
      FitnessAlignment.BoundsPenaltyShouldBeInLineWithSheetsPenalty(sut, FitnessRange.Lower);
    }
  }
}
