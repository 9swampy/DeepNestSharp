namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib.Placement;

  public abstract class ExporterBase : IExport
  {
    public abstract string SaveFileDialogFilter { get; }

    public async Task Export(string path, ISheetPlacement sheetPlacement, bool doMergeLines, bool differentiateChildren)
    {
      await this.Export(
        path,
        sheetPlacement.PolygonsForExport,
        new ISheet[] { sheetPlacement.Sheet, },
        doMergeLines,
        differentiateChildren).ConfigureAwait(false);
    }

    protected abstract Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets, bool doMergeLines, bool differentiateChildren);
  }
}
