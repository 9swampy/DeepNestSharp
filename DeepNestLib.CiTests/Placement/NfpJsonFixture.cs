namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using FluentAssertions;
  using Xunit;

  public class NfpJsonFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new NFP();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSutWhenToJsonThenShouldNotThrow()
    {
      var sut = new NFP();
      Action act = () => sut.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSquareWhenToJsonThenShouldNotThrow()
    {
      NFP firstSheet;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryImportFromRawDetail(3, out firstSheet).Should().BeTrue();

      Action act = () => _ = firstSheet.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSquareWhenToJsonThenShouldBeExpected()
    {
      NFP firstSheet;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryImportFromRawDetail(3, out firstSheet).Should().BeTrue();

      var json = firstSheet.ToJson();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SimpleRectangleNfp.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd();
        json.Should().Be(fromFile);
      }
    }

    [Fact]
    public void ShouldRoundTripSerialise()
    {
      NFP expected;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryImportFromRawDetail(3, out expected).Should().BeTrue();
      expected.Rotation = 12;

      var json = expected.ToJson();

      var actual = NFP.FromJson(json);

      actual.Should().BeEquivalentTo(expected);
      actual.Rotation.Should().Be(12);
    }
  }
}
