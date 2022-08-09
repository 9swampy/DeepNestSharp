namespace DeepNestLib.CiTests
{
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using DeepNestLib.Placement;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class MinkowskiSumDebugFixture
  {
    private readonly MinkowskiDictionary cache;
    private readonly ISheet sheet;
    private readonly INfp part;
    private readonly INfp ret;
    private readonly INfp[] dllResult;
    private readonly INfp[] newClipperResult;

    public MinkowskiSumDebugFixture()
    {
      cache = new MinkowskiDictionary();

      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSumA.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      sheet = Sheet.FromJson(json);
      sheet.EnsureIsClosed();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSumB.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      part = NoFitPolygon.FromJson(json);
      part.EnsureIsClosed();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSumRet.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      ret = NoFitPolygon.FromJson(json);

      var minkowski = MinkowskiSum.CreateInstance(false, A.Fake<INestStateMinkowski>());

      dllResult = ((ITestNfpHelper)new NfpHelper(minkowski, A.Fake<IWindowUnk>())).ExecuteInterchangeableMinkowski(true, sheet, part);
      newClipperResult = ((ITestNfpHelper)new NfpHelper(minkowski, A.Fake<IWindowUnk>())).ExecuteInterchangeableMinkowski(false, sheet, part);
    }

    [Fact]
    public void CacheShouldHaveNoEntries()
    {
      cache.Should().BeEmpty();
    }

    [Fact]
    public void AShouldBeSheet()
    {
      sheet.Should().BeOfType<Sheet>();
    }

    [Fact]
    public void BShouldBePart()
    {
      part.Should().BeOfType<NoFitPolygon>();
    }

    [Fact]
    public void AWidthShouldBeBiggerThanB()
    {
      sheet.WidthCalculated.Should().BeGreaterThan(part.WidthCalculated);
    }

    [Fact]
    public void AHeightShouldBeBiggerThanB()
    {
      sheet.HeightCalculated.Should().BeGreaterThan(part.HeightCalculated);
    }

    [Fact]
    public void NewClipperLengthShouldBeSameAsDllImportExpected1()
    {
      newClipperResult.Length.Should().Be(dllResult.Length);
      newClipperResult.Length.Should().Be(1);
    }

    [Fact]
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldBeSameWidth()
    {
      newClipperResult[0].WidthCalculated.Should().BeApproximately(dllResult[0].WidthCalculated, 0.01);
    }

    [Fact]
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldBeSameHeight()
    {
      newClipperResult[0].WidthCalculated.Should().BeApproximately(dllResult[0].HeightCalculated, 0.01);
    }

    [Fact]
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldHaveChildren()
    {
      newClipperResult[0].Children.Count().Should().Be(dllResult[0].Children.Count());
      newClipperResult[0].Children.Count().Should().Be(1, "part fits");
    }

    [Fact]
    public void PartShouldBeClosed()
    {
      part.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void SheetShouldBeClosed()
    {
      sheet.IsClosed.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test(bool useDllImport)
    {
      var minkowski = MinkowskiSum.CreateInstance(false, A.Fake<INestStateMinkowski>());
      var config = new TestSvgNestConfig();

      INfp[] result;
      SheetNfp sheetNfp;
      var deframedSheet = new Sheet(sheet.Children[0], WithChildren.Excluded);
      deframedSheet.IsClosed.Should().BeTrue();
      var windowUnk = A.Fake<IWindowUnk>();
      A.CallTo(() => windowUnk.Find(A<DbCacheKey>._, A<bool>._)).Returns(null);
      if (useDllImport)
      {
        sheetNfp = new SheetNfp(new NfpHelper(minkowski, windowUnk), deframedSheet, part, config.ClipperScale, useDllImport);
        result = ((ITestNfpHelper)new NfpHelper(minkowski, windowUnk)).ExecuteInterchangeableMinkowski(useDllImport, sheet, part);
      }
      else
      {
        sheetNfp = new SheetNfp(new NfpHelper(minkowski, windowUnk), deframedSheet, part, config.ClipperScale, useDllImport);
        result = ((ITestNfpHelper)new NfpHelper(minkowski, windowUnk)).ExecuteInterchangeableMinkowski(useDllImport, sheet, part);
      }

      sheet.WidthCalculated.Should().BeApproximately(12.1, 0.01);
      deframedSheet.WidthCalculated.Should().BeApproximately(11, 0.01);
      sheet.HeightCalculated.Should().BeApproximately(12.1, 0.01);
      deframedSheet.HeightCalculated.Should().BeApproximately(11, 0.01);
      part.WidthCalculated.Should().Be(10);
      part.HeightCalculated.Should().Be(10);

      sheetNfp.CanAcceptPart.Should().BeTrue();

      result.Length.Should().Be(1, "part can be fit");
      if (useDllImport)
      {
        result[0].Area.Should().BeApproximately(488, 1);
        result[0].Children.Count.Should().Be(1);
        result[0].Children[0].Area.Should().BeApproximately(1, 0.0001);
        result[0].Children[0].Length.Should().Be(5);
      }
      else
      {
        result[0].Area.Should().BeApproximately(488, 1);
        result[0].Children.Count.Should().Be(1);
        result[0].Children[0].Area.Should().BeApproximately(1, 0.0001);
        result[0].Children[0].Length.Should().Be(5);
      }

      result[0].WidthCalculated.Should().BeApproximately(sheet.WidthCalculated + part.WidthCalculated, 0.001, "it's the outerNfp that we're not interested in");
      result[0].WidthCalculated.Should().BeApproximately(22.1, 0.001);
      result[0].WidthCalculated.Should().BeApproximately(ret.WidthCalculated, 0.01);
      result[0].HeightCalculated.Should().BeApproximately(sheet.HeightCalculated + part.HeightCalculated, 0.001, "it's the outerNfp that we're not interested in");
      result[0].HeightCalculated.Should().BeApproximately(22.1, 0.001);
      result[0].HeightCalculated.Should().BeApproximately(ret.HeightCalculated, 0.01);
      result[0].MinX.Should().BeApproximately(-0.55, 0.01);
      result[0].MinX.Should().BeApproximately(ret.MinX, 0.01);
      result[0].MinY.Should().BeApproximately(-10.55, 0.01);
      result[0].MinY.Should().BeApproximately(ret.MinY, 0.01);
      if (useDllImport)
      {
        result[0].Children.Count.Should().Be(ret.Children.Count);
        result[0].Children.Count.Should().Be(1, "part fits");
        result[0].Children[0].WidthCalculated.Should().BeApproximately(deframedSheet.WidthCalculated - part.WidthCalculated, 0.001);
        result[0].Children[0].HeightCalculated.Should().BeApproximately(deframedSheet.HeightCalculated - part.HeightCalculated, 0.001);
      }
      else
      {
        result[0].Children.Count.Should().Be(ret.Children.Count);
        result[0].Children.Count.Should().Be(1, "part fits");
        result[0].Children[0].WidthCalculated.Should().BeApproximately(deframedSheet.WidthCalculated - part.WidthCalculated, 0.001);
        result[0].Children[0].HeightCalculated.Should().BeApproximately(deframedSheet.HeightCalculated - part.HeightCalculated, 0.001);
      }
    }
  }
}
