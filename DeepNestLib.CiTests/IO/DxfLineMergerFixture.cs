namespace DeepNestLib.CiTests.IO
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  using DeepNestLib.IO;
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
      DxfLine entity = new DxfGenerator().Line(10);

      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(entity);

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(1);
    }

    [Fact]
    public async Task GivenDxfRectangleWhenMergedThenExpectFourLines()
    {
      DxfPolyline closedRectangle = new DxfGenerator().Square(10, RectangleType.Normal, true);

      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle);

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(4);
    }

    [Fact]
    public void GivenDxfRectangleTwiceWhenMergedThenExpectFourLines()
    {
      DxfPolyline closedRectangle = new DxfGenerator().Square(10, RectangleType.Normal, true);

      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle);
      dxfFile.Entities.Add(closedRectangle);

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(4);
    }

    [Fact]
    public void GivenIntersectingDxfRectanglesWhenMergedThenExpectSixLines()
    {
      DxfPolyline closedRectangle10 = new DxfGenerator().Square(10, RectangleType.Normal, true);
      DxfPolyline closedRectangle20 = new DxfGenerator().Square(20, RectangleType.Normal, true);

      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(6);
    }

    [Fact]
    public void GivenIntersectingOppositeCornerDxfRectanglesWhenMergedThenExpectSixLines()
    {
      DxfPolyline closedRectangle10 = new DxfGenerator().Square(10, RectangleType.Normal, true);
      closedRectangle10 = new DxfPolyline(Translate(closedRectangle10, 10, 10));
      DxfPolyline closedRectangle20 = new DxfGenerator().Square(20, RectangleType.Normal, true);


      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(6);
    }

    [Fact]
    public void GivenIntersectingOneSideDxfRectanglesWhenMergedThenExpectSevenLines()
    {
      DxfPolyline closedRectangle10 = new DxfGenerator().Square(10, RectangleType.Normal, true);
      closedRectangle10 = new DxfPolyline(Translate(closedRectangle10, 5, 10));
      DxfPolyline closedRectangle20 = new DxfGenerator().Square(20, RectangleType.Normal, true);


      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(closedRectangle10);
      dxfFile.Entities.Add(closedRectangle20);

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile result = sut.MergeLines(dxfFile);
      result.Entities.Count().Should().Be(7);
    }


    private static IEnumerable<DxfVertex> Translate(DxfPolyline closedRectangle10, int xOffset, int yOffset)
    {
      foreach (DxfVertex vertex in closedRectangle10.Vertices)
      {
        yield return new DxfVertex(new DxfPoint(vertex.Location.X + xOffset, vertex.Location.Y + yOffset, 0));
      }
    }

    [Fact]
    public void GivenLinesWhenSameThenExpectCoaligned()
    {
      MergeLine line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      DxfLineMerger.Coaligned(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenCoalignedThenExpectCoaligned()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coaligned(left, right).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenNotCoincidentThenExpectFalse()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coincident(left, right).Should().BeFalse();
    }

    [Fact]
    public void GivenLinesWhenCoincidentThenExpectTrue()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(20, 0, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coincident(left, right).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenOverlappingThenExpectCoincident()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(25, 0, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.Coincident(left, right).Should().BeTrue();
    }

    [Fact]
    public void GivenLinesWhenSameThenExpectCoincident()
    {
      MergeLine line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0)));
      DxfLineMerger.Coincident(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenSingleLineWhenMergedThenExpectOneLine()
    {
      DxfLine line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      List<DxfLine> list = new List<DxfLine>() { line };

      DxfLineMerger sut = new DxfLineMerger();
      sut.MergeLines(list).Count().Should().Be(1);
    }

    [Fact]
    public void GivenSingleLineWhenMergedThenExpectSameLine()
    {
      DxfLine line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      List<DxfLine> list = new List<DxfLine>() { line };

      DxfLineMerger sut = new DxfLineMerger();
      sut.MergeLines(list).Single().Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenHorizontalDxfLineTwiceWhenMergedThenExpectOneLine()
    {
      DxfLine line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      List<DxfLine> list = new List<DxfLine>() { line, line };

      DxfLineMerger sut = new DxfLineMerger();
      sut.MergeLines(list).Count().Should().Be(1);
    }

    [Fact]
    public void GivenHorizontalDxfLineTwiceWhenMergedThenExpectSameLine()
    {
      DxfLine line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(10, 0, 0));
      List<DxfLine> list = new List<DxfLine>() { line, line };

      DxfLineMerger sut = new DxfLineMerger();
      sut.MergeLines(list).Single().Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenVerticalWhenCoalignedThenExpectCoaligned()
    {
      MergeLine line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));

      DxfLineMerger.Coaligned(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenVerticalWhenCoincidentThenExpectCoincident()
    {
      MergeLine line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));

      DxfLineMerger.Coincident(line, line).Should().BeTrue();
    }

    [Fact]
    public void GivenVerticalOverlappingThenExpectSame()
    {
      MergeLine line = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));

      DxfLineMerger.GetCombined(line, line).Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenVerticalCoalignedWhenNotOverlappingThenExpectFalse()
    {
      MergeLine a = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));
      MergeLine b = new MergeLine(new DxfLine(new DxfPoint(0, 20, 0), new DxfPoint(0, 30, 0)));

      DxfLineMerger.Coincident(a, b).Should().BeFalse();
    }

    [Fact]
    public void GivenVerticalCoalignedWhenReverseNotOverlappingThenExpectFalse()
    {
      MergeLine a = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0)));
      MergeLine b = new MergeLine(new DxfLine(new DxfPoint(0, 30, 0), new DxfPoint(0, 20, 0)));

      DxfLineMerger.Coincident(a, b).Should().BeFalse();
    }

    [Fact]
    public void GivenVerticalDxfLineTwiceWhenMergedThenExpectOneLine()
    {
      DxfLine line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0));
      List<DxfLine> list = new List<DxfLine>() { line, line };

      DxfLineMerger sut = new DxfLineMerger();
      sut.MergeLines(list).Count().Should().Be(1);
    }

    [Fact]
    public void GivenVerticalDxfLineTwiceWhenMergedThenExpectSameLine()
    {
      DxfLine line = new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 10, 0));
      List<DxfLine> list = new List<DxfLine>() { line, line };

      DxfLineMerger sut = new DxfLineMerger();
      sut.MergeLines(list).Single().Should().BeEquivalentTo(line);
    }

    [Fact]
    public void GivenHorizontalLinesWhenOverlappingWhenGetCombinedThenExpect()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(25, 0, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(20, 0, 0), new DxfPoint(30, 0, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(30, 0, 0)));
    }

    [Fact]
    public void GivenVerticalLinesWhenOverlappingWhenGetCombinedThenExpect()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 25, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(0, 20, 0), new DxfPoint(0, 30, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 30, 0)));
    }


    [Fact]
    public void GivenVerticalLinesWhenReversedOverlappingWhenGetCombinedThenExpect()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 25, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(0, 30, 0), new DxfPoint(0, 20, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 30, 0)));
    }

    [Fact]
    public void GivenVerticalLinesWhenOverlappingWhenGetCombinedThenExpectFullExtent()
    {
      MergeLine left = new MergeLine(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 25, 0)));
      MergeLine right = new MergeLine(new DxfLine(new DxfPoint(0, 20, 0), new DxfPoint(0, 30, 0)));
      DxfLineMerger.GetCombined(left, right).Line
        .Should()
        .BeEquivalentTo(new DxfLine(new DxfPoint(0, 0, 0), new DxfPoint(0, 30, 0)));
    }

    [Fact]
    public void GivenImpreciseOverlapWhenMergeLinesThenShouldMerge()
    {
      DxfFile dxfFile = new DxfFile();
      dxfFile.Entities.Add(new DxfLine(new DxfPoint(-1.7000000021027972E-06, 49.9999963, 0), new DxfPoint(49.9999963, 49.9999963, 0)));
      dxfFile.Entities.Add(new DxfLine(new DxfPoint(49.999996599999996, 49.999996399999986, 0), new DxfPoint(-1.4000000021496817E-06, 49.99999639999999, 0)));

      DxfLineMerger sut = new DxfLineMerger();
      DxfFile actual = sut.MergeLines(dxfFile);
      actual.Entities.Count.Should().Be(dxfFile.Entities.Count - 1);
    }

    [Fact]
    public void GivenImpreciseOverlapWhenMergeLinesThenShouldMergeDetail()
    {
      MergeLine a = new MergeLine(new DxfLine(new DxfPoint(-1.7000000021027972E-06, 49.9999963, 0), new DxfPoint(49.9999963, 49.9999963, 0)));
      MergeLine b = new MergeLine(new DxfLine(new DxfPoint(49.999996599999996, 49.999996399999986, 0), new DxfPoint(-1.4000000021496817E-06, 49.99999639999999, 0)));

      DxfLineMerger.Coincident(a, b).Should().BeTrue();
      DxfLineMerger.Coaligned(a, b).Should().BeTrue();
      DxfLine actual = DxfLineMerger.GetCombined(a, b).Line;
      actual.P1.X.Should().BeApproximately(0, 0.0001);
      actual.P1.Y.Should().BeApproximately(50, 0.0001);
      actual.P2.X.Should().BeApproximately(50, 0.0001);
      actual.P2.Y.Should().BeApproximately(50, 0.0001);

      DxfFile dxfFile = new DxfFile();
      DxfLineMerger sut = new DxfLineMerger();
      dxfFile.Entities.Add(a.Line);
      dxfFile.Entities.Add(b.Line);
      DxfFile mergeFile = sut.MergeLines(dxfFile);
      mergeFile.Entities.Count().Should().Be(1);
    }

    [Fact]
    public void GivenRealNestWhenCompressExpectLessEntities()
    {
      var DxfTestFilename = "Dxfs.TopsNest.dxf";
      DxfFile dxffile;
      using (System.IO.Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        dxffile = DxfFile.Load(inputStream);
      }
      IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
      DxfLineMerger sut = new DxfLineMerger();
      IEnumerable<DxfEntity> expected = sut.SplitLines(entities);
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
      IEnumerable<DxfEntity> actual = sut.MergeLines(expected);

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
      var DxfTestFilename = "Dxfs.TopsNestVert.dxf";
      using (System.IO.Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        DxfLineMerger sut = new DxfLineMerger();
        IEnumerable<DxfEntity> expected = sut.SplitLines(entities);

        MergeLine lineA = new MergeLine(expected.First() as DxfLine);
        MergeLine lineB = new MergeLine(expected.Skip(1).First() as DxfLine);
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
        IEnumerable<DxfEntity> actual = sut.MergeLines(expected);
        actual.Count().Should().Be(1);

        System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
        actual.Count().Should().BeLessThan(expected.Count());
      }
    }

    [Fact]
    public void GivenRealNestVert2WhenCompressExpectLessEntities()
    {
      var DxfTestFilename = "Dxfs.TopsNestVert2.dxf";
      using (System.IO.Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        DxfLineMerger sut = new DxfLineMerger();
        IEnumerable<DxfEntity> expected = sut.SplitLines(entities);
        expected.Count().Should().Be(3);

        MergeLine lineA = new MergeLine(expected.First() as DxfLine);
        MergeLine lineB = new MergeLine(expected.Skip(1).First() as DxfLine);
        MergeLine lineC = new MergeLine(expected.Skip(2).First() as DxfLine);
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
        IEnumerable<DxfEntity> actual = sut.MergeLines(expected);
        actual.Count().Should().Be(1);

        System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
        actual.Count().Should().BeLessThan(expected.Count());
      }
    }

    [Fact]
    public void GivenRealNestTops2vert_WhenCompressExpectLessEntities()
    {
      var DxfTestFilename = "Dxfs.Tops_2vert.dxf";
      using (System.IO.Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        DxfLineMerger sut = new DxfLineMerger();
        IEnumerable<DxfEntity> expected = sut.SplitLines(entities);
        expected.Count().Should().Be(7);

        MergeLine lineA = new MergeLine(expected.First() as DxfLine);
        MergeLine lineB = new MergeLine(expected.Skip(1).First() as DxfLine);
        MergeLine lineC = new MergeLine(expected.Skip(2).First() as DxfLine);
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
        IEnumerable<DxfEntity> actual = sut.MergeLines(expected);
        actual.Count().Should().Be(1);
        List<MergeLine> expectedMergeLines = expected.Select(o => new MergeLine(o as DxfLine)).ToList();
        MergeLine actualMergeLine = new MergeLine(actual.Single() as DxfLine);
        actualMergeLine.Intercept.Should().Be(827.5M);
        actualMergeLine.Left.Y.Should().Be(expectedMergeLines.Min(o => Math.Min(o.Left.Y, o.Right.Y)));
        actualMergeLine.Right.Y.Should().Be(expectedMergeLines.Max(o => Math.Max(o.Left.Y, o.Right.Y)));

        System.Diagnostics.Debug.Print($"actual {actual.Count()} < expected {expected.Count()}");
        actual.Count().Should().BeLessThan(expected.Count());

        dxffile.Entities.Clear();
        foreach (DxfEntity entity in actual)
        {
          dxffile.Entities.Add(entity);
        }

        //dxffile.Save(@$"C:\Git\DeepNestSharp\DeepNestLib.CiTests\Dxfs\Tops_2vert-{DateTime.Now.ToString("yyMMdd-hhmmssfff")}.dxf", true);
      }
    }

    [Fact]
    public void GivenRealNestTops2_WhenCompressExpectLessEntities()
    {
      var DxfTestFilename = "Dxfs.Tops_2.dxf";
      using (System.IO.Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(DxfTestFilename))
      {
        DxfFile dxffile = DxfFile.Load(inputStream);
        IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
        DxfLineMerger sut = new DxfLineMerger();
        IEnumerable<DxfEntity> expected = sut.SplitLines(entities);
        //var expectedLines = expected.Cast<DxfLine>().Select(o => new MergeLine(o));
        //expectedLines = expectedLines.Where(o => o.IsVertical && o.Intercept >= 524.9M && o.Intercept <= 525.1M).OrderBy(o => o.Intercept);
        IEnumerable<DxfLine> expectedLines = expected.Cast<DxfLine>();
        expectedLines = expectedLines.Where(o => o.P1.X >= 524.9 && o.P1.X <= 525.1 && o.P2.X >= 524.9 && o.P2.X <= 525.1);
        expectedLines.Count().Should().Be(7);
        IEnumerable<MergeLine> expectedMergeLines = expectedLines.Select(o => new MergeLine(o));
        expected.Count().Should().Be(3412);

        foreach (MergeLine mergeLine in expectedMergeLines)
        {
          mergeLine.IsVertical.Should().BeTrue();
          mergeLine.Intercept.Should().Be(525M);
        }

        expectedLines.Count().Should().Be(7);
        IEnumerable<DxfLine> actualLines = sut.MergeLines(expectedLines);
        actualLines.Count().Should().Be(1);
        MergeLine actualMergeLine = new MergeLine(actualLines.Single());
        actualMergeLine.Left.Y.Should().Be(expectedMergeLines.Min(o => Math.Min(o.Left.Y, o.Right.Y)));
        actualMergeLine.Right.Y.Should().Be(expectedMergeLines.Max(o => Math.Max(o.Left.Y, o.Right.Y)));

        System.Diagnostics.Debug.Print($"actual {actualLines.Count()} < expected {expected.Count()}");
        actualLines.Count().Should().BeLessThan(expected.Count());

        IEnumerable<DxfEntity> actual = sut.MergeLines(expected);

        IEnumerable<MergeLine> expectBoundedLines = actual.Cast<DxfLine>().Select(o => new MergeLine(o));
        expectBoundedLines.Min(o => Math.Min(Math.Min(o.Left.Y, o.Right.Y), Math.Min(o.Left.Y, o.Right.Y))).Should().BeGreaterThan(-0.1);
        expectBoundedLines.Max(o => Math.Max(Math.Max(o.Left.Y, o.Right.Y), Math.Max(o.Left.Y, o.Right.Y))).Should().BeLessThan(495.1);

        dxffile.Entities.Clear();
        foreach (DxfEntity entity in actual)
        {
          dxffile.Entities.Add(entity);
        }

        //dxffile.Save(@$"C:\Git\DeepNestSharp\DeepNestLib.CiTests\Dxfs\Tops_2-{DateTime.Now.ToString("yyMMdd-HHmmssfff")}.dxf", true);
      }
    }
  }
}
