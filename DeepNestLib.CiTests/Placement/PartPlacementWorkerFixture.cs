namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using System.Text.Json;
  using DeepNestLib.CiTests;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Microsoft.CodeAnalysis;
  using Xunit;

  public class PartPlacementWorkerFixture
  {
    [Fact]
    public void DummyCtorShouldNotThrow()
    {
      Action act = () => _ = A.Dummy<PartPlacementWorker>();

      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldSerialize()
    {
      var sut = A.Dummy<PartPlacementWorker>();

      Action act = () => _ = sut.ToJson();

      act.Should().NotThrow();
    }

    [Fact]
    public void GivenPropertiesPopulatedWhenSerializedThenShouldNotThrow()
    {
      var part = new NFP(new List<SvgPoint>() { new SvgPoint(1, 2) });
      var parts = new INfp[] { part };
      var placements = new List<IPartPlacement>() { new PartPlacement(part) };
      var sheet = new Sheet();
      var window = new WindowUnk();
      var clipCache = new Dictionary<string, ClipCacheItem>();
      clipCache.Add("item1", new ClipCacheItem() { index = 1, nfpp = new ClipperLib.IntPoint[][] { new ClipperLib.IntPoint[] { new ClipperLib.IntPoint(0, 0), new ClipperLib.IntPoint(1, 1) } } });
      var sut = new PartPlacementWorker(
        A.Fake<IPlacementWorker>(),
        new SvgNestConfig(),
        parts,
        placements,
        sheet,
        new NfpHelper(MinkowskiSum.CreateInstance(A.Fake<INestStateMinkowski>()), window),
        clipCache,
        A.Fake<INestState>());

      string json = null;
      Action act = () => json = sut.ToJson();

      act.Should().NotThrow();

      json.Should().Contain("\"Config\"");
      json.Should().Contain("\"Parts\"");
      json.Should().Contain("\"Placements\"");
      json.Should().Contain("\"Sheet\"");
      json.Should().Contain("\"NfpHelper\"");
      json.Should().Contain("\"ClipCache\"");

      if (json == "disabled as brittle; keep for debugging")
      {
        using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementWorker.json"))
        using (StreamReader reader = new StreamReader(stream))
        {
          string fromFile = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
          json.Should().Be(fromFile);
        }
      }
    }

    [Fact]
    public void GivenClipCacheItemJsonConverterWithClipCacheShouldRoundTripSerialize()
    {
      var clipCache = new Dictionary<string, ClipCacheItem>();
      clipCache.Add("item1", new ClipCacheItem() { index = 1, nfpp = new ClipperLib.IntPoint[][] { new ClipperLib.IntPoint[] { new ClipperLib.IntPoint(0, 0), new ClipperLib.IntPoint(1, 1) } } });

      var sut = new ClipCacheItemJsonConverter();

      var options = new JsonSerializerOptions();
      options.Converters.Add(new ClipCacheItemJsonConverter());
      string json = System.Text.Json.JsonSerializer.Serialize<Dictionary<string, ClipCacheItem>>(clipCache, options);
      System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, ClipCacheItem>>(json, options);
    }

    [Fact]
    public void GivenPropertiesPopulatedShouldRoundTripSerialize()
    {
      var part = new NFP(new List<SvgPoint>() { new SvgPoint(1, 2) });
      var parts = new INfp[] { part };
      var placements = new List<IPartPlacement>() { new PartPlacement(part) };
      var sheet = new Sheet();
      var window = new WindowUnk();
      var clipCache = new Dictionary<string, ClipCacheItem>();
      clipCache.Add("item1", new ClipCacheItem() { index = 1, nfpp = new ClipperLib.IntPoint[][] { new ClipperLib.IntPoint[] { new ClipperLib.IntPoint(0, 0), new ClipperLib.IntPoint(1, 1) } } });

      var minkowskiSum = (MinkowskiSum)MinkowskiSum.CreateInstance(A.Fake<INestStateMinkowski>());
      minkowskiSum.MinkowskiCache.Add(new MinkowskiKey(1, new double[] { 1, 2 }, 2, new int[] { 3, 4 }, new double[] { 1, 2 }, 5, new double[] { 6, 8 }), part);
      var sut = new PartPlacementWorker(
        A.Fake<IPlacementWorker>(),
        new SvgNestConfig(),
        parts,
        placements,
        sheet,
        new NfpHelper(minkowskiSum, window),
        clipCache,
        A.Fake<INestState>());

      var json = sut.ToJson();

      PartPlacementWorker.FromJson(json).Should().BeEquivalentTo(sut);
    }
  }
}
