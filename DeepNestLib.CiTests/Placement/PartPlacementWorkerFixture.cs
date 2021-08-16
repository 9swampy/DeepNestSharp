namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using DeepNestLib.CiTests;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
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
      var sut = new PartPlacementWorker(
        A.Fake<IPlacementWorker>(),
        new DefaultSvgNestConfig(),
        parts,
        placements,
        sheet,
        new NfpHelper(A.Fake<IMinkowskiSumService>(), window));

      string json = null;
      Action act = () => json = sut.ToJson();

      act.Should().NotThrow();

      json.Should().Contain("\"Config\"");
      json.Should().Contain("\"Parts\"");
      json.Should().Contain("\"Placements\"");
      json.Should().Contain("\"Sheet\"");
      json.Should().Contain("\"NfpHelper\"");
      json.Should().Contain("\"ClipCache\"");

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Placement.PartPlacementWorker.json"))
      using (StreamReader reader = new StreamReader(stream))
      {
        string fromFile = reader.ReadToEnd().Replace(" ", string.Empty).Replace("\r\n", string.Empty);
        json.Should().Be(fromFile);
      }
    }

    [Fact]
    public void GivenPropertiesPopulatedShouldRoundTripSerialize()
    {
      var part = new NFP(new List<SvgPoint>() { new SvgPoint(1, 2) });
      var parts = new INfp[] { part };
      var placements = new List<IPartPlacement>() { new PartPlacement(part) };
      var sheet = new Sheet();
      var window = new WindowUnk();
      var sut = new PartPlacementWorker(
        A.Fake<IPlacementWorker>(),
        new SvgNestConfig(),
        parts,
        placements,
        sheet,
        new NfpHelper(A.Fake<IMinkowskiSumService>(), window));

      var json = sut.ToJson();

      PartPlacementWorker.FromJson(json).Should().BeEquivalentTo(sut);
    }
  }
}
