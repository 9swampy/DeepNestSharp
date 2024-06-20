namespace DeepNestLib.CiTests
{
  using DeepNestLib.CiTests.IO;
  using FluentAssertions;
  using Xunit;

  public class OutPtFixture
  {
    [Fact]
    public void GivenTriangleThenOutPtChainedLoopShouldBeExpected()
    {
      ClipperLib.OutPt sut = NfpSimplifier.ToOutPt(NoFitPolygon.FromDxf(new DxfGenerator().IsoscelesTriangle(10)));
      ClipperLib.OutPt origin = sut;
      do
      {
        sut.Next.Should().NotBeNull();
        sut = sut.Next;
        sut.Pt.Should().NotBeNull();
        sut.Prev.Should().NotBeNull();
        if (sut != origin)
        {
          sut.Idx.Should().Be(sut.Prev.Idx + 1);
        }
      } while (origin != sut);

      sut.Should().Be(origin);
    }
  }
}
