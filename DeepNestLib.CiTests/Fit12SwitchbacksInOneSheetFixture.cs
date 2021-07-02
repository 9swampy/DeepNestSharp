namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class Fit12SwitchbacksInOneSheetFixture
  {
    private static readonly DxfGenerator DxfGenerator = new DxfGenerator();
    private SheetPlacement sheetPlacement;
    private int firstSheetIdSrc = new Random().Next();

    private static volatile object syncLock = new object();

    public Fit12SwitchbacksInOneSheetFixture()
    {
      lock (syncLock)
      {
        var nestingContext = new NestingContext(A.Fake<IMessageService>());
        NFP firstSheet;
        nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { DxfGenerator.Rectangle(595D, 155D, RectangleType.FileLoad) }), firstSheetIdSrc, out firstSheet).Should().BeTrue();
        NFP switchback;
        var raw = DxfParser.LoadDxf("LowerSwitchBack.dxf");
        nestingContext.TryImportFromRawDetail(raw, -1, out switchback).Should().BeTrue();

        var config = new DefaultSvgNestConfig();
        config.CurveTolerance = 0.4;
        switchback = SvgNest.simplifyFunction(switchback, false, config);
        var switchback270 = switchback.Clone();
        switchback270.Rotation = 270;
        var switchback90 = switchback.Clone();
        switchback90.Rotation = 90;

        var parts = new List<NFP>(12);
        

        for (int p = 0; p < 12; p++)
        {
          NFP clone;
          switch (p)
          {
            case 0:
            case 2:
            case 4:
            case 5:
            case 8:
            case 9:
              clone = switchback270.Clone();
              break;

            case 1:
            case 3:
            case 6:
            case 7:
            case 10:
            case 11:
            default:
              clone = switchback90.Clone();
              break;
          }

          clone.Name = p.ToString();
          clone.Source = p;
          parts.Add(clone);
        }

        this.sheetPlacement = new Background().PlaceParts(new NFP[] { firstSheet }, parts.ToArray(), config, 0);
      }
    }

    [Fact]
    public void ShouldHaveReturnedASheetPlacement()
    {
      this.sheetPlacement.Should().NotBeNull();
    }

    //  [Fact]
    //  public void GivenOnePartOnlyThenShouldBeNoMergedLines()
    //  {
    //    this.sheetPlacement.mergedLength.Should().Be(0, "there was only one part on each sheet; no lines to merge possible.");
    //  }

    //  [Fact]
    //  public void ShouldHaveExpectedFitness()
    //  {
    //    this.sheetPlacement.fitness.Should().BeApproximately(617.04158790170129, 10);
    //  }

    [Fact]
    public void ShouldHaveExpectedNullRotation()
    {
      this.sheetPlacement.Rotation.Should().BeNull();
    }

    [Fact]
    public void ShouldHaveExpectedArea()
    {
      this.sheetPlacement.area.Should().Be(92225);
    }

    [Fact]
    public void ShouldHaveOnePlacement()
    {
      this.sheetPlacement.placements.Length.Should().Be(1);
    }

    [Fact]
    public void ShouldHaveOneNestResultWithOneSheet()
    {
      this.sheetPlacement.placements[0].Count.Should().Be(1);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedId()
    {
      this.sheetPlacement.placements[0][0].sheetId.Should().Be(0);
    }

    [Fact]
    public void FirstSheetShouldHaveExpectedSource()
    {
      this.sheetPlacement.placements[0][0].sheetSource.Should().Be(firstSheetIdSrc, "this is the first sheet in the single nest result");
    }

    [Fact]
    public void ShouldHaveNoPlacementsOnFirstSheet()
    {
      this.sheetPlacement.placements[0][0].placements.Should().BeEmpty("not sure why there would be placements on the sheet; inners maybe?");
    }

    [Fact]
    public void ShouldHaveFirstSheet()
    {
      this.sheetPlacement.placements[0][0].sheetplacements.Count.Should().Be(12, "they all fit on one sheet");
    }

    //  [Fact]
    //  public void ShouldHaveOnePartOnFirstPlacementWithExpectedX()
    //  {
    //    this.sheetPlacement.placements[0][0].sheetplacements[0].x.Should().Be(10.999999933643268, "bottom left");
    //  }

    //  [Fact]
    //  public void ShouldHaveOnePartOnFirstPlacementWithExpectedY()
    //  {
    //    this.sheetPlacement.placements[0][0].sheetplacements[0].y.Should().Be(10.999999933643265, "bottom left");
    //  }

    //  [Fact]
    //  public void ShouldHaveOnePartOnFirstPlacementWithExpectedRotation()
    //  {
    //    this.sheetPlacement.placements[0][0].sheetplacements[0].rotation.Should().Be(180);
    //  }
  }
}

