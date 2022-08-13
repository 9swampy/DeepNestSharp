namespace DeepNestLib.CiTests.ReadMe
{
  using DeepNestLib.NestProject;
  using FluentAssertions;
  using System.IO;
  using System.Reflection;
  using Xunit;

  public class ReadMeExampleFixture
  {
    [Fact]
    public void ProjectShouldRoundTripSerialize()
    {
      var expectedConfig = new TestSvgNestConfig();
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("ReadMe.ReadMeExampleSmall.dnest"))
      using (StreamReader reader = new StreamReader(stream))
      {
        ProjectInfo expected = ProjectInfo.LoadFromStream(expectedConfig, reader);

        var actualConfig = new TestSvgNestConfig();
        var json = expected.ToJson();
        var actual = ProjectInfo.FromJson(actualConfig, json);

        actual.Should().BeEquivalentTo(expected);
        actualConfig.Should().BeEquivalentTo(expectedConfig);
        actual.DetailLoadInfos.Count.Should().Be(12);
        actual.DetailLoadInfos[6].IsDifferentiated.Should().BeTrue();
      }
    }
  }
}
