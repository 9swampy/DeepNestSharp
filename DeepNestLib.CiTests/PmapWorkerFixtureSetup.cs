﻿namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class PmapWorkerFixtureSetup
  {
    internal static SvgPoint[] expectedPoints1 = new SvgPoint[]
    {
      new SvgPoint(0, 11),
      new SvgPoint(-22, 11),
      new SvgPoint(-22, -11),
      new SvgPoint(0, -11),
    };

    internal static SvgPoint[] expectedPoints2 = new SvgPoint[]
    {
      new SvgPoint(11, 11),
      new SvgPoint(-11, 11),
      new SvgPoint(-11, -11),
      new SvgPoint(11, -11),
    };

    internal static SvgPoint[] expectedInPoints = new SvgPoint[]
    {
      new SvgPoint(11, 11),
      new SvgPoint(0, 11),
      new SvgPoint(0, 0),
      new SvgPoint(11, 0),
    };

    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();
    private int thirdPartIdSrc = new Random().Next();
    private int fourthPartIdSrc = new Random().Next();
    protected NFP firstPart;
    protected NFP secondPart;
    protected NfpPair pair1;
    protected NfpPair pair2;

    private NfpPair[] processed;

    public PmapWorkerFixtureSetup()
    {
      var nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("firstPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FileLoad) }), firstPartIdSrc, out firstPart).Should().BeTrue();
      firstPart = SvgNest.cleanPolygon2(firstPart);
      firstPart.Rotation = 180;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("secondPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FileLoad) }), secondPartIdSrc, out secondPart).Should().BeTrue();
      secondPart = SvgNest.cleanPolygon2(secondPart);
      secondPart.Rotation = 90;

      pair1 = new NfpPair();
      pair1.A = firstPart;
      pair1.B = secondPart;
      pair1.ARotation = firstPart.Rotation;
      pair1.BRotation = secondPart.Rotation;

      NFP thirdPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("thirdPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FileLoad) }), thirdPartIdSrc, out thirdPart).Should().BeTrue();
      thirdPart = SvgNest.cleanPolygon2(thirdPart);
      thirdPart.Rotation = 0;
      NFP fourthPart;
      nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("fourthPart", new List<DxfEntity>() { DxfGenerator.Rectangle(11D, RectangleType.FileLoad) }), fourthPartIdSrc, out fourthPart).Should().BeTrue();
      fourthPart = SvgNest.cleanPolygon2(fourthPart);
      fourthPart.Rotation = 180;

      pair2 = new NfpPair();
      pair2.A = thirdPart;
      pair2.B = fourthPart;
      pair2.ARotation = thirdPart.Rotation;
      pair2.BRotation = fourthPart.Rotation;
    }
  }
}
