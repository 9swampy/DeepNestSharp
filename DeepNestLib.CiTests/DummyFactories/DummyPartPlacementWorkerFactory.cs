namespace DeepNestLib.CiTests.DummyFactories
{
  using System.Collections.Generic;
  using System.Diagnostics;
  using FakeItEasy;

  internal class DummyPartPlacementWorkerFactory : DummyFactory<PartPlacementWorker>
  {
    protected override PartPlacementWorker Create()
    {
      return new PartPlacementWorker(new Dictionary<string, ClipCacheItem>());
    }
  }
}