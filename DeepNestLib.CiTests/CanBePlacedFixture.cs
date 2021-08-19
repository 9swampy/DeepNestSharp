namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class CanBePlacedFixture
  {
    [Fact]
    public void GivenSmallerSquareWhenFitInLargeSquareThenCanBePlaced()
    {
      var generator = new DxfGenerator();
      var nfpHelper = new NfpHelper();
      ((ITestNfpHelper)nfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<INestStateMinkowski>());
      new SheetNfp(
        nfpHelper,
        generator.GenerateRectangle("Sheet", 1.01, 1.01, RectangleType.FileLoad).ToNfp(),
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        100000).CanAcceptPart.Should().BeTrue();
    }

    [Fact]
    public void GivenLargerSquareWhenFitInSmallSquareThenCanNotBePlaced()
    {
      var generator = new DxfGenerator();
      var nfpHelper = new NfpHelper();
      ((ITestNfpHelper)nfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<INestStateMinkowski>());
      new SheetNfp(
        nfpHelper,
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        generator.GenerateRectangle("Sheet", 1.01, 1.01, RectangleType.FileLoad).ToNfp(),
        100000).CanAcceptPart.Should().BeFalse();
    }

    [Fact]
    public void GivenIdenticalSquaresWhenFitThenCanNotBePlaced()
    {
      var generator = new DxfGenerator();
      var nfpHelper = new NfpHelper();
      ((ITestNfpHelper)nfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<INestStateMinkowski>());
      new SheetNfp(
        nfpHelper,
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        100000).CanAcceptPart.Should().BeFalse();
    }
  }
}
