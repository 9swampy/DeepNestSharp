namespace DeepNestLib.CiTests
{
  using System;
  using System.Linq;
  using FluentAssertions;
  using Xunit;

  public class ExtensionsSpliceFixture
  {
    [Fact]
    public void Given1ArrayWhenSpliceThenNoneLeft()
    {
      var source = new[] { 1 };
      var actual = source.Splice(source.Length - 1, 1);
      actual.Length.Should().Be(0);
    }

    [Fact]
    public void Given2ArrayWhenSplice0ThenSecondLeft()
    {
      var source = new[] { 0, 1 };
      var actual = source.Splice(0, 1);
      actual.Length.Should().Be(1);
      actual[0].Should().Be(1);
    }

    [Fact]
    public void Given2ArrayWhenSplice1ThenFirstLeft()
    {
      var source = new[] { 0, 1 };
      var actual = source.Splice(1, 1);
      actual.Length.Should().Be(1);
      actual[0].Should().Be(0);
    }

    [Fact]
    public void Given3ArrayWhenSpliceHigherThanArrayLengthThenAllReturned()
    {
      var source = new[] { 0, 1, 2 };
      var actual = source.Splice(new Random().Next(3, 50), 1);
      actual.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Given3ArrayWhenSplice3ThenAllReturned()
    {
      var source = new[] { 0, 1, 2 };
      var actual = source.Splice(3, 1);
      actual.Should().BeEquivalentTo(source);
    }

    [Fact]
    public void Given3ArrayWhenSplice2ThenAllReturned()
    {
      var source = new[] { 0, 1, 2 };
      int toRemove = 2;
      var actual = source.Splice(toRemove, 1);
      var expected = source.ToList();
      expected.RemoveAt(toRemove);
      actual.Should().BeEquivalentTo(expected.ToArray());
    }

    [Fact]
    public void Given3ArrayWhenSplice0ThenAllReturned()
    {
      var source = new[] { 0, 1, 2 };
      int toRemove = 0;
      var actual = source.Splice(toRemove, 1);
      var expected = source.ToList();
      expected.RemoveAt(toRemove);
      actual.Should().BeEquivalentTo(expected.ToArray());
    }

    [Fact]
    public void Given3ArrayWhenSplice1ThenAllReturned()
    {
      var source = new[] { 0, 1, 2 };
      int toRemove = 0;
      var actual = source.Splice(toRemove, 1);
      var expected = source.ToList();
      expected.RemoveAt(toRemove);
      actual.Should().BeEquivalentTo(expected.ToArray());
    }
  }

}
