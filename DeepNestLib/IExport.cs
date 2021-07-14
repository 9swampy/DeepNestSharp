namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface IExport
  {
    void Export(string path, IEnumerable<INfp> polygons, IEnumerable<INfp> sheets);

    string SaveFileDialogFilter { get; }
  }
}