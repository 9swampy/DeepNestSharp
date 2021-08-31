namespace DeepNestLib.CiTests
{
  using System.Linq;
  using FakeItEasy;
  using FluentAssertions;
  using Xunit;

  public class CanBePlacedFixture
  {
    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenSmallerSquareWhenFitInLargeSquareThenCanBePlaced(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.CanAcceptPart.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenSmallerSquareWhenFitInLargeSquareThenItemsCandidatesShouldNotBeEmpty(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.Items.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenSmallerSquareWhenFitInLargeSquareThenItemsShouldHaveSingleNfpCandidate(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.Items.Count().Should().Be(1);
    }

    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenSmallerSquareWhenFitInLargeSquareThenItemsShouldHaveSingleNfpCandidateWithNoChildren(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.Items[0].Children.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenSmallerSquareWhenFitInLargeSquareThenItemsShouldHaveSingleNfpCandidateWithFourVertices(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      SvgNest.CleanPolygon2(sheetNfp.Items[0]).Length.Should().Be(5, "it's closed, hence 5 vertices for a rectangle");
      sheetNfp.Items[0].IsClosed.Should().BeTrue();
    }

    [Theory]
    //[InlineData(101, 1)]
    //[InlineData(1.01, 1)]
    [InlineData(1, 1)]
    [InlineData(1, 1.01)]
    [InlineData(1, 101)]
    public void GivenSubstitutableWhenGetInnerNfpThenBothShouldBeEquivalent(double sheetSize, double partSize)
    {
      SheetNfp sheetNfpDllImport = GivenPartWhenFitOnSheetThenGetSheetNfp(true, sheetSize, partSize);
      SheetNfp sheetNfpNewClipper = GivenPartWhenFitOnSheetThenGetSheetNfp(false, sheetSize, partSize);

      sheetNfpNewClipper.Should().BeEquivalentTo(
        sheetNfpDllImport,
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                          .WhenTypeIs<double>());
    }

    [Theory]
    [InlineData(101, 1)]
    public void GivenSubstitutableWhenGetInnerNfpThenBothShouldBeEquivalentButDllImportNeedsCleaned(double sheetSize, double partSize)
    {
      SheetNfp sheetNfpDllImport = GivenPartWhenFitOnSheetThenGetSheetNfp(true, sheetSize, partSize);
      sheetNfpDllImport.CanAcceptPart.Should().BeTrue();
      sheetNfpDllImport.NumberOfNfps.Should().Be(1, "we're placing a single part on an empty sheet.");
      SheetNfp sheetNfpNewClipper = GivenPartWhenFitOnSheetThenGetSheetNfp(false, sheetSize, partSize);
      sheetNfpNewClipper.CanAcceptPart.Should().BeTrue();
      sheetNfpNewClipper.NumberOfNfps.Should().Be(1, "we're placing a single part on an empty sheet.");
      sheetNfpNewClipper.Part.Should().BeEquivalentTo(sheetNfpDllImport.Part);
      sheetNfpNewClipper.Sheet.Should().BeEquivalentTo(sheetNfpDllImport.Sheet);
      sheetNfpNewClipper.Items[0].Should().BeEquivalentTo(SvgNest.CleanPolygon2(sheetNfpDllImport.Items[0]));

      sheetNfpNewClipper[0].Should().BeEquivalentTo(
        SvgNest.CleanPolygon2(sheetNfpDllImport[0]),
        options => options.Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, 1))
                          .WhenTypeIs<double>());
    }

    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenPartWhenSheetNfpMemoisesShouldBeEquivalentOnly(bool useDllImport, double sheetSize, double partSize)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateRectangle("Sheet", sheetSize, sheetSize, RectangleType.FileLoad).ToSheet();
      var part = generator.GenerateRectangle("Sheet", partSize, partSize, RectangleType.FileLoad).ToNfp();

      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheet, part);

      sheetNfp.Part.Should().NotBe(part);
      sheetNfp.Part.Should().BeEquivalentTo(part);
    }

    [Theory]
    [InlineData(true, 1.01, 1)]
    [InlineData(false, 1.01, 1)]
    public void GivenSheetWhenSheetNfpMemoisesShouldBeEquivalentOnly(bool useDllImport, double sheetSize, double partSize)
    {
      var generator = new DxfGenerator();
      var sheet = generator.GenerateRectangle("Sheet", sheetSize, sheetSize, RectangleType.FileLoad).ToSheet();
      sheet.EnsureIsClosed();
      var part = generator.GenerateRectangle("Sheet", partSize, partSize, RectangleType.FileLoad).ToNfp();
      sheet.EnsureIsClosed();

      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheet, part);

      sheetNfp.Sheet.Should().NotBe(sheet);
      sheetNfp.Sheet.Should().BeEquivalentTo(sheet);
    }

    [Theory]
    [InlineData(true, 1, 1.01)]
    [InlineData(false, 1, 1.01)]
    public void GivenLargerSquareWhenFitInSmallSquareThenCanNotBePlaced(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.CanAcceptPart.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, 1, 1.01)]
    [InlineData(false, 1, 1.01)]
    public void GivenLargerSquareWhenFitInSmallSquareThenItemsCandidatesShouldBeEmpty(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true, 1, 1)]
    [InlineData(false, 1, 1)]
    [InlineData(true, 10, 10)]
    [InlineData(false, 10, 10)]
    public void GivenIdenticalSquaresWhenFitThenCanNotBePlaced(bool useDllImport, double sheetSize, double partSize)
    {
      SheetNfp sheetNfp = GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheetSize, partSize);

      sheetNfp.CanAcceptPart.Should().BeFalse();
    }

    private static SheetNfp GivenPartWhenFitOnSheetThenGetSheetNfp(bool useDllImport, double sheetSize, double partSize)
    {
      var generator = new DxfGenerator();
      //var sheet = generator.GenerateRectangle("Sheet", sheetSize, sheetSize, RectangleType.FileLoad).ToSheet();
      var sheet = generator.GenerateRectangle("Sheet", sheetSize, sheetSize, RectangleType.TopLeftClockwise, false).ToSheet();
      //sheet = new Sheet(SvgNest.CleanPolygon2(sheet), WithChildren.Excluded);
      //sheet.Length.Should().Be(4);
      var part = generator.GenerateRectangle("Part", partSize, partSize, RectangleType.FileLoad).ToNfp();
      //part = SvgNest.CleanPolygon2(part);
      //part.Length.Should().Be(4);
      return GivenPartWhenFitOnSheetThenGetSheetNfp(useDllImport, sheet, part);
    }

    private static SheetNfp GivenPartWhenFitOnSheetThenGetSheetNfp(bool useDllImport, ISheet sheet, INfp part)
    {
      var nfpHelper = new NfpHelper();
      ((ITestNfpHelper)nfpHelper).MinkowskiSumService = MinkowskiSum.CreateInstance(A.Fake<ISvgNestConfig>(), A.Fake<INestStateMinkowski>());
      ((ITestNfpHelper)nfpHelper).UseDllImport = useDllImport;
      var sheetNfp = new SheetNfp(
        nfpHelper,
        sheet,
        part,
        100000);
      return sheetNfp;
    }
  }
}
