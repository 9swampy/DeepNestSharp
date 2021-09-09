namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class NestResultJsonFixture
  {
    [Fact]
    public void ShouldCtorSimpleNestResult()
    {
      var nfp = new NoFitPolygon() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(1, 0), new SvgPoint(1, 1), new SvgPoint(0, 1), } };
      var partPlacement = new PartPlacement(nfp) { X = 11, Y = 22 };
      var partPlacementList = new List<IPartPlacement>();
      partPlacementList.Add(partPlacement);
      var sheet = new Sheet() { Width = 100, Height = 200 };
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList, 0, A.Dummy<double>());
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      Action act = () => _ = new NestResult(1, sheetPlacementsCollection, new List<INfp>(), sheetPlacement.PlacementType, 1234, 4321);

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldSerializeSimpleNestResult()
    {
      var nfp = new NoFitPolygon() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(1, 0), new SvgPoint(1, 1), new SvgPoint(0, 1), } };
      var partPlacement = new PartPlacement(nfp) { X = 11, Y = 22 };
      var partPlacementList = new List<IPartPlacement>();
      partPlacementList.Add(partPlacement);
      var sheet = Sheet.NewSheet(1, 100, 200);
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList, 121, A.Dummy<double>());
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      var nestResult = new NestResult(1, sheetPlacementsCollection, new List<INfp>(), sheetPlacement.PlacementType, 1234, 4321);
      Action act = () => _ = nestResult.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldRoundTripSerializeSimpleNestResult()
    {
      var nfp = new NoFitPolygon() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(1, 0), new SvgPoint(1, 1), new SvgPoint(0, 1), } };
      var unplacedNfp = new NoFitPolygon() { Points = new SvgPoint[] { new SvgPoint(0, 0), new SvgPoint(2, 0), new SvgPoint(2, 2), new SvgPoint(0, 2), } };
      var partPlacement = new PartPlacement(nfp) { X = 11, Y = 22 };
      var partPlacementList = new List<IPartPlacement>();
      partPlacementList.Add(partPlacement);
      var sheet = Sheet.NewSheet(1, 100, 200);
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList, 121, A.Dummy<double>());
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      var nestResult = new NestResult(1, sheetPlacementsCollection, new List<INfp>() { unplacedNfp }, sheetPlacement.PlacementType, 1234, 4321);
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
