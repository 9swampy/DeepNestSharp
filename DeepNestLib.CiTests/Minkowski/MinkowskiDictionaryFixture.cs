namespace DeepNestLib.CiTests
{
  using System;
  using FluentAssertions;
  using Xunit;

  public class MinkowskiDictionaryFixture
  {
    [Fact]
    public void ShouldSerialize()
    {
      var sut = new MinkowskiDictionary();
      var json = sut.ToJson();
    }

    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var sut = new MinkowskiDictionary();
      var nfp = new NoFitPolygon();
      sut.Add(new MinkowskiKey(1, new double[] { 1, 2 }, 2, new int[] { 3, 4 }, new double[] { 1, 2 }, 5, new double[] { 6, 8 }), nfp);
      var json = sut.ToJson();

      MinkowskiDictionary.FromJson(json).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void ShouldThrowWhenAddSameItemTwice()
    {
      var sut = new MinkowskiDictionary();
      var nfp = new NoFitPolygon();
      Action act = () => sut.Add(new MinkowskiKey(1, new double[] { 1, 2 }, 2, new int[] { 3, 4 }, new double[] { 1, 2 }, 5, new double[] { 6, 8 }), nfp);

      act.Should().NotThrow();

      act.Should().Throw<ArgumentException>().WithMessage("An item with the same key has already been added. Key: a1-ac2-arr2-h2-b5-bp2");
    }
  }
}
