namespace DeepNestLib.CiTests
{
  using System.IO;
  using System.Linq;
  using System.Reflection;
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

      part = NFP.FromJson(json);
      part.EnsureIsClosed();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSumRet.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      ret = NFP.FromJson(json);

      var minkowski = MinkowskiSum.CreateInstance(false, A.Fake<INestStateMinkowski>());
      dllResult = minkowski.DllImportExecute(sheet, part, MinkowskiSumCleaning.None);
      newClipperResult = minkowski.NewMinkowskiSum(part, sheet, WithChildren.Included, true);
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
      part.Should().BeOfType<NFP>();
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
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldHaveNoChildren()
    {
      newClipperResult[0].Children.Count().Should().Be(dllResult[0].Children.Count());
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

    [Fact]
    public void NewClipperShouldReturnSameAsDllImport()
    {
      newClipperResult.Should().NotBeEquivalentTo(
                  dllResult,
                  options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
                                    .WhenTypeIs<double>(),
                  "they'll only be the same when cleaned");
    }

    [Fact]
    public void NewClipperShouldReturnSameAsDllImportCleaned()
    {
      var cleanedDllResult = SvgNest.CleanPolygon2(dllResult[0]);
      //Revisit after validate others; dllResult is closed, clipper isn't.
      newClipperResult[0].Should().BeEquivalentTo(
                  cleanedDllResult,
                  options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
                                    .WhenTypeIs<double>());
    }
  }
}
