namespace DeepNestLib.CiTests
{
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PmapWorkerProcessSecondFixture : PmapWorkerFixtureSetup
  {
    private NfpPair processed;

    public PmapWorkerProcessSecondFixture()
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
      processed.nfp.Should().NotBeNull();
    }

    [Fact]
    public void ShouldSetNfpHeight()
    {
      processed.nfp.HeightCalculated.Should().Be(22);
    }

    [Fact]
    public void ShouldSetNfpWidth()
    {
      processed.nfp.WidthCalculated.Should().Be(22);
    }

    [Fact]
    public void ShouldSetNfpArea()
    {
      processed.nfp.Area.Should().Be(484);
    }

    [Fact]
    public void ShouldSetPoints()
    {
      processed.nfp.Points.Should().BeEquivalentTo(expectedPoints2, options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }
  }
}
