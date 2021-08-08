namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.Placement;

  public interface IExport
  {
    void Export(string path, ISheetPlacement sheetPlacement);

    void Export(string path, IEnumerable<INfp> polygons, IEnumerable<ISheet> sheets);

    string SaveFileDialogFilter { get; }
  }
}