namespace DeepNestLib.CiTests.Placement
{
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
      ((IRawDetail)DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(100D, 200D, RectangleType.FileLoad) })).TryConvertToSheet(firstSheetIdSrc, out sutDxfEntity).Should().BeTrue();
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
          new SvgPoint(100, 0),
          new SvgPoint(100, 200),
          new SvgPoint(0, 200),
          new SvgPoint(0, 200)
        };

      sutDxfEntity.Points.Should().BeEquivalentTo(expected);

      expected = new SvgPoint[] {
          new SvgPoint(0, 0),
          new SvgPoint(100, 0),
          new SvgPoint(100, 200),
          new SvgPoint(0, 200),
          //new SvgPoint(0, 0)
        };

      sut.Points.Should().BeEquivalentTo(expected);
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
