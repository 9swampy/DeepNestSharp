namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using DeepNestLib.GeneticAlgorithm;
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
      var sheet = Sheet.NewSheet(0, 100, 200);
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList, 0, A.Dummy<double>());
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      Action act = () => _ = new NestResult(A.Dummy<DeepNestGene>(), sheetPlacementsCollection, new List<INfp>(), sheetPlacement.PlacementType, 1234, 4321);

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
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList, 121, new TestSvgNestConfig().ClipperScale);
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      var nestResult = new NestResult(A.Dummy<DeepNestGene>(), sheetPlacementsCollection, new List<INfp>(), sheetPlacement.PlacementType, 1234, 4321);
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
      var sheetPlacement = new SheetPlacement(PlacementTypeEnum.Gravity, sheet, partPlacementList, 121, new TestSvgNestConfig().ClipperScale);
      var sheetPlacementsCollection = new SheetPlacementCollection();
      sheetPlacementsCollection.Add(sheetPlacement);

      var gene = new DeepNestGene(new List<Chromosome>() { new Chromosome(nfp, 0), new Chromosome(unplacedNfp, 0) });

      var nestResult = new NestResult(gene, sheetPlacementsCollection, new List<INfp>() { unplacedNfp }, sheetPlacement.PlacementType, 1234, 4321);
      var json = nestResult.ToJson();
      var actual = NestResult.FromJson(json);
      actual.Should().BeEquivalentTo(nestResult,
                                     options => options
                                                .Excluding(o => o.IsDirty)
                                                .Excluding(o => o.IsValid)
                                                );
    }
  }
}
