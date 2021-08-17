namespace DeepNestLib.CiTests.DummyFactories
{
  using System.Collections.Generic;
  using System.Diagnostics;
  using FakeItEasy;

  internal class DummyPlacementWorkerFactory : DummyFactory<PlacementWorker>
  {
    protected override PlacementWorker Create()
    {
      return new PlacementWorker(A.Dummy<NfpHelper>(), new List<ISheet>(), new INfp[0], A.Fake<ISvgNestConfig>(), A.Dummy<Stopwatch>(), A.Fake<INestState>());
    }
  }
}