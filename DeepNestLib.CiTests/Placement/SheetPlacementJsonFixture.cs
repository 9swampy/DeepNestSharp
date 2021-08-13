namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
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
      Action act = () => _ = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), A.Dummy<Sheet>(), A.Dummy<List<IPartPlacement>>());

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSutWhenToJsonThenShouldNotThrow()
    {
      var nfp = new Sheet();
      var partPlacements = new List<IPartPlacement>();
      var sut = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), A.Dummy<Sheet>(), partPlacements);
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
      var sut = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), firstSheet, new List<IPartPlacement>() { new PartPlacement(firstPart) { X = 1, Y = 2, Rotation = 90, Id = 10, } });

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
      var expected = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), firstSheet, new List<IPartPlacement>() { new PartPlacement(firstPart) });

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

  public class NestResultJsonFixture
  {
    [Fact]
    public void ShouldCtorSimpleNestResult()
    {
      var nfp = new NFP() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(1, 0), new SvgPoint(1, 1), new SvgPoint(0, 1), } };
      var partPlacement = new PartPlacement(nfp) { X = 11, Y = 22 };
      var partPlacementList = new List<IPartPlacement>();
      partPlacementList.Add(partPlacement);
      var sheet = new Sheet() { Width = 100, Height = 200 };
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList);
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      Action act = () => _ = new NestResult(1, sheetPlacementsCollection, new List<INfp>(), 121, sheetPlacement.PlacementType, 1234, 4321);

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldSerializeSimpleNestResult()
    {
      var nfp = new NFP() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(1, 0), new SvgPoint(1, 1), new SvgPoint(0, 1), } };
      var partPlacement = new PartPlacement(nfp) { X = 11, Y = 22 };
      var partPlacementList = new List<IPartPlacement>();
      partPlacementList.Add(partPlacement);
      var sheet = Sheet.NewSheet(1, 100, 200);
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList);
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      var nestResult = new NestResult(1, sheetPlacementsCollection, new List<INfp>(), 121, sheetPlacement.PlacementType, 1234, 4321);
      Action act = () => _ = nestResult.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldRoundTripSerializeSimpleNestResult()
    {
      var nfp = new NFP() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(1, 0), new SvgPoint(1, 1), new SvgPoint(0, 1), } };
      var unplacedNfp = new NFP() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(2, 0), new SvgPoint(2, 2), new SvgPoint(0, 2), } };
      var partPlacement = new PartPlacement(nfp) { X = 11, Y = 22 };
      var partPlacementList = new List<IPartPlacement>();
      partPlacementList.Add(partPlacement);
      var sheet = Sheet.NewSheet(1, 100, 200);
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList);
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      var nestResult = new NestResult(1, sheetPlacementsCollection, new List<INfp>() { unplacedNfp }, 121, sheetPlacement.PlacementType, 1234, 4321);
      var json = nestResult.ToJson();
      var actual = NestResult.FromJson(json);

      actual.Should().BeEquivalentTo(nestResult, options =>
            options// .Including(o => o.MergedLength)
                   // .Including(o => o.UnplacedParts)
                   .Including(o => o.UsedSheets)
                   .Excluding(o => o.Fitness)
                   .Excluding(o => o.CreatedAt));
      //.ExcludingProperties());
    }
  }
}
