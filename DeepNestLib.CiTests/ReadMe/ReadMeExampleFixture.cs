namespace DeepNestLib.CiTests.ReadMe
{
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;
  using FakeItEasy;
  using FluentAssertions;
  using System.IO;
  using System.Linq;
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
        var relativePathHelper = A.Dummy<IRelativePathHelper>();
        relativePathHelper.GetSolutionDirectory().Should().NotBeNullOrWhiteSpace();
        var original = reader.ReadToEnd();
        stream.Position = 0;
        ProjectInfo expected = ProjectInfo.LoadFromStream(expectedConfig, reader, relativePathHelper);
        expected.DetailLoadInfos.Count.Should().Be(12);
        expected.DetailLoadInfos[0].Path.Should().StartWith(relativePathHelper.GetSolutionDirectory());
        expected.DetailLoadInfos.Any(o => !o.Path.StartsWith(relativePathHelper.GetSolutionDirectory())).Should().BeFalse();

        var actualConfig = new TestSvgNestConfig();
        var json = expected.ToJson();
        var actual = ProjectInfo.FromJson(actualConfig, json, A.Dummy<IRelativePathHelper>());
        actual.DetailLoadInfos.Count.Should().Be(12);

        actual.Should().BeEquivalentTo(expected); //, opt => opt); //.Excluding(o => o.DetailLoadInfos));
        actual.DetailLoadInfos.Should().BeEquivalentTo(
          expected.DetailLoadInfos,
          opt => opt.Excluding(o => o.Path)
                    .Excluding(o => o.Name));
        actualConfig.Should().BeEquivalentTo(expectedConfig);
        actual.DetailLoadInfos.Count.Should().Be(12);
        actual.DetailLoadInfos[6].IsDifferentiated.Should().BeTrue();
      }
    }
  }
}
