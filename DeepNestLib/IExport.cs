namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface IExport
  {
    void Export(string path, IEnumerable<NFP> polygons, IEnumerable<NFP> sheets);

    string SaveFileDialogFilter { get; }
  }
}