namespace DeepNestLib
{
  using DeepNestLib.Placement;
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using System.Xml.Linq;

  public abstract class ParserBase : IExport
  {
    public abstract string SaveFileDialogFilter { get; }

    public async Task Export(string path, ISheetPlacement sheetPlacement)
    {
      await this.Export(path, sheetPlacement.PartPlacements.Select(
        o =>
        {
          var result = new NFP(o.Part, WithChildren.Included);
          result.Sheet = sheetPlacement.Sheet;
          result.X = o.X;
          result.Y = o.Y;
          return result;
        }),
        new ISheet[] {
          sheetPlacement.Sheet,
        }).ConfigureAwait(false);
    }

    public abstract Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets);
  }
}
