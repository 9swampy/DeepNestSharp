namespace DeepNestLib.CiTests.NestProject
{
  using FluentAssertions;
  using Xunit;

  public class SvgNestConfigSerializationFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var sut = SvgNest.Config;
      var json = sut.ToJson();
      var actual = SvgNestConfig.FromJson(json);

      actual.Should().BeEquivalentTo(sut);
    }
  }
}
