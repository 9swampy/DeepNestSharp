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
      Action act = () => _ = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), A.Dummy<NFP>(), A.Dummy<List<PartPlacement>>());

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSutWhenToJsonThenShouldNotThrow()
    {
      var nfp = new NFP();
      var partPlacements = new List<PartPlacement>();
      var sut = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), nfp, partPlacements);
      Action act = () => sut.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenSimpleSquareOnSheetPlacementWhenToJsonThenShouldBeExpected()
    {
      NFP firstSheet;
      new DxfGenerator().GenerateRectangle("firstSheet", 5D, 5D, RectangleType.FileLoad).TryImportFromRawDetail(3, out firstSheet).Should().BeTrue();
      NFP firstPart;
      new DxfGenerator().GenerateRectangle("firstPart", 1D, 2D, RectangleType.FileLoad).TryImportFromRawDetail(3, out firstPart).Should().BeTrue();

      var sut = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), firstSheet, new List<PartPlacement>() { new PartPlacement(firstPart) });

      var json = firstSheet.ToJson();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.SimpleRectangleNfpOnSheetPlacement.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd();
        json.Should().Be(fromFile);
      }
    }

    [Fact]
    public void GivenSimpleSquareOnSheetPlacementWhenToJsonThenShouldRoundTrip()
    {
      NFP firstSheet;
      new DxfGenerator().GenerateRectangle("firstSheet", 5D, 5D, RectangleType.FileLoad).TryImportFromRawDetail(3, out firstSheet).Should().BeTrue();
      NFP firstPart;
      new DxfGenerator().GenerateRectangle("firstPart", 1D, 2D, RectangleType.FileLoad).TryImportFromRawDetail(3, out firstPart).Should().BeTrue();
      var expected = new SheetPlacement(A.Dummy<PlacementTypeEnum>(), firstSheet, new List<PartPlacement>() { new PartPlacement(firstPart) });

      var json = expected.ToJson();
      var actual = SheetPlacement.FromJson(json);

      actual.Should().BeEquivalentTo(expected);
    }
  }
}
