namespace DeepNestLib.CiTests.PairMap
{
  using DeepNestLib.PairMap;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using System;
  using Xunit;

  public class PmapWorkerProcessFixture : PmapWorkerFixtureSetup
  {
    private NfpPair processed;
    private const double aSize = 11D;
    private const double bSize = 11D;

    public PmapWorkerProcessFixture()
      : base(aSize, bSize)
    {
      processed = new PmapWorker(null, A.Fake<IProgressDisplayer>(), false, A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).Process(pair1);
    }

    [Fact]
    public void ShouldGenerateAResult()
    {
      processed.Should().NotBeNull();
    }

    [Fact]
    public void ShouldGenerateFirstPartExpected()
    {
      firstPart.Points.Should().BeEquivalentTo(ExpectedInPoints);
    }

    [Fact]
    public void ShouldGenerateSecondPartExpected()
    {
      secondPart.Points.Should().BeEquivalentTo(ExpectedInPoints);
    }

    [Fact]
    public void ShouldDereferenceA()
    {
      processed.A.Should().BeNull();
    }

    [Fact]
    public void ShouldDereferenceB()
    {
      processed.B.Should().BeNull();
    }

    [Fact]
    public void ShouldSetNfp()
    {
      processed.Nfp.Should().NotBeNull();
    }

    [Fact]
    public void ShouldSetNfpHeight()
    {
      processed.Nfp.HeightCalculated.Should().Be(aSize + bSize);
    }

    [Fact]
    public void ShouldSetNfpWidth()
    {
      processed.Nfp.WidthCalculated.Should().Be(aSize + bSize);
    }

    [Fact]
    public void ShouldSetNfpArea()
    {
      processed.Nfp.Area.Should().Be(Math.Pow(aSize + bSize, 2));
    }

    [Fact]
    public void ShouldSetPointsRealRun()
    {
      processed.Nfp.Points.Should().BeEquivalentTo(ExpectedPoints1, options => options
        .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
        .WhenTypeIs<double>());
    }
  }
}
