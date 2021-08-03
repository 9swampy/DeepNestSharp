namespace DeepNestLib.CiTests
{
  using FakeItEasy;

  internal class DummyNestStateFactory : DummyFactory<NestState>
  {
    protected override NestState Create()
    {
      return NestState.Default;
    }
  }

  internal class DummyMinkowskiSumFactory : DummyFactory<MinkowskiSum>
  {
    protected override MinkowskiSum Create()
    {
      return (MinkowskiSum)MinkowskiSum.CreateInstance(NestState.CreateInstance(SvgNest.Config));
    }
  }
}
