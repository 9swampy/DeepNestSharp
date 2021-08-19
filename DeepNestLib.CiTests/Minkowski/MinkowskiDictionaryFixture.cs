namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using DeepNestLib.NestProject;
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
      var nfp = new NFP();
      sut.Add(new MinkowskiKey(1, new double[] { 1, 2 }, 2, new int[] { 3, 4 }, new double[] { 1, 2 }, 5, new double[] { 6, 8 }), nfp);
      var json = sut.ToJson();

      MinkowskiDictionary.FromJson(json).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void ShouldThrowWhenAddSameItemTwice()
    {
      var sut = new MinkowskiDictionary();
      var nfp = new NFP();
      Action act = () => sut.Add(new MinkowskiKey(1, new double[] { 1, 2 }, 2, new int[] { 3, 4 }, new double[] { 1, 2 }, 5, new double[] { 6, 8 }), nfp);

      act.Should().NotThrow();

      act.Should().Throw<ArgumentException>().WithMessage("An item with the same key has already been added.");
    }
  }
}
