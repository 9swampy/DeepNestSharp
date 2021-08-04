namespace DeepNestLib.CiTests.DummyFactories
{
  using System.Collections.ObjectModel;
  using DeepNestLib.Placement;
  using FakeItEasy;

  internal class DummyNestStateFactory : DummyFactory<NestState>
  {
    protected override NestState Create()
    {
      return new NestState(SvgNest.Config, A.Fake<IDispatcherService>());
    }
  }
}
