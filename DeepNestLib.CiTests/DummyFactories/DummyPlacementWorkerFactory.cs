namespace DeepNestLib.CiTests.DummyFactories
{
  using FakeItEasy;

  internal class DummyPlacementWorkerFactory : DummyFactory<PlacementWorker>
  {
    protected override PlacementWorker Create()
    {
      return new PlacementWorker(A.Fake<IProgressDisplayer>(), A.Dummy<SvgNest>(), A.Dummy<MinkowskiSum>(), A.Dummy<NestState>(), new WindowUnk());
    }
  }
}