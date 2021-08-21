namespace DeepNestLib.CiTests
{
  using FluentAssertions;
  using Xunit;

  public class NfpRotateFixture
  {
    [Fact]
    public void GivenPartWithAsymetricHolesRotatedClockwiseWhenRotatedBackThenShouldBeEquivalent()
    {
      var sut = DxfParser.LoadDxfStream("Dxfs._10.dxf").ToNfp();
      sut.Rotate(90).Rotate(-90).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GivenPartWithAsymetricHolesWhenRotatedClockwiseThenShouldHaveSameNumberOfPoints()
    {
      var sut = DxfParser.LoadDxfStream("Dxfs._10.dxf").ToNfp();
      sut.Rotate(90).Points.Length.Should().Be(sut.Points.Length);
    }

    [Fact]
    public void GivenPartWithAsymetricHolesWhenRotatedClockwiseThenShouldHaveSameNumberOfChildren()
    {
      var sut = DxfParser.LoadDxfStream("Dxfs._10.dxf").ToNfp();
      sut.Rotate(90).Children.Count.Should().Be(sut.Children.Count);
    }

    [Fact]
    public void GivenPartWithNoHolesRotatedClockwiseWhenRotatedBackThenShouldBeEquivalent()
    {
      var sut = DxfParser.LoadDxfStream("Dxfs._5.dxf").ToNfp();
      sut.Rotate(90).Rotate(-90).Should().BeEquivalentTo(sut);
    }
  }
}
