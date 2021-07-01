namespace DeepNestLib.CiTests
{
  using System;
  using FluentAssertions;
  using Xunit;

  public class NpfFixture
  {
    [Fact]
    public void ShouldNotThrowOnCtor()
    {
      Action act = () => new NFP();

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldCtor()
    {
      var sut = new NFP();

      sut.Should().NotBeNull();
    }

    [Fact]
    public void GivenNfpWhenClonedThenSourceShouldBeEqual()
    {
      var sut = new NFP();
      var clone = Background.Clone(sut);

      sut.Source.Should().Be(clone.Source);
    }

    [Fact]
    public void GivenNfpWhenClonedThenPointsShouldBeEquivalent()
    {
      var sut = new NFP();
      var clone = Background.Clone(sut);

      sut.Points.Should().BeEquivalentTo(clone.Points);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenEmptyChildrenThenShouldBeEquivalent()
    {
      var sut = new NFP();
      var clone = Background.Clone(sut);

      sut.Children.Should().BeEquivalentTo(clone.Children);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenHasChildrenThenChildrenPointsShouldBeEquivalent()
    {
      var sut = new NFP();
      sut.Children.Add(new NFP());
      var clone = Background.Clone(sut);

      sut.Children[0].Points.Should().BeEquivalentTo(clone.Children[0].Points);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenHasChildrenThenChildrenSourceShouldBeNull()
    {
      var sut = new NFP();
      sut.Children.Add(new NFP() { Source = 1 });
      var clone = Background.Clone(sut);

      clone.Children[0].Source.Should().Be(null, "child Source is not copied. Note this is a refactoring; I don't see why it shouldn't be cloned.");
    }
  }
}
