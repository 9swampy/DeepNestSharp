namespace MinkowskiWrapper.CiTests
{
  using System.IO;
  using FluentAssertions;
  using Xunit;

  public class CheckMinkowskiDllFixture
  {
#if x86
    [Fact]
    public void PreX86DllShouldExist()
    {
        new FileInfo("minkowski_x86.dll").Exists.Should().BeTrue($"{new FileInfo("minkowski.dll").FullName} is needed");
    }
#elif x64
    [Fact]
    public void PreX64DllShouldExist()
    {
      new FileInfo("minkowski_x64.dll").Exists.Should().BeTrue($"{new FileInfo("minkowski.dll").FullName} is needed");
    }
#else
    [Fact]
    public void PreAnyCpuDllShouldExist()
    {
      new FileInfo("minkowski.dll").Exists.Should().BeTrue($"{new FileInfo("minkowski.dll").FullName} is needed");
    }
#endif
  }
}
