namespace DeepNestLib.CiTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FakeItEasy;
  using FluentAssertions;
  using IxMilia.Dxf.Entities;
  using Xunit;

  public class RunFullNestFixture
  {
    private const string DxfTestFilename = "_5.dxf";

    private static volatile object testSyncLock = new object();
    private DefaultSvgNestConfig config;
    private RawDetail loadedRawDetail;
    private NestingContext nestingContext;
    private NFP loadedNfp;
    private NFP simplifiedNfp;
    private long simplifiedNfpTime;
    private bool hasImportedRawDetail;
    private int terminateNestResultCount = 2;
    private int firstSheetIdSrc = new Random().Next();

    /// <summary>
    /// MinkowskiWrapper.CalculateNfp occasionally sticks; not sure why; seems fine at runtime only nCrunch has the problem.
    /// </summary>
    [Fact(Timeout=10000)]
    public void RunNest()
    {
      lock (testSyncLock)
      {
        if (!this.hasImportedRawDetail)
        {
          this.config = new DefaultSvgNestConfig();
          this.loadedRawDetail = DxfParser.LoadDxf(DxfTestFilename);
          this.nestingContext = new NestingContext(A.Fake<IMessageService>(), A.Fake<IProgressDisplayer>());
          this.hasImportedRawDetail = this.nestingContext.TryImportFromRawDetail(this.loadedRawDetail, A.Dummy<int>(), out this.loadedNfp);
          this.nestingContext.Polygons.Add(this.loadedNfp);

          NFP firstSheet;
          nestingContext.TryImportFromRawDetail(DxfParser.ConvertDxfToRawDetail("Sheet", new List<DxfEntity>() { new DxfGenerator().Rectangle(595D, 395D, RectangleType.FileLoad) }), firstSheetIdSrc, out firstSheet).Should().BeTrue();
          this.nestingContext.Sheets.Add(firstSheet);

          this.nestingContext.StartNest();
          while (this.nestingContext.Nest.nests.Count < terminateNestResultCount)
          {
            this.nestingContext.NestIterate(this.config);
          }
        }
      }
    }
  }
}
