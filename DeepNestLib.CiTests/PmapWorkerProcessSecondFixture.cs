namespace DeepNestLib.CiTests
{
  using DeepNestLib.PairMap;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PmapWorkerProcessSecondFixture : PmapWorkerFixtureSetup
  {
    private NfpPair processed;

    public PmapWorkerProcessSecondFixture()
      : base(11D, 11D)
    {
      processed = new PmapWorker(null, A.Fake<IProgressDisplayer>(), false, A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).Process(pair2);
    }

    [Fact]
    public void ShouldGenerateAResult()
    {
      processed.Should().NotBeNull();
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
      processed.Nfp.HeightCalculated.Should().Be(22);
    }

    [Fact]
    public void ShouldSetNfpWidth()
    {
      processed.Nfp.WidthCalculated.Should().Be(22);
    }

    [Fact]
    public void ShouldSetNfpArea()
    {
      processed.Nfp.Area.Should().Be(484);
    }

    [Fact]
    public void ShouldSetPoints()
    {
      processed.Nfp.Points.Should().BeEquivalentTo(ExpectedPoints2, options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }
  }
}
