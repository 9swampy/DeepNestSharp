namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
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
      var closedRectangle = new DxfGenerator().Square(10, RectangleType.Normal, true);

      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(4);
    }

    [Fact]
    public void GivenDxfRectangleTwiceWhenMergedThenExpectFourLines()
    {
      var closedRectangle = new DxfGenerator().Square(10, RectangleType.Normal, true);

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
      var closedRectangle10 = new DxfGenerator().Square(10, RectangleType.Normal, true);
      var closedRectangle20 = new DxfGenerator().Square(20, RectangleType.Normal, true);

      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(6);
    }

    [Fact]
    public void GivenIntersectingOppositeCornerDxfRectanglesWhenMergedThenExpectSixLines()
    {
      var closedRectangle10 = new DxfGenerator().Square(10, RectangleType.Normal, true);
      closedRectangle10 = new DxfPolyline(Translate(closedRectangle10, 10, 10));
      var closedRectangle20 = new DxfGenerator().Square(20, RectangleType.Normal, true);


      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(6);
    }

    [Fact]
    public void GivenIntersectingOneSideDxfRectanglesWhenMergedThenExpectSevenLines()
    {
      var closedRectangle10 = new DxfGenerator().Square(10, RectangleType.Normal, true);
      closedRectangle10 = new DxfPolyline(Translate(closedRectangle10, 5, 10));
      var closedRectangle20 = new DxfGenerator().Square(20, RectangleType.Normal, true);


      var dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      var sut = new DxfLineMerger();
      var result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(7);
    }


    private static IEnumerable<DxfVertex> Translate(DxfPolyline closedRectangle10, int xOffset, int yOffset)
    {
      foreach (var vertex in closedRectangle10.Vertices)
      {
        yield return new DxfVertex(new DxfPoint(vertex.Location.X + xOffset, vertex.Location.Y + yOffset, 0));
      }
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
    public void GivenVerticalLinesWhenOverlappingWhenGetCombinedThenExpectFullExtent()
    {
      var left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 25, 0)));
      var right = new MergeLine(new DxfLine(new DxfPoint(0, 20, 0), new DxfPoint(0, 30, 0)));
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

    [Fact]
    public void GivenRealNestWhenCompressExpectLessEntities()
    {
      string DxfTestFilename = "Dxfs.TopsNest.dxf";
      DxfFile dxffile;
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        dxffile = DxfFile.Load(inputStream);
      }
      IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
      var sut = new DxfLineMerger();
      var expected = sut.SplitLines(entities);
      expected.Count().Should().Be(2750);
      //var matchLines = expected.Where(o =>
      //{
      //  if (o is DxfLine line &&
      //      new MergeLine(line) is MergeLine mergeLine &&
      //      mergeLine.IsVertical &&
      //      //mergeLine.Intercept > 302M &&
      //      mergeLine.Intercept < 0.1M
      //      )
      //  {
      //    return true;// mergeLine.Intercept == 302.5M;
      //  }

      //  return false;
      //});
      //matchLines.Count().Should().Be(3);
      var actual = sut.MergeLines(expected);

      System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
      actual.Count().Should().BeLessThanOrEqualTo(expected.Count());
#if NCRUNCH
      actual.Count().Should().Be(2681);
      actual.Count().Should().BeLessThan(expected.Count());
      //dxffile.Entities.Clear();
      //foreach (var entity in actual)
      //{
      //  dxffile.Entities.Add(entity);
      //}

      //dxffile.Save(@$"C:\Git\DeepNestSharp\DeepNestLib.CiTests\Dxfs\TopsNest-{DateTime.Now.ToString("yyMMdd-hhmmssfff")}.dxf", true);
#endif
    }

    [Fact]
    public void GivenRealNestVertWhenCompressExpectLessEntities()
    {
      string DxfTestFilename = "Dxfs.TopsNestVert.dxf";
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        var sut = new DxfLineMerger();
        var expected = sut.SplitLines(entities);

        var lineA = new MergeLine(expected.First() as DxfLine);
        var lineB = new MergeLine(expected.Skip(1).First() as DxfLine);
        lineA.IsVertical.Should().BeTrue();
        lineA.Left.Y.Should().BeLessThan(lineA.Right.Y);
        lineB.IsVertical.Should().BeTrue();
        lineB.Left.Y.Should().BeLessThan(lineB.Right.Y);

        DxfLineMerger.Coaligned(lineA, lineB).Should().BeTrue();
        DxfLineMerger.Coaligned(lineB, lineA).Should().BeTrue();
        DxfLineMerger.Coincident(lineB, lineA).Should().BeTrue();
        DxfLineMerger.Coincident(lineA, lineB).Should().BeTrue();
        DxfLineMerger.GetCombined(lineA, lineB).Should().BeEquivalentTo(DxfLineMerger.GetCombined(lineB, lineA),
          options => options.Using<decimal>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.0001M))
                            .WhenTypeIs<decimal>()
                            .Excluding(o =>
                              o.Name == "Left" ||  //These 4 properties are structs 
                              o.Name == "Right" || //that differ only decimal member 
                              o.Name == "P1" ||    //rounding...
                              o.Name == "P2"       //lazy solution...
                              ));

        expected.Count().Should().Be(2);
        var actual = sut.MergeLines(expected);
        actual.Count().Should().Be(1);

        System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
        actual.Count().Should().BeLessThan(expected.Count());
      }
    }

    [Fact]
    public void GivenRealNestVert2WhenCompressExpectLessEntities()
    {
      string DxfTestFilename = "Dxfs.TopsNestVert2.dxf";
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        var sut = new DxfLineMerger();
        var expected = sut.SplitLines(entities);
        expected.Count().Should().Be(3);

        var lineA = new MergeLine(expected.First() as DxfLine);
        var lineB = new MergeLine(expected.Skip(1).First() as DxfLine);
        var lineC = new MergeLine(expected.Skip(2).First() as DxfLine);
        lineA.IsVertical.Should().BeTrue();
        lineA.Left.Y.Should().BeLessThan(lineA.Right.Y);
        lineB.IsVertical.Should().BeTrue();
        lineB.Left.Y.Should().BeLessThan(lineB.Right.Y);

        DxfLineMerger.Coaligned(lineA, lineB).Should().BeTrue();
        DxfLineMerger.Coaligned(lineB, lineA).Should().BeTrue();
        DxfLineMerger.Coaligned(lineA, lineC).Should().BeTrue();
        DxfLineMerger.Coincident(lineA, lineB).Should().BeFalse();
        DxfLineMerger.Coincident(lineB, lineA).Should().BeFalse();
        DxfLineMerger.Coincident(lineA, lineC).Should().BeTrue();
        Action act = () => DxfLineMerger.GetCombined(lineA, lineB);
        act.Should().Throw<InvalidOperationException>();

        DxfLineMerger.GetCombined(lineA, lineC).Line.P1.X.Should().BeApproximately(302.5, 0.0001);
        DxfLineMerger.GetCombined(lineA, lineC).Line.P1.Y.Should().Be(212.5);
        DxfLineMerger.GetCombined(lineA, lineC).Line.P2.Y.Should().Be(687.5);

        DxfLineMerger.GetCombined(lineA, lineC).Should().BeEquivalentTo(DxfLineMerger.GetCombined(lineC, lineA),
        options => options.Using<decimal>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.0001M))
                            .WhenTypeIs<decimal>()
                            .Excluding(o =>
                              o.Name == "Left" ||  //These 4 properties are structs 
                              o.Name == "Right" || //that differ only decimal member 
                              o.Name == "P1" ||    //rounding...
                              o.Name == "P2"       //lazy solution...
                              ));

        expected.Count().Should().Be(3);
        var actual = sut.MergeLines(expected);
        actual.Count().Should().Be(1);

        System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
        actual.Count().Should().BeLessThan(expected.Count());
      }
    }

    [Fact]
    public void GivenRealNestTops2vert_WhenCompressExpectLessEntities()
    {
      string DxfTestFilename = "Dxfs.Tops_2vert.dxf";
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        var sut = new DxfLineMerger();
        var expected = sut.SplitLines(entities);
        expected.Count().Should().Be(7);

        var lineA = new MergeLine(expected.First() as DxfLine);
        var lineB = new MergeLine(expected.Skip(1).First() as DxfLine);
        var lineC = new MergeLine(expected.Skip(2).First() as DxfLine);
        lineA.IsVertical.Should().BeTrue();
        lineA.Left.Y.Should().BeLessThan(lineA.Right.Y);
        lineB.IsVertical.Should().BeTrue();
        lineB.Left.Y.Should().BeLessThan(lineB.Right.Y);

        DxfLineMerger.Coaligned(lineA, lineB).Should().BeTrue();
        DxfLineMerger.Coaligned(lineB, lineA).Should().BeTrue();
        DxfLineMerger.Coaligned(lineA, lineC).Should().BeTrue();
        DxfLineMerger.Coincident(lineA, lineB).Should().BeTrue();
        DxfLineMerger.Coincident(lineB, lineA).Should().BeTrue();
        DxfLineMerger.Coincident(lineA, lineC).Should().BeFalse();
        
        expected.Count().Should().Be(7);
        var actual = sut.MergeLines(expected);
        actual.Count().Should().Be(1);
        var expectedMergeLines = expected.Select(o => new MergeLine(o as DxfLine)).ToList();
        var actualMergeLine = new MergeLine(actual.Single() as DxfLine);
        actualMergeLine.Intercept.Should().Be(827.5M);
        actualMergeLine.Left.Y.Should().Be(expectedMergeLines.Min(o => Math.Min(o.Left.Y, o.Right.Y)));
        actualMergeLine.Right.Y.Should().Be(expectedMergeLines.Max(o => Math.Max(o.Left.Y, o.Right.Y)));

        System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
        actual.Count().Should().BeLessThan(expected.Count());

        dxffile.Entities.Clear();
        foreach (var entity in actual)
        {
          dxffile.Entities.Add(entity);
        }

        //dxffile.Save(@$"C:\Git\DeepNestSharp\DeepNestLib.CiTests\Dxfs\Tops_2vert-{DateTime.Now.ToString("yyMMdd-hhmmssfff")}.dxf", true);
      }
    }

    [Fact]
    public void GivenRealNestTops2_WhenCompressExpectLessEntities()
    {
      string DxfTestFilename = "Dxfs.Tops_2.dxf";
      using (var inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        var sut = new DxfLineMerger();
        var expected = sut.SplitLines(entities);
        //var expectedLines = expected.Cast<DxfLine>().Select(o => new MergeLine(o));
        //expectedLines = expectedLines.Where(o => o.IsVertical && o.Intercept >= 524.9M && o.Intercept <= 525.1M).OrderBy(o => o.Intercept);
        var expectedLines = expected.Cast<DxfLine>();
        expectedLines = expectedLines.Where(o => o.P1.X >= 524.9 && o.P1.X <= 525.1 && o.P2.X >= 524.9 && o.P2.X <= 525.1);
        expectedLines.Count().Should().Be(7);
        var expectedMergeLines = expectedLines.Select(o => new MergeLine(o));
        expected.Count().Should().Be(3412);

        foreach (var mergeLine in expectedMergeLines)
        {
          mergeLine.IsVertical.Should().BeTrue();
          mergeLine.Intercept.Should().Be(525M);
        }

        expectedLines.Count().Should().Be(7);
        var actualLines = sut.MergeLines(expectedLines);
        actualLines.Count().Should().Be(1);
        var actualMergeLine = new MergeLine(actualLines.Single());
        actualMergeLine.Left.Y.Should().Be(expectedMergeLines.Min(o => Math.Min(o.Left.Y, o.Right.Y)));
        actualMergeLine.Right.Y.Should().Be(expectedMergeLines.Max(o => Math.Max(o.Left.Y, o.Right.Y)));

        System.Diagnostics.Debug.Print($"actual {actualLines.Count()} < expected {expected.Count()}");
        actualLines.Count().Should().BeLessThan(expected.Count());

        var actual = sut.MergeLines(expected);

        var expectBoundedLines = actual.Cast<DxfLine>().Select(o => new MergeLine(o));
        expectBoundedLines.Min(o => Math.Min(Math.Min(o.Left.Y, o.Right.Y), Math.Min(o.Left.Y, o.Right.Y))).Should().BeGreaterThan(-0.1);
        expectBoundedLines.Max(o => Math.Max(Math.Max(o.Left.Y, o.Right.Y), Math.Max(o.Left.Y, o.Right.Y))).Should().BeLessThan(495.1);

        dxffile.Entities.Clear();
        foreach (var entity in actual)
        {
          dxffile.Entities.Add(entity);
        }

        //dxffile.Save(@$"C:\Git\DeepNestSharp\DeepNestLib.CiTests\Dxfs\Tops_2-{DateTime.Now.ToString("yyMMdd-HHmmssfff")}.dxf", true);
      }
    }
  }
}
