namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;

  public interface IProjectInfo
  {
    ISvgNestConfig Config { get; }

    IList<IDetailLoadInfo> DetailLoadInfos { get; }

    IList<ISheetLoadInfo> SheetLoadInfos { get; }

    void Load(string filePath);

    void Load(ProjectInfo source);

    string ToJson();
  }
}