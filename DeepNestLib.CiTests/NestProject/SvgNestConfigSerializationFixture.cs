namespace DeepNestLib.CiTests.NestProject
{
  using System;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.NestProject;
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
