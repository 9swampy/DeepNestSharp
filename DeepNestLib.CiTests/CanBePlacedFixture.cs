namespace DeepNestLib.CiTests
{
  using System;
  using System.Diagnostics;
  using System.Threading;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class CanBePlacedFixture
  {
    [Fact]
    public void GivenSmallerSquareWhenFitInLargeSquareThenCanBePlaced()
    {
      var sut = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>());
      var generator = new DxfGenerator();
      sut.CanBePlaced(
        generator.GenerateRectangle("Sheet", 1.01, 1.01, RectangleType.FileLoad).ToNfp(),
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        100000,
        out _).Should().BeTrue();
    }

    [Fact]
    public void GivenLargerSquareWhenFitInSmallSquareThenCanNotBePlaced()
    {
      var sut = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>());
      var generator = new DxfGenerator();
      sut.CanBePlaced(
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        generator.GenerateRectangle("Sheet", 1.01, 1.01, RectangleType.FileLoad).ToNfp(),
        100000,
        out _).Should().BeFalse();
    }

    [Fact]
    public void GivenIdenticalSquaresWhenFitThenCanNotBePlaced()
    {
      var sut = new Background(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>());
      var generator = new DxfGenerator();
      sut.CanBePlaced(
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        generator.GenerateRectangle("Sheet", 1, 1, RectangleType.FileLoad).ToNfp(),
        100000,
        out _).Should().BeFalse();
    }
  }
}
