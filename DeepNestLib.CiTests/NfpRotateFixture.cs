namespace DeepNestLib.CiTests
{
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using System.Collections.Generic;
  using Xunit;

  public class NfpRotateFixture
  {
    [Fact]
    public void GivenPartWithAsymetricHolesRotatedClockwiseWhenRotatedBackThenShouldBeEquivalent()
    {
      var sut = DxfParser.LoadDxfFileStreamAsNfp("Dxfs._10.dxf");
      sut.Rotate(90).Rotate(-90).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GivenPartWithAsymetricHolesWhenRotatedClockwiseThenShouldHaveSameNumberOfPoints()
    {
      var sut = DxfParser.LoadDxfFileStreamAsNfp("Dxfs._10.dxf");
      sut.Rotate(90).Points.Length.Should().Be(sut.Points.Length);
    }

    [Fact]
    public void GivenPartWithAsymetricHolesWhenRotatedClockwiseThenShouldHaveSameNumberOfChildren()
    {
      var sut = DxfParser.LoadDxfFileStreamAsNfp("Dxfs._10.dxf");
      sut.Rotate(90).Children.Count.Should().Be(sut.Children.Count);
    }

    [Fact]
    public void GivenPartWithNoHolesRotatedClockwiseWhenRotatedBackThenShouldBeEquivalent()
    {
      var sut = DxfParser.LoadDxfFileStreamAsNfp("Dxfs._5.dxf");
      sut.Rotate(90).Rotate(-90).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GivenRectangleExpectPoints()
    {
      var sut = NoFitPolygon.FromDxf(new DxfGenerator().Rectangle(10, 20, RectangleType.Normal));
      sut.Points.Should().BeEquivalentTo(new List<SvgPoint>() {
        new SvgPoint(0, 0),
        new SvgPoint(0, 20),
        new SvgPoint(10, 0),
        new SvgPoint(0, 20),
        new SvgPoint(10, 20),
      });
    }

    [Fact]
    public void GivenRectangleWhenRotateThenOriginalUnchanged()
    {
      var sut = NoFitPolygon.FromDxf(new DxfGenerator().Rectangle(10, 20, RectangleType.BottomLeftClockwise));
      sut.Rotate(90);
      sut.Points.Should().BeEquivalentTo(new List<SvgPoint>() {
        new SvgPoint(0, 0),
        new SvgPoint(0, 20),
        new SvgPoint(10, 20),
        new SvgPoint(10, 0),
        new SvgPoint(0, 0),
      });
    }

    [Fact]
    public void GivenRectangleWhenRotatedClockwiseExpectPoints()
    {
      var sut = NoFitPolygon.FromDxf(new DxfGenerator().Rectangle(10, 20, RectangleType.BottomLeftClockwise));
      sut.Rotate(-90).Points.Should().BeEquivalentTo(new List<SvgPoint>() {
        new SvgPoint(0, 0),
        new SvgPoint(20, 0),
        new SvgPoint(20, -10),
        new SvgPoint(0, -10),
        new SvgPoint(0, 0),
      }, opt =>
        opt.Excluding(o => o.Name == "Source")
           .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
           .WhenTypeIs<double>());
    }

    [Fact]
    public void GivenSquareWithChildWhenRotatedClockwiseExpectChildPoints()
    {
      var sut = NoFitPolygon.FromDxf(new DxfGenerator().Square(20, RectangleType.BottomLeftClockwise));
      sut.Children.Add(NoFitPolygon.FromDxf(new DxfGenerator().Square(10, RectangleType.BottomLeftClockwise)).Shift(5, 5));
      sut.Rotate(-90).Children[0].Points.Should().BeEquivalentTo(new List<SvgPoint>() {
        new SvgPoint(5, -5),
        new SvgPoint(15, -5),
        new SvgPoint(15, -15),
        new SvgPoint(5, -15),
        new SvgPoint(5, -5),
      }, opt =>
        opt.Excluding(o => o.Name == "Source")
           .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
           .WhenTypeIs<double>());
    }

    [Fact]
    public void GivenRectangleWhenRotatedAntiClockwiseExpectPoints()
    {
      var sut = NoFitPolygon.FromDxf(new DxfGenerator().Rectangle(10, 20, RectangleType.BottomLeftClockwise));
      sut.Rotate(90).Points.Should().BeEquivalentTo(new List<SvgPoint>() {
        new SvgPoint(0, 0),
        new SvgPoint(-20, 0),
        new SvgPoint(-20, 10),
        new SvgPoint(0, 10),
        new SvgPoint(0, 0),
      }, opt =>
        opt.Excluding(o => o.Name == "Source")
           .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
           .WhenTypeIs<double>());
    }
  }
}
