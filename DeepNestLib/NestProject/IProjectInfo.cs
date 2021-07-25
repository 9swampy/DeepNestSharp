using System.Collections.Generic;

namespace DeepNestLib.NestProject
{
  public interface IProjectInfo
  {
    IList<IDetailLoadInfo> DetailLoadInfos { get; }

    IList<ISheetLoadInfo> SheetLoadInfos { get; }

    void Load(string filePath);

    void Load(ProjectInfo source);

    string ToJson();
  }
}