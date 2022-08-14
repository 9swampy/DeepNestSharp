namespace DeepNestLib.CiTests.NestProject
{
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class ProjectInfoSerializationFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var config = SvgNest.Config;
      var sut = new ProjectInfo(config, A.Dummy<IRelativePathHelper>());
      sut.SheetLoadInfos.Should().NotBeEmpty();
      var json = sut.ToJson();
      ProjectInfo actual = ProjectInfo.FromJson(config, json, A.Dummy<IRelativePathHelper>());

      actual.Should().BeEquivalentTo(sut);
    }
  }
}
