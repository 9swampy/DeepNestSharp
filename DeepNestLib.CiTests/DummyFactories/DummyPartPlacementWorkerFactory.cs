namespace DeepNestLib.CiTests.DummyFactories
{
  using System.Collections.Generic;
  using System.Diagnostics;
  using FakeItEasy;

  internal class DummyPartPlacementWorkerFactory : DummyFactory<PartPlacementWorker>
  {
    protected override PartPlacementWorker Create()
    {
      var result = new PartPlacementWorker(new Dictionary<string, ClipCacheItem>());
      var nfpHelper = new NfpHelper();
      ((ITestNfpHelper)nfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<INestStateMinkowski>());
      ((ITestPartPlacementWorker)result).NfpHelper = nfpHelper;
      return result;
    }
  }
}