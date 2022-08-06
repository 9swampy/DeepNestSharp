namespace DeepNestLib.CiTests.DummyFactories
{
  using DeepNestLib.Placement;
  using FakeItEasy;

  internal class DummyNfpHelperFactory : DummyFactory<NfpHelper>
  {
    protected override NfpHelper Create()
    {
      return new NfpHelper(A.Dummy<MinkowskiSum>(), new WindowUnk());
    }
  }
}
