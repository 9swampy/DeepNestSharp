namespace DeepNestLib.CiTests.NestProject
{
  using System;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.NestProject;
  using FluentAssertions;
  using Xunit;

  public class ProjectInfoSerializationFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var sut = new ProjectInfo();
      ProjectInfo actual = ProjectInfo.FromJson(sut.ToJson());

      actual.Should().BeEquivalentTo(sut);
    }
  }
}
