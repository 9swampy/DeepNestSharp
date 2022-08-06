namespace DeepNestLib.CiTests.PairMap
{
  using DeepNestLib.PairMap;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class PmapWorkerPmapDeepNestFixture : PmapWorkerFixtureSetup
  {
    private NfpPair[] processed;

    public PmapWorkerPmapDeepNestFixture()
      : base(11D, 11D)
    {
      var pairs = new NfpPair[] { pair1, pair2 };
      processed = new PmapWorker(pairs, A.Fake<IProgressDisplayer>(), false, A.Dummy<MinkowskiSum>(), A.Dummy<NestState>()).PmapDeepNest();
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
      processed[0].Nfp.Points.Should().BeEquivalentTo(ExpectedPoints1, options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                      .WhenTypeIs<double>());
    }

    [Fact]
    public void ShouldSetPoints2()
    {
      processed[1].Nfp.Points.Should().BeEquivalentTo(ExpectedPoints2, options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                                    .WhenTypeIs<double>());
    }
  }
}
