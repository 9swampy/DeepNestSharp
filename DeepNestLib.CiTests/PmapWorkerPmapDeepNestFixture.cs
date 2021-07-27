namespace DeepNestLib.CiTests
{
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PmapWorkerPmapDeepNestFixture : PmapWorkerFixtureSetup
  {
    private NfpPair[] processed;

    public PmapWorkerPmapDeepNestFixture()
    {
      var pairs = new NfpPair[] { pair1, pair2 };
      processed = new PmapWorker(pairs, A.Fake<IProgressDisplayer>(), false, MinkowskiSum.CreateInstance()).PmapDeepNest();
    }

    [Fact]
    public void ShouldGenerateAResult()
    {
      processed.Should().NotBeNull();
    }

    [Fact]
    public void ShouldGenerate2Pairs()
    {
      processed.Length.Should().Be(2);
    }

    [Fact]
    public void ShouldSetPoints1()
    {
      processed[0].nfp.Points.Should().BeEquivalentTo(PmapWorkerProcessFixture.expectedPoints1, options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }

    [Fact]
    public void ShouldSetPoints2()
    {
      processed[1].nfp.Points.Should().BeEquivalentTo(PmapWorkerProcessSecondFixture.expectedPoints2, options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }
  }
}
