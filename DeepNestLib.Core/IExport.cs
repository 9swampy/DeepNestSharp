namespace DeepNestLib
{
  using System.Threading.Tasks;
  using DeepNestLib.Placement;

  public interface IExport
  {
    string SaveFileDialogFilter { get; }

    Task Export(string path, ISheetPlacement sheetPlacement, bool doMergeLines, bool differentiateChildren);
  }
}