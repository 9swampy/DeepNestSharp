namespace DeepNestLib.CiTests.DummyFactories
{
  using DeepNestLib.Placement;
  using FakeItEasy;

  internal class DummyMinkowskiSumFactory : DummyFactory<MinkowskiSum>
  {
    protected override MinkowskiSum Create()
    {
      var config = new TestSvgNestConfig();
      return (MinkowskiSum)MinkowskiSum.CreateInstance(config, NestState.CreateInstance(config, A.Fake<IDispatcherService>()));
    }
  }
}
