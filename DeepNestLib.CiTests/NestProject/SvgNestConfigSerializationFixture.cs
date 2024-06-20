namespace DeepNestLib.CiTests.NestProject
{
  using FluentAssertions;
  using Xunit;

  public class SvgNestConfigSerializationFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      ISvgNestConfig sut = SvgNest.Config;
      var json = sut.ToJson();
      ISvgNestConfig actual = SvgNestConfig.FromJson(json);

      actual.Should().BeEquivalentTo(sut);
    }
  }
}
