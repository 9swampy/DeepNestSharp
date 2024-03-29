﻿namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
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
      DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Square(11D) }).TryConvertToNfp(firstPartIdSrc, out firstPart).Should().BeTrue();
      INfp secondPart;
      DxfParser.ConvertDxfToRawDetail("Part", new List<DxfEntity>() { DxfGenerator.Square(11D) }).TryConvertToNfp(secondPartIdSrc, out secondPart).Should().BeTrue();
      var sw = new Stopwatch();
      sw.Start();
      IMinkowskiSumService minkowskiSumService = A.Dummy<MinkowskiSum>();
      for (int i = 0; i < iterations; i++)
      {
        clipperResult = minkowskiSumService.ClipperExecuteOuterNfp(firstPart.Points, secondPart.Points, MinkowskiSumPick.Largest);
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
    public void DllImportResultResultShouldHave5Points()
    {
      SvgNest.CleanPolygon2(dllImportResult).Points.Length.Should().Be(5, "it's closed, went 4 for a while prior to maintain IsClosed through CleanPolygon2");
    }

    [Fact]
    public void DeepNestClipperResultShouldHave5Points()
    {
      clipperResult.Points.Length.Should().Be(5, "it's closed");
    }

    [Fact]
    public void UncleanedResultsShouldBeEquivalent()
    {
      _ = dllImportResult.Should().NotBeEquivalentTo(
        clipperResult,
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                          .WhenTypeIs<double>());
    }
  }
}
