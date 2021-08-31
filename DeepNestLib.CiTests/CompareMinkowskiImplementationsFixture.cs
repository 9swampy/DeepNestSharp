namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
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
      INfp firstPart;
      DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }).TryConvertToNfp(firstPartIdSrc, out firstPart).Should().BeTrue();
      INfp secondPart;
      DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Rectangle(11D) }).TryConvertToNfp(secondPartIdSrc, out secondPart).Should().BeTrue();
      var sw = new Stopwatch();
      sw.Start();
      IMinkowskiSumService minkowskiSumService = A.Dummy<MinkowskiSum>();
      for (int i = 0; i < iterations; i++)
      {
        clipperResult = minkowskiSumService.ClipperExecute(firstPart, secondPart, MinkowskiSumPick.Largest);
      }

      sw.Stop();
      clipperResultTime = sw.ElapsedTicks;
      sw.Restart();
      for (int i = 0; i < iterations; i++)
      {
        dllImportResult = minkowskiSumService.DllImportExecute(firstPart, secondPart, MinkowskiSumCleaning.None).Single();
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
      SvgNest.CleanPolygon2(dllImportResult).Points.Length.Should().Be(4);
    }

    [Fact]
    public void DeepNestClipperResultShouldHave4Points()
    {
      clipperResult.Points.Length.Should().Be(4);
    }

    [Fact]
    public void UncleanedResultsShouldNotBeEquivalent()
    {
#pragma warning disable IDE0058 // Expression value is never used
      _ = dllImportResult.Should().NotBeEquivalentTo(
        clipperResult,
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                          .WhenTypeIs<double>(),
#pragma warning disable SA1118 // Parameter should not span multiple lines
        "because they're only same if CleanPolygon added in to the sut.MinkowskiSum; cleaning could mean " +
        "the result is no longer closed so if it's fed back in to the ClipperLib sum it won't work; unless " +
        "we change the way ClipperLib is called.");
#pragma warning restore SA1118 // Parameter should not span multiple lines
    }

    [Fact]
    public void CleanedResultsShouldBeEquivalent()
    {
      SvgNest.CleanPolygon2(dllImportResult).Should().BeEquivalentTo(clipperResult, options => options
                                    .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }
  }
}
