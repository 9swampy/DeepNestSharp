namespace DeepNestLib.CiTests.DummyFactories
{
  using FakeItEasy;

  internal class DummyNestStateFactory : DummyFactory<NestState>
  {
    protected override NestState Create()
    {
      return new NestState(SvgNest.Config, A.Fake<IDispatcherService>());
    }
  }
}
