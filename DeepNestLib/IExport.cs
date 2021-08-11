namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using DeepNestLib.Placement;

  public interface IExport
  {
    Task Export(string path, ISheetPlacement sheetPlacement);

    Task Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets);

    string SaveFileDialogFilter { get; }
  }
}