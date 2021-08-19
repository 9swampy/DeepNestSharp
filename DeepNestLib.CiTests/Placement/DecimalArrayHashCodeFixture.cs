namespace DeepNestLib.CiTests.Placement
{
  using System.Collections;
  using System.Collections.Generic;
using System.Linq;
  using FluentAssertions;
  using Xunit;

  public class DecimalArrayHashCodeFixture
  {
    [Fact]
    public void DifferingItem7HashCodeShouldBeDifferent()
    {
      var sut = new decimal[]
      {
        40.0815659M,
        -33.7844009M,
        40.0815659M,
        16.2155972M,
        -9.9184322M,
        16.2155972M,
        -9.9184322M,
        -33.7844009M,
      };

      var sutOther = new decimal[]
      {
        -40.0815659M,
        33.7844009M,
        -40.0815659M,
        -16.2155972M,
        9.9184322M,
        -16.2155972M,
        9.9184322M,
        33.7844009M,
      };

      ((IStructuralEquatable)sutOther.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default).Should().NotBe(((IStructuralEquatable)sut.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default));
    }

    [Fact]
    public void DifferingItem7StringHashCodeShouldBeDifferent()
    {
      var sut = new string[]
      {
        "40.0815659M",
        "-33.7844009M",
        "40.0815659M",
        "16.2155972M",
        "-9.9184322M",
        "16.2155972M",
        "-9.9184322M",
        "-33.7844009M",
      };

      var sutOther = new string[]
      {
        "-40.0815659M",
        "33.7844009M",
        "-40.0815659M",
        "-16.2155972M",
        "9.9184322M",
        "-16.2155972M",
        "9.9184322M",
        "33.7844009M",
      };

      ((IStructuralEquatable)sutOther).GetHashCode(EqualityComparer<string>.Default).Should().NotBe(((IStructuralEquatable)sut).GetHashCode(EqualityComparer<string>.Default));
    }

    [Fact]
    public void SameItem7HashCodeShouldBeSame()
    {
      var sut = new decimal[]
      {
        40.0815659M,
        -33.7844009M,
        40.0815659M,
        16.2155972M,
        -9.9184322M,
        16.2155972M,
        -9.9184322M,
        -33.7844009M,
      };

      var sutOther = new decimal[]
      {
        40.0815659M,
        -33.7844009M,
        40.0815659M,
        16.2155972M,
        -9.9184322M,
        16.2155972M,
        -9.9184322M,
        -33.7844009M,
      };

      ((IStructuralEquatable)sutOther).GetHashCode(EqualityComparer<decimal>.Default).Should().Be(((IStructuralEquatable)sut).GetHashCode(EqualityComparer<decimal>.Default));
    }
  }
}