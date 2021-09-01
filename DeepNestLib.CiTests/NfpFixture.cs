namespace DeepNestLib.CiTests
{
  using System;
  using FluentAssertions;
  using Xunit;

  public class NfpFixture
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
      var clone = sut.Clone();

      sut.Source.Should().Be(clone.Source);
    }

    [Fact]
    public void GivenNfpWhenClonedThenPointsShouldBeEquivalent()
    {
      var sut = new NFP();
      var clone = sut.Clone();

      sut.Points.Should().BeEquivalentTo(clone.Points);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenEmptyChildrenThenShouldBeEquivalent()
    {
      var sut = new NFP();
      var clone = sut.Clone();

      sut.Children.Should().BeEquivalentTo(clone.Children);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenHasChildrenThenChildrenPointsShouldBeEquivalent()
    {
      var sut = new NFP();
      sut.Children.Add(new NFP());
      var clone = sut.Clone();

      sut.Children[0].Points.Should().BeEquivalentTo(clone.Children[0].Points);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenHasChildrenThenChildrenSourceShouldBeCopiedRefactoringBugFix()
    {
      var sut = new NFP();
      sut.Children.Add(new NFP() { Source = 1 });
      var clone = sut.Clone();

      clone.Children[0].Source.Should().Be(sut.Children[0].Source, "child Source wasn't originally copied. Note this is a refactoring; I had to remove nullable as a null would kill the PlaceParts logic; can't ever be valid; and then had to copy when there was a value.");
    }

    [Fact]
    public void GivenNfpWhenCloneTreeThenShouldBeEquivalent()
    {
      var random = new Random();
      var sut = new NFP();
      sut.IsPriority = true;
      sut.StrictAngle = DeepNestLib.NestProject.AnglesEnum.Vertical;
      sut.Name = DateTime.Now.ToString();
      var firstPoint = new SvgPoint(0, 0);
      sut.AddPoint(firstPoint);
      sut.AddPoint(new SvgPoint(random.Next(), random.Next()));
      sut.AddPoint(new SvgPoint(random.Next(), random.Next()));
      var clone = sut.CloneTree();
      clone.Should().BeEquivalentTo(sut);
      firstPoint.X = -1;
      clone.Should().NotBeEquivalentTo(sut, "points get cloned aren't referenced in the clone.");
    }
  }
}
