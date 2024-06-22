namespace DeepNestLib.IO
{
  using System.Collections.Generic;
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
        new ISheet[] { sheetPlacement.OriginalSheet, },
        doMergeLines,
        differentiateChildren).ConfigureAwait(false);
    }

    protected abstract Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets, bool doMergeLines, bool differentiateChildren);
  }
}
