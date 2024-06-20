namespace DeepNestLib.CiTests.NestProject
{
  using DeepNestLib.NestProject;
  using FluentAssertions;
  using Xunit;

  public class ProjectInfoSerializationFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      ISvgNestConfig config = SvgNest.Config;
      ProjectInfo sut = new ProjectInfo(config);
      sut.SheetLoadInfos.Should().NotBeEmpty();
      var json = sut.ToJson();
      ProjectInfo actual = ProjectInfo.FromJson(config, json);

      actual.Should().BeEquivalentTo(sut);
    }
  }
}
