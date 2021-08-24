namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class SheetPlacementJsonFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), A.Dummy<Sheet>(), A.Dummy<List<IPartPlacement>>(), 0);

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSutWhenToJsonThenShouldNotThrow()
    {
      var nfp = new Sheet();
      var partPlacements = new List<IPartPlacement>();
      var sut = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), A.Dummy<Sheet>(), partPlacements, 0);
      Action act = () => sut.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSquareOnSheetPlacementWhenToJsonThenShouldBeExpected()
    {
      ISheet firstSheet;
      new DxfGenerator().GenerateRectangle("firstSheet", 5D, 5D, RectangleType.FileLoad).TryConvertToSheet(3, out firstSheet).Should().BeTrue();
      INfp firstPart;
      new DxfGenerator().GenerateRectangle("firstPart", 1D, 2D, RectangleType.FileLoad).TryConvertToNfp(3, out firstPart).Should().BeTrue();
      firstPart.X = 3;
      firstPart.Y = 4;
      var sut = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), firstSheet, new List<IPartPlacement>() { new PartPlacement(firstPart) { X = 1, Y = 2, Rotation = 90, Id = 10, } }, 0);

      var json = sut.ToJson();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SimpleRectangleNfpOnSheetPlacement.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
        json.Should().Be(fromFile);
      }
    }

    [Fact]
    public void GivenSimpleSquareOnSheetPlacementWhenToJsonThenShouldRoundTrip()
    {
      ISheet firstSheet;
      new DxfGenerator().GenerateRectangle("firstSheet", 5D, 5D, RectangleType.FileLoad).TryConvertToSheet(3, out firstSheet).Should().BeTrue();
      INfp firstPart;
      new DxfGenerator().GenerateRectangle("firstPart", 1D, 2D, RectangleType.FileLoad).TryConvertToNfp(4, out firstPart).Should().BeTrue();
      var expected = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), firstSheet, new List<IPartPlacement>() { new PartPlacement(firstPart) }, 0);

      var json = expected.ToJson();
      var actual = SheetPlacement.FromJson(json);

      actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void SheetJsonConverterCannotConvertSheet()
    {
      var sut = new SheetJsonConverter();
      sut.CanConvert(typeof(Sheet)).Should().BeFalse();
    }

    [Fact]
    public void SheetJsonConverterCanConvertISheet()
    {
      var sut = new SheetJsonConverter();
      sut.CanConvert(typeof(ISheet)).Should().BeTrue();
    }

    [Fact]
    public void SheetJsonConverterCannotConvertNfp()
    {
      var sut = new SheetJsonConverter();
      sut.CanConvert(typeof(NFP)).Should().BeFalse();
    }

    [Fact]
    public void SheetJsonConverterCannotConvertINfp()
    {
      var sut = new SheetJsonConverter();
      sut.CanConvert(typeof(INfp)).Should().BeFalse();
    }

    [Fact]
    public void NfpJsonConverterCannotConvertNfp()
    {
      var sut = new NfpJsonConverter();
      sut.CanConvert(typeof(NFP)).Should().BeFalse();
    }

    [Fact]
    public void NfpJsonConverterCanConvertINfp()
    {
      var sut = new NfpJsonConverter();
      sut.CanConvert(typeof(INfp)).Should().BeTrue();
    }

    [Fact]
    public void NfpJsonConverterCannotConvertSheet()
    {
      var sut = new NfpJsonConverter();
      sut.CanConvert(typeof(Sheet)).Should().BeFalse();
    }

    [Fact]
    public void NfpJsonConverterCannotConvertISheet()
    {
      var sut = new NfpJsonConverter();
      sut.CanConvert(typeof(ISheet)).Should().BeFalse();
    }
  }
}
