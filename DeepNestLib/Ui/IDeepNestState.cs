namespace DeepNestLib.Ui
{
  using System.Collections.Generic;

  public interface IDeepNestState
  {
    TopNestResultsCollection NestResults { get; }

    List<DetailLoadInfo> PartInfos { get; }

    ICollection<INfp> Polygons { get; }

    List<ISheetLoadInfo> SheetInfos { get; }

    List<INfp> Sheets { get; }

    string ToJson();
  }
}