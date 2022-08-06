namespace DeepNestLib.CiTests.PairMap
{
  using System;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.PairMap;
  using FluentAssertions;

  public class PmapWorkerFixtureSetup
  {
    internal static SvgPoint[] ExpectedPoints1 = new SvgPoint[]
      {
        new SvgPoint(0, 11),
        new SvgPoint(-22, 11),
        new SvgPoint(-22, -11),
        new SvgPoint(0, -11),
        new SvgPoint(0, 11),
      };

    internal static SvgPoint[] ExpectedPoints2 = new SvgPoint[]
    {
      new SvgPoint(11, 11),
      new SvgPoint(-11, 11),
      new SvgPoint(-11, -11),
      new SvgPoint(11, -11),
      new SvgPoint(11, 11),
    };

    internal static SvgPoint[] ExpectedInPoints = new SvgPoint[]
    {
      new SvgPoint(11, 11),
      new SvgPoint(0, 11),
      new SvgPoint(0, 0),
      new SvgPoint(11, 0),
      new SvgPoint(11, 11),
    };

    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private int firstPartIdSrc = new Random().Next();
    private int secondPartIdSrc = new Random().Next();
    private int thirdPartIdSrc = new Random().Next();
    private int fourthPartIdSrc = new Random().Next();
    protected INfp firstPart;
    protected INfp secondPart;
    protected NfpPair pair1;
    protected NfpPair pair2;

    public PmapWorkerFixtureSetup(double pair1ASize, double pair1BSize)
    {
      DxfGenerator.GenerateSquare("firstPart", pair1ASize, RectangleType.FileLoad).TryConvertToNfp(firstPartIdSrc, out firstPart).Should().BeTrue();
      firstPart = SvgNest.CleanPolygon2(firstPart);
      DxfGenerator.GenerateSquare("secondPart", pair1BSize, RectangleType.FileLoad).TryConvertToNfp(secondPartIdSrc, out secondPart).Should().BeTrue();
      secondPart = SvgNest.CleanPolygon2(secondPart);

      pair1 = new NfpPair();
      pair1.A = firstPart;
      pair1.B = secondPart;
      pair1.ARotation = 180;
      pair1.BRotation = 90;

      INfp thirdPart;
      DxfGenerator.GenerateSquare("thirdPart", 11D, RectangleType.FileLoad).TryConvertToNfp(thirdPartIdSrc, out thirdPart).Should().BeTrue();
      thirdPart = SvgNest.CleanPolygon2(thirdPart);
      INfp fourthPart;
      DxfGenerator.GenerateSquare("fourthPart", 11D, RectangleType.FileLoad).TryConvertToNfp(fourthPartIdSrc, out fourthPart).Should().BeTrue();
      fourthPart = SvgNest.CleanPolygon2(fourthPart);

      pair2 = new NfpPair();
      pair2.A = thirdPart;
      pair2.B = fourthPart;
      pair2.ARotation = 0;
      pair2.BRotation = 180;
    }
  }
}
