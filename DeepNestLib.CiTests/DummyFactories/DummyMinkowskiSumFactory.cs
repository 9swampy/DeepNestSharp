namespace DeepNestLib.CiTests.DummyFactories
{
  using FakeItEasy;

  internal class DummyMinkowskiSumFactory : DummyFactory<MinkowskiSum>
  {
    protected override MinkowskiSum Create()
    {
      return (MinkowskiSum)MinkowskiSum.CreateInstance(NestState.CreateInstance(SvgNest.Config, A.Fake<IDispatcherService>()));
    }
  }
}
