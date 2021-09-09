namespace DeepNestLib.NestProject
{
  public interface IProjectInfo
  {
    ISvgNestConfig Config { get; }

    IList<IDetailLoadInfo, DetailLoadInfo> DetailLoadInfos { get; }

    IList<ISheetLoadInfo, SheetLoadInfo> SheetLoadInfos { get; }

    void Load(ISvgNestConfig config, string filePath);

    void Load(ProjectInfo source);

    string ToJson();
  }
}