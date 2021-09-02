namespace DeepNestLib.CiTests.DummyFactories
{
  using FakeItEasy;

  internal class DummyMinkowskiSumFactory : DummyFactory<MinkowskiSum>
  {
    protected override MinkowskiSum Create()
    {
      var config = new DefaultSvgNestConfig();
      return (MinkowskiSum)MinkowskiSum.CreateInstance(config, NestState.CreateInstance(config, A.Fake<IDispatcherService>()));
    }
  }
}
