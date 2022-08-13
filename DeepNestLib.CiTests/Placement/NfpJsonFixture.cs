namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using DeepNestLib.CiTests.IO;
  using FluentAssertions;
  using Xunit;

  public class NfpJsonFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new NoFitPolygon();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSutWhenToJsonThenShouldNotThrow()
    {
      var sut = new NoFitPolygon();
      Action act = () => sut.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSquareWhenToJsonThenShouldNotThrow()
    {
      ISheet firstSheet;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryConvertToSheet(3, out firstSheet).Should().BeTrue();

      Action act = () => _ = firstSheet.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSquareNfpWhenToJsonThenShouldBeExpected()
    {
      INfp firstPart;
      new DxfGenerator().GenerateRectangle("Part", 1D, 2D, RectangleType.FileLoad).TryConvertToNfp(3, out firstPart).Should().BeTrue();

      var json = firstPart.ToJson();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SimpleRectangleNfp.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd();
        json.Should().Be(fromFile);
      }
    }

    [Fact]
    public void GivenSimpleSquareSheetWhenToJsonThenShouldBeExpected()
    {
      ISheet firstSheet;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryConvertToSheet(3, out firstSheet).Should().BeTrue();

      var json = firstSheet.ToJson();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SimpleRectangleSheet.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd();
        json.Should().Be(fromFile);
      }
    }

    [Fact]
    public void SheetClonedToNfpShouldRoundTripSerialiseHavingDroppedAddedSheetProperties()
    {
      ISheet expected;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryConvertToSheet(3, out expected).Should().BeTrue();
      expected = expected.Rotate(12) as ISheet;

      var json = expected.ToJson();

      var actual = NoFitPolygon.FromJson(json);

      actual.Should().BeEquivalentTo(expected, options =>
                   options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.0001))
                          .WhenTypeIs<double>());
      ((IEquatable<IPolygon>)actual).Equals(expected).Should().BeTrue();
      actual.Rotation.Should().Be(12);
    }

    [Fact]
    public void ViaSheetToJsonShouldRoundTripSerialiseFully()
    {
      ISheet expected;
      new DxfGenerator().GenerateRectangle("Sheet", 1D, 2D, RectangleType.FileLoad).TryConvertToSheet(3, out expected).Should().BeTrue();
      expected = expected.Rotate(12) as ISheet;
      expected.Rotation.Should().Be(12);

      var json = expected.ToJson();

      var actual = Sheet.FromJson(json);

      actual.Should().BeEquivalentTo(expected, options =>
                   options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.0001))
                          .WhenTypeIs<double>());
      actual.Rotation.Should().Be(12);
    }
  }
}
