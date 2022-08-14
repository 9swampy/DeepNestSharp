namespace DeepNestLib.CiTests.Placement
{
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using System;
  using System.Collections.Generic;
  using Xunit;

  public class NewSheetFixture
  {
    private ISheet sut = Sheet.NewSheet(1, 100, 200);
    private ISheet sutDxfEntity;
    int firstSheetIdSrc = new Random().Next();

    public NewSheetFixture()
    {
      ((IRawDetail)DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(100D, 200D, RectangleType.BottomLeftClockwise, true) })).TryConvertToSheet(firstSheetIdSrc, out sutDxfEntity).Should().BeTrue();
    }

    [Fact]
    public void WidthExpected()
    {
      sut.WidthCalculated.Should().Be(100);
      sutDxfEntity.WidthCalculated.Should().Be(100);
    }

    [Fact]
    public void HeightExpected()
    {
      sut.HeightCalculated.Should().Be(200);
      sutDxfEntity.HeightCalculated.Should().Be(200);
    }

    [Fact]
    public void AreaExpected()
    {
      sut.Area.Should().Be(100 * 200);
      sutDxfEntity.Area.Should().Be(100 * 200);
    }

    [Fact]
    public void NoChildrenExpected()
    {
      sut.Children.Should().BeEmpty();
      sutDxfEntity.Children.Should().BeEmpty();
    }

    [Fact]
    public void PointsExpected()
    {
      var expected = new SvgPoint[] {
          new SvgPoint(0, 0),
          new SvgPoint(0, 200),
          new SvgPoint(100, 200),
          new SvgPoint(100, 0),
          new SvgPoint(0, 0)
        };

      sutDxfEntity.Points.Should().BeEquivalentTo(expected, opt => opt.WithStrictOrdering());
      sut.Points.Should().BeEquivalentTo(expected, opt => opt.WithStrictOrdering());
    }

    [Fact]
    public void IsClosedExpected()
    {
      sut.IsClosed.Should().BeTrue();
      sutDxfEntity.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void IsExactExpected()
    {
      sut.IsExact.Should().BeTrue();
      sutDxfEntity.IsExact.Should().BeTrue();
    }
  }
}
