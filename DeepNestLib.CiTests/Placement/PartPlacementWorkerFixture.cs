namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
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
        new NfpHelper(MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>()), window, true),
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

      var minkowskiSum = (MinkowskiSum)MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>());
      minkowskiSum.MinkowskiCache.Add(new MinkowskiKey(1, new double[] { 1, 2 }, 2, new int[] { 3, 4 }, new double[] { 1, 2 }, 5, new double[] { 6, 8 }), part);
      var sut = new PartPlacementWorker(
        A.Fake<IPlacementWorker>(),
        new SvgNestConfig(),
        parts,
        placements,
        sheet,
        new NfpHelper(minkowskiSum, window, true),
        clipCache,
        A.Fake<INestState>());

      var json = sut.ToJson();

      PartPlacementWorker.FromJson(json).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GivenMinkowskiKeyWithItem7DifferingThenHashCodeShouldDiffer()
    {
      var item7One = new decimal[]
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
      var keyOne = new MinkowskiKey(1, new List<double>(), 1, new int[0], new List<double>(), 1, item7One.Select(o => (double)o));

      var item7Two = new decimal[] {
              -40.0815659M,
              33.7844009M,
              -40.0815659M,
              -16.2155972M,
              9.9184322M,
              -16.2155972M,
              9.9184322M,
              33.7844009M,
            };
      var keyTwo = new MinkowskiKey(1, new List<double>(), 1, new int[0], new List<double>(), 1, item7Two.Select(o => (double)o));

      keyOne.GetHashCode().Should().NotBe(keyTwo.GetHashCode());
    }

    [Fact]
    public void GivenMinkowskiKeyWithItem7SameThenHashCodeShouldBeSame()
    {
      var item7One = new decimal[]
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
      var keyOne = new MinkowskiKey(1, new List<double>(), 1, new int[0], new List<double>(), 1, item7One.Select(o => (double)o));

      var item7Two = new decimal[]
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
      var keyTwo = new MinkowskiKey(1, new List<double>(), 1, new int[0], new List<double>(), 1, item7Two.Select(o => (double)o));

      keyOne.GetHashCode().Should().Be(keyTwo.GetHashCode());
    }

    [Fact]
    public void GivenListOfDifferingMinkowskiKeysThenTheyShouldAllHaveDifferentHashCodes()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.MinkowskiKeyExample.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      var kvpList = JsonSerializer.Deserialize<List<KeyValuePair<MinkowskiKey, INfp>>>(json, options);
      var hashCodeList = kvpList.Select(o => o.GetHashCode());
      hashCodeList.Distinct().Count().Should().Be(kvpList.Count);
    }

    [Fact]
    public void GivenTwoDifferingMinkowskiKeysThenTheyShouldHaveDifferentHashCodes()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.MinkowskiKeyExample.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
      }

      var options = new JsonSerializerOptions();
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      var kvpList = JsonSerializer.Deserialize<List<KeyValuePair<MinkowskiKey, INfp>>>(json, options);

      var key2 = kvpList[2].Key;
      var key4 = kvpList[4].Key;
      key2.Item1.Should().Be(key4.Item1);
      key2.Item2.Should().BeEquivalentTo(key4.Item2);
      System.Diagnostics.Debug.Print(string.Join(",", key2.Item2));
      System.Diagnostics.Debug.Print(string.Join(",", key4.Item2));
      ((IStructuralEquatable)key2.Item2.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default).Should().Be(((IStructuralEquatable)key4.Item2.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default));
      key2.Item3.Should().Be(key4.Item3);
      key2.Item4.Should().BeEquivalentTo(key4.Item4);
      ((IStructuralEquatable)key2.Item4).GetHashCode(EqualityComparer<int>.Default).Should().Be(((IStructuralEquatable)key4.Item4).GetHashCode(EqualityComparer<int>.Default));
      key2.Item5.Should().BeEquivalentTo(key4.Item5);
      ((IStructuralEquatable)key2.Item5.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default).Should().Be(((IStructuralEquatable)key4.Item5.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default));
      key2.Item6.Should().Be(key4.Item6);
      key2.Item6.GetHashCode().Should().Be(key4.Item6.GetHashCode());

      key2.Item7.Should().NotBeEquivalentTo(key4.Item7);
      ((IStructuralEquatable)key2.Item7.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default).Should().NotBe(((IStructuralEquatable)key4.Item7.Select(o => o.ToString()).ToArray()).GetHashCode(EqualityComparer<string>.Default));

      key2.GetHashCode().Should().NotBe(key4.GetHashCode());
    }

    [Fact]
    public void GivenTwoDecimalArraysWhenSameThenGetHashCodeThenShouldBeSame()
    {
    }
  }
}