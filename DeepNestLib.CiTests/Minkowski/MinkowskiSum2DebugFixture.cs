namespace DeepNestLib.CiTests
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class MinkowskiSum2DebugFixture
  {
    private readonly MinkowskiDictionary cache;
    private readonly ISheet sheet;
    private readonly INfp part;
    private readonly INfp ret;
    private readonly INfp[] dllResult;
    private readonly INfp[] newClipperResult;

    public MinkowskiSum2DebugFixture()
    {
      cache = new MinkowskiDictionary();

      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2A.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      sheet = Sheet.FromJson(json);
      //sheet.EnsureIsClosed();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2B.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      part = NFP.FromJson(json);
      //part.EnsureIsClosed();

      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2Ret.dnpoly"))
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
    public void NewClipperLengthShouldBeSameAsDllImport()
    {
      newClipperResult.Length.Should().Be(dllResult.Length);
    }

    [Fact]
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldBeSameWidth()
    {
      newClipperResult[0].WidthCalculated.Should().BeApproximately(dllResult[0].WidthCalculated, 0.01);
    }

    [Fact]
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldBeSameHeight()
    {
      newClipperResult[0].HeightCalculated.Should().BeApproximately(dllResult[0].HeightCalculated, 0.01);
    }

    [Fact]
    public void GivenSameNestWhenSingleNfpReturnedThenBothShouldHaveNoChildren()
    {
      //Revisit after validate others
      newClipperResult[0].Children.Count().Should().Be(dllResult[0].Children.Count());
    }

    [Fact]
    public void NewClipperShouldReturnSameAsDllImport()
    {
      //Revisit after validate others
      newClipperResult.Should().BeEquivalentTo(
                  dllResult,
                  options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
                                    .WhenTypeIs<double>());
    }
  }
}
