namespace DeepNestLib.CiTests.IO
{
  using System;
  using DeepNestLib.IO;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class RelativePathHelperFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new RelativePathHelper(AppDomain.CurrentDomain.RelativeSearchPath);

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldRoundTripSolutionSubstitution()
    {
      var sut = A.Dummy<IRelativePathHelper>();
      var interim = sut.ConvertToRelativePath($"{AppDomain.CurrentDomain.BaseDirectory}");
      interim.Should().NotBe(sut.GetSolutionDirectory());
      interim.Should().StartWith("${SolutionDir}");
      sut.ConvertToFullPath(interim).Should().Be(AppDomain.CurrentDomain.BaseDirectory);
    }

    [Fact]
    public void GivenInvalidRootShouldStillRoundTripSolutionSubstitution()
    {
      var sut = new RelativePathHelper(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NotExists"));
      var interim = sut.ConvertToRelativePath($"{AppDomain.CurrentDomain.BaseDirectory}");
      interim.Should().NotBe(sut.GetSolutionDirectory());
      interim.Should().StartWith("${SolutionDir}");
      sut.ConvertToFullPath(interim).Should().Be(AppDomain.CurrentDomain.BaseDirectory);
    }

    [Fact]
    public void GivenInvalidRootSuffixGetSolutionShouldStillFindRoot()
    {
      var invalidPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NotExists");
      var sut = new RelativePathHelper(invalidPath);
      sut.GetSolutionDirectory().Should().Be(new RelativePathHelper(AppDomain.CurrentDomain.BaseDirectory).GetSolutionDirectory());
    }
  }
}
