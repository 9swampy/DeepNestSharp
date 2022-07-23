namespace DeepNestLib.CiTests.Placement
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;
#if NCRUNCH
  using System.Text;
#endif

  public class SheetNfpFixture
  {
    [Fact]
    public void ShouldSerialize()
    {
      var sut = new SheetNfp(A.Fake<INfpHelper>(), new Sheet(), new NoFitPolygon(), A.Dummy<int>(), A.Dummy<bool>());
      Action act = () => sut.ToJson();
      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var nfpHelper = A.Fake<INfpHelper>();
      var sut = new SheetNfp(nfpHelper, new Sheet(), new NoFitPolygon(), A.Dummy<int>(), A.Dummy<bool>());
      var json = sut.ToJson();
      SheetNfp.FromJson(json).Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GivenPopulatedShouldStillRoundTripSerialize()
    {
      var sheet = new Sheet();
      var part = new NoFitPolygon(new SvgPoint[] { new SvgPoint(1, 1) });
      var sut = new SheetNfp(new List<INfp>().ToArray(), sheet, part);
      var json = sut.ToJson();
      SheetNfp actual = SheetNfp.FromJson(json);
      actual.Part.Should().BeEquivalentTo(part);
      actual.Sheet.Should().BeEquivalentTo(sheet);
      actual.Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GivenPopulatedNfpCandidateListShouldStillRoundTripSerialize()
    {
      var sheet = new Sheet();
      var part = new NoFitPolygon(new SvgPoint[] { new SvgPoint(1, 1) });
      var nfpHelper = A.Fake<INfpHelper>();
      A.CallTo(() => nfpHelper.GetInnerNfp(A<ISheet>._, A<INfp>._, A<MinkowskiCache>._, A<double>._, A<bool>.Ignored, A<Action<string>>._)).Returns(new INfp[] { part });
      var sut = new NfpCandidateList(nfpHelper, sheet, part, 1, true);
      var json = sut.ToJson();
      NfpCandidateList actual = NfpCandidateList.FromJson(json);
      actual.Should().BeEquivalentTo(sut);
      actual.Part.Should().BeEquivalentTo(part);
      actual.Sheet.Should().BeEquivalentTo(sheet);
      actual.Items.Length.Should().Be(1);
    }
  }
}