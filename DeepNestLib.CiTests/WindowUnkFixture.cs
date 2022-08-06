namespace DeepNestLib.CiTests
{
  using System;
  using DeepNestLib.CiTests.IO;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class WindowUnkFixture
  {
    [Fact]
    public void ShouldCtor()
    {
      Action act = () => _ = new WindowUnk();
      act.Should().NotThrow();
    }

    [Fact]
    public void ShouldRoundTripSerialize()
    {
      var sut = new WindowUnk();
      var json = sut.ToJson();
      var actual = WindowUnk.FromJson(json);
      actual.Should().BeEquivalentTo(sut);
    }

    [Fact]
    public void GenerateAJsonViaNfpHelper()
    {
      var sut = new WindowUnk();
      var config = new TestSvgNestConfig();
      config.UseDllImport = true;
      var nfpHelper = new NfpHelper(MinkowskiSum.CreateInstance(config, A.Fake<INestStateMinkowski>()), sut);

      ISheet firstSheet;
      new DxfGenerator().GenerateRectangle("Sheet", 180D, 88D, RectangleType.FileLoad).TryConvertToSheet(1, out firstSheet).Should().BeTrue();
      firstSheet.Id = 0;
      INfp firstPart = DxfParser.LoadDxfFileStreamAsNfp("Dxfs._2.dxf");
      var sheetNfp = new SheetNfp(nfpHelper, firstSheet, firstPart, config.ClipperScale, config.UseDllImport, o => { });

      var json = sut.ToJson();
    }
  }
}
