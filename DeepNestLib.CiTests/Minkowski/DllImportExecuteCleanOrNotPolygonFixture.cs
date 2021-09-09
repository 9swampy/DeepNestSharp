namespace DeepNestLib.CiTests
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class DllImportExecuteCleanOrNotPolygonFixture
  {
    public enum MinkowskiExecutor
    {
      DllImportCleaned,
      DllImportUncleaned,
      NewMinkowskiSum,
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum)]
    public void ShouldExecuteWithoutError(MinkowskiExecutor executor)
    {
      var sheet = InitSheet();
      var part = InitPart();
      Action act = () => _ = GetNfp(executor, sheet, part);

      act.Should().NotThrow();
    }

    [Fact]
    public void LoadedSheetIsActuallyAFramedSheet()
    {
      var frame = InitSheet();
      var sheet = new Sheet(frame.Children[0], WithChildren.Excluded);

      new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included).Should().BeEquivalentTo(
        frame,
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 0.001))
                                    .WhenTypeIs<double>());
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum)]
    public void GivenEmptySheetShouldReturnSingleNfp(MinkowskiExecutor executor)
    {
      var sheet = InitSheet();
      var part = InitPart();
      var actual = GetNfp(executor, sheet, part);

      actual.Length.Should().Be(1);
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned, true)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, true)]
    [InlineData(MinkowskiExecutor.DllImportCleaned, false)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, false)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, false)]
    public void GivenEmptySheetShouldReturnSingleNfpWithExpectedHeight(MinkowskiExecutor executor, bool useSimplePart)
    {
      INfp part;
      ISheet sheet;
      ISheet frame;
      if (useSimplePart)
      {
        var generator = new DxfGenerator();
        sheet = generator.GenerateSquare("Sheet", 20, RectangleType.FileLoad).ToSheet();
        part = generator.GenerateSquare("Part", 10, RectangleType.FileLoad).ToNfp();
        frame = new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included);
      }
      else
      {
        frame = InitSheet();
        part = InitPart();
      }

      var actual = GetNfp(executor, frame, part);

      actual[0].HeightCalculated.Should().BeApproximately(frame.HeightCalculated + part.HeightCalculated, 0.01, "this is an outer Nfp but the frame makes it useless so it's discarded? It's the child we actually want. tbc...");
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum)]
    public void GivenEmptySheetShouldReturnSingleNfpWithExpectedWidth(MinkowskiExecutor executor)
    {
      //var sheet = InitSheet();
      //var part = InitPart();
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("Sheet", 20, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateSquare("Part", 10, RectangleType.FileLoad).ToNfp();

      var actual = GetNfp(executor, sheet, part);

      actual[0].WidthCalculated.Should().BeApproximately(sheet.WidthCalculated + part.WidthCalculated, 0.01);
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum)]
    public void GivenEmptySheetShouldReturnSingleNfpWithAChildOfExpectedHeight(MinkowskiExecutor executor)
    {
      //var sheet = InitSheet();
      //var part = InitPart();
      var generator = new DxfGenerator();
      var sheet = generator.GenerateSquare("Sheet", 20, RectangleType.FileLoad).ToSheet();
      var frame = new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included);
      var part = generator.GenerateSquare("Part", 10, RectangleType.FileLoad).ToNfp();

      var actual = GetNfp(executor, frame, part);

      actual[0].Children[0].HeightCalculated.Should().BeApproximately(sheet.HeightCalculated - part.HeightCalculated, 0.01);
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned, true, true, true)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, true, true, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, true, true, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, true, true, false)]
    [InlineData(MinkowskiExecutor.DllImportCleaned, false, true, true)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, false, true, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, false, true, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, false, true, false)]
    [InlineData(MinkowskiExecutor.DllImportCleaned, true, false, true)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, true, false, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, true, false, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, true, false, false)]
    [InlineData(MinkowskiExecutor.DllImportCleaned, false, false, true)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, false, false, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, false, false, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, false, false, false)]
    public void GivenEmptySheetReturn1NfpWith1Child(MinkowskiExecutor e, bool generatePart, bool generateSheet, bool manipulateSheetToMatchLoaded)
    {
      var generator = new DxfGenerator();
      var minSheetSize = 159.340005;
      var expectedSheetLength = 5;
      INfp part;
      ISheet sheet;
      ISheet frame;
      if (generatePart)
      {
        part = generator.GenerateSquare("Part", 10, RectangleType.FileLoad).ToNfp();
      }
      else
      {
        part = InitPart();
      }

      if (generateSheet)
      {
        if (manipulateSheetToMatchLoaded)
        {
          sheet = new Sheet(SvgNest.CleanPolygon2(generator.GenerateRectangle("Sheet", 160, 196, RectangleType.TopLeftClockwise, false).ToSheet()), WithChildren.Excluded);
          sheet.Length.Should().Be(expectedSheetLength);
          sheet.WidthCalculated.Should().BeApproximately(160, 0.01);
          sheet.HeightCalculated.Should().BeApproximately(196, 0.01);
        }
        else
        {
          sheet = generator.GenerateRectangle("Sheet", minSheetSize, minSheetSize, RectangleType.FileLoad).ToSheet();
          sheet = generator.GenerateRectangle("Sheet", minSheetSize, minSheetSize, RectangleType.TopLeftClockwise, true).ToSheet();
          sheet = generator.GenerateRectangle("Sheet", minSheetSize, minSheetSize, RectangleType.TopLeftClockwise, false).ToSheet();
          sheet.Length.Should().Be(expectedSheetLength);
          // All above work with 5 points and Cleaning breaks them all...
          var cleaned = new Sheet(SvgNest.CleanPolygon2(sheet), WithChildren.Excluded);
          sheet = cleaned;
          //sheet.Length.Should().Be(expectedSheetLength);
          sheet.WidthCalculated.Should().BeApproximately(minSheetSize, 0.01);
          sheet.HeightCalculated.Should().BeApproximately(minSheetSize, 0.01);
          //sheet = generator.GenerateSquare("Sheet", 20, RectangleType.FileLoad).ToSheet();
        }

        frame = new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included);
      }
      else
      {
        frame = InitSheet();
        sheet = new Sheet(frame.Children[0], WithChildren.Excluded);
        //sheet.WidthCalculated.Should().Be(160);
        //sheet.HeightCalculated.Should().Be(196);
        //sheet = new Sheet(sheet.ShiftToOrigin(), WithChildren.Excluded);
        //frame = new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included);
        frame.WidthCalculated.Should().BeGreaterOrEqualTo(minSheetSize);
        frame.HeightCalculated.Should().BeGreaterOrEqualTo(minSheetSize);
      }

      //sheet.Length.Should().Be(expectedSheetLength);

      var actual = GetNfp(e, frame, part);

      actual[0].Children.Count().Should().Be(1);
    }

    [Theory]
    [InlineData(MinkowskiExecutor.DllImportCleaned, true)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, true)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, true)]
    [InlineData(MinkowskiExecutor.DllImportCleaned, false)]
    [InlineData(MinkowskiExecutor.DllImportUncleaned, false)]
    [InlineData(MinkowskiExecutor.NewMinkowskiSum, false)]
    public void GivenEmptySheetShouldReturnSingleNfpWithAChildOfExpectedWidth(MinkowskiExecutor executor, bool useSimplePart)
    {
      var generator = new DxfGenerator();
      INfp part;
      ISheet sheet;
      ISheet frame;
      if (useSimplePart)
      {
        sheet = generator.GenerateSquare("Sheet", 20, RectangleType.FileLoad).ToSheet();
        part = generator.GenerateSquare("Part", 10, RectangleType.FileLoad).ToNfp();
        frame = new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included);
      }
      else
      {
        sheet = generator.GenerateSquare("Sheet", 159.340005, RectangleType.FileLoad).ToSheet();
        frame = new Sheet(NfpHelper.GetExpandedFrame(sheet), WithChildren.Included);
        //frame = InitSheet();
        //sheet = new Sheet(frame.Children[0], WithChildren.Excluded);
        part = InitPart();
      }

      var actual = GetNfp(executor, frame, part);

      actual[0].Children[0].WidthCalculated.Should().BeApproximately(sheet.WidthCalculated - part.WidthCalculated, 0.01);
    }

    private static INfp[] GetNfp(MinkowskiExecutor executor, ISheet path, INfp pattern)
    {
      var minkowski = MinkowskiSum.CreateInstance(false, A.Fake<INestStateMinkowski>());
      switch (executor)
      {
        case MinkowskiExecutor.DllImportCleaned:
          return minkowski.DllImportExecute(path, pattern, MinkowskiSumCleaning.Cleaned);
        case MinkowskiExecutor.DllImportUncleaned:
          return minkowski.DllImportExecute(path, pattern, MinkowskiSumCleaning.None);
        case MinkowskiExecutor.NewMinkowskiSum:
          return minkowski.NewMinkowskiSum(pattern.Points, path, WithChildren.Included, false);
      }

      throw new ArgumentException();
    }

    private static INfp InitPart()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2B.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      return NoFitPolygon.FromJson(json);
    }

    private static Sheet InitSheet()
    {
      string json;
      using (Stream stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream("Minkowski.MinkowskiSum2A.dnpoly"))
      using (StreamReader reader = new StreamReader(stream))
      {
        json = reader.ReadToEnd();
      }

      return Sheet.FromJson(json);
    }
  }
}
