namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.NestProject;
  using FluentAssertions;
  using Xunit;

  public class NfpCtorCloneFixture
  {
    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var sut = new NoFitPolygon();
      NoFitPolygon.FromJson(sut.ToJson()).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void CtorCloneShouldSerializeToSame()
    {
      var children = new List<INfp>();
      var random = new Random();
      children.Add(new NoFitPolygon() { Points = new SvgPoint[] { new SvgPoint(0, 0) { Exact = true, Marked = false }, new SvgPoint(random.Next(), random.Next()) { Exact = random.NextBool(), Marked = random.NextBool() } } });
      var points = new List<SvgPoint>() { new SvgPoint(1, 1) { Exact = random.NextBool(), Marked = random.NextBool() }, new SvgPoint(random.Next(), random.Next()) { Exact = random.NextBool(), Marked = random.NextBool() } };
      var expected = new NoFitPolygon(points);
      expected.Children = children;
      expected.Id = random.Next();
      expected.IsPriority = random.NextBool();
      expected.IsPriority = random.NextBool();
      expected.Name = random.NextString();
      expected.OffsetX = random.NextDouble();
      expected.OffsetY = random.NextDouble();
      expected.PlacementOrder = random.Next();
      expected.Rotation = random.NextDouble();
      expected.Sheet = Sheet.NewSheet(random.Next(), random.Next(), random.Next());
      expected.Source = random.Next();
      expected.StrictAngle = random.Next<AnglesEnum>();
      expected.X = random.NextDouble();
      expected.Y = random.NextDouble();
      var expectedJson = expected.ToJson();

      var actual = new NoFitPolygon(expected, WithChildren.Included);
      actual.Should().BeEquivalentTo(expected);
      actual.ToJson().Should().Be(expectedJson);
      actual.Sheet.Should().Be(expected.Sheet, "it's a reference to the same sheet object.");
      actual.Children.Should().BeEquivalentTo(expected.Children);
      actual.Children[0].Should().NotBe(expected.Children[0]);

      actual.Points[0].X -= random.NextDouble();
      actual.Points[0].X.Should().NotBe(expected.Points[0].X, "points are cloned not referenced.");
      actual.Points[0].Y -= random.NextDouble();
      actual.Points[0].Y.Should().NotBe(expected.Points[0].Y, "points are cloned not referenced.");
    }
  }
}
