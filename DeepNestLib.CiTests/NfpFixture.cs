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
      Action act = () => new NoFitPolygon();

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldCtor()
    {
      NoFitPolygon sut = new NoFitPolygon();

      sut.Should().NotBeNull();
    }

    [Fact]
    public void GivenNfpWhenClonedThenSourceShouldBeEqual()
    {
      NoFitPolygon sut = new NoFitPolygon();
      INfp clone = sut.Clone();

      sut.Source.Should().Be(clone.Source);
    }

    [Fact]
    public void GivenNfpWhenClonedThenPointsShouldBeEquivalent()
    {
      NoFitPolygon sut = new NoFitPolygon();
      INfp clone = sut.Clone();

      sut.Points.Should().BeEquivalentTo(clone.Points);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenEmptyChildrenThenShouldBeEquivalent()
    {
      NoFitPolygon sut = new NoFitPolygon();
      INfp clone = sut.Clone();

      sut.Children.Should().BeEquivalentTo(clone.Children);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenHasChildrenThenChildrenPointsShouldBeEquivalent()
    {
      NoFitPolygon sut = new NoFitPolygon();
      sut.Children.Add(new NoFitPolygon());
      INfp clone = sut.Clone();

      sut.Children[0].Points.Should().BeEquivalentTo(clone.Children[0].Points);
    }

    [Fact]
    public void GivenNfpWhenClonedWhenHasChildrenThenChildrenSourceShouldBeCopiedRefactoringBugFix()
    {
      NoFitPolygon sut = new NoFitPolygon();
      sut.Children.Add(new NoFitPolygon() { Source = 1 });
      INfp clone = sut.Clone();

      clone.Children[0].Source.Should().Be(sut.Children[0].Source, "child Source wasn't originally copied. Note this is a refactoring; I had to remove nullable as a null would kill the PlaceParts logic; can't ever be valid; and then had to copy when there was a value.");
    }

    [Fact]
    public void GivenNfpWhenCloneTreeThenShouldBeEquivalent()
    {
      Random random = new Random();
      NoFitPolygon sut = new NoFitPolygon();
      sut.IsPriority = true;
      sut.StrictAngle = DeepNestLib.NestProject.AnglesEnum.AsPreviewed;
      sut.Name = DateTime.Now.ToString();
      SvgPoint firstPoint = new SvgPoint(0, 0);
      sut.AddPoint(firstPoint);
      sut.AddPoint(new SvgPoint(random.Next(), random.Next()));
      sut.AddPoint(new SvgPoint(random.Next(), random.Next()));
      INfp clone = sut.CloneTree();
      clone.Should().BeEquivalentTo(sut);
      firstPoint.X = -1;
      clone.Should().NotBeEquivalentTo(sut, "points get cloned aren't referenced in the clone.");
    }
  }
}
