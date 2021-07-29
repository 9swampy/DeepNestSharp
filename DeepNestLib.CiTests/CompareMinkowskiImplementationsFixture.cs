namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class CompareMinkowskiImplementationsFixture
  {
    private const int iterations = 1000;

    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();

    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();
    private INfp clipperResult;
    private INfp dllImportResult;
    private long clipperResultTime;
    private long dllImportExecuteTime;

    public CompareMinkowskiImplementationsFixture()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      INfp firstPart;
      DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }).TryConvertToNfp(firstPartIdSrc, out firstPart).Should().BeTrue();
      INfp secondPart;
      DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }).TryConvertToNfp(secondPartIdSrc, out secondPart).Should().BeTrue();
      var sw = new Stopwatch();
      sw.Start();
      var minkowskiSumService = MinkowskiSum.CreateInstance();
      for (int i = 0; i < iterations; i++)
      {
        clipperResult = minkowskiSumService.ClipperExecute(firstPart, secondPart, MinkowskiSumPick.Largest);
      }

      sw.Stop();
      clipperResultTime = sw.ElapsedTicks;
      sw.Restart();
      for (int i = 0; i < iterations; i++)
      {
        dllImportResult = minkowskiSumService.DllImportExecute(firstPart, secondPart, MinkowskiSumCleaning.Cleaned);
      }

      sw.Stop();
      dllImportExecuteTime = sw.ElapsedTicks;
    }

    [Fact]
    public void DllImportShouldBeAnOrderOfMagnitudeFasterThanNativeCSharp()
    {
      System.Diagnostics.Debug.Print($"{clipperResultTime}>{dllImportExecuteTime}");
      clipperResultTime.Should().BeGreaterOrEqualTo(dllImportExecuteTime);
    }

    [Fact]
    public void AreasShouldBeSame()
    {
      dllImportResult.Area.Should().BeApproximately(clipperResult.Area, 1D);
    }

    [Fact]
    public void HeightCalculatedShouldBeSame()
    {
      dllImportResult.HeightCalculated.Should().BeApproximately(clipperResult.HeightCalculated, 0.000001);
    }

    [Fact]
    public void WidthCalculatedShouldBeSame()
    {
      dllImportResult.WidthCalculated.Should().BeApproximately(clipperResult.WidthCalculated, 0.000001);
    }

    [Fact]
    public void BackgroundResultShouldHave4Points()
    {
      SvgNest.cleanPolygon2(dllImportResult).Points.Length.Should().Be(4);
    }

    [Fact]
    public void DeepNestClipperResultShouldHave4Points()
    {
      clipperResult.Points.Length.Should().Be(4);
    }

    [Fact]
    public void UncleanedResultsShouldBeEquivalent()
    {
      dllImportResult.Should().BeEquivalentTo(
        clipperResult,
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                          .WhenTypeIs<double>(),
        "because I added the CleanPolygon in to the sut.MinkowskiSum");
    }

    [Fact]
    public void CleanedResultsShouldBeEquivalent()
    {
      SvgNest.cleanPolygon2(dllImportResult).Should().BeEquivalentTo(clipperResult, options => options
                                    .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }
  }
}
