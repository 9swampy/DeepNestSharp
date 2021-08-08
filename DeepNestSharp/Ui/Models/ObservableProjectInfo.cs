namespace DeepNestSharp.Ui.Models
{
  using System;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableProjectInfo : ObservableObject, IProjectInfo
  {
    private readonly IProjectInfo projectInfo;
    private ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>? detailLoadInfos;
    private ObservableCollection<ISheetLoadInfo, SheetLoadInfo, ObservableSheetLoadInfo>? sheetLoadInfos;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableProjectInfo"/> class.
    /// </summary>
    /// <param name="projectInfo">The ProjectInfo to wrap.</param>
    public ObservableProjectInfo(IProjectInfo projectInfo) => this.projectInfo = projectInfo;

    public IList<IDetailLoadInfo, DetailLoadInfo> DetailLoadInfos
    {
      get
      {
        if (this.detailLoadInfos == null)
        {
          this.detailLoadInfos = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(this.projectInfo.DetailLoadInfos, x => new ObservableDetailLoadInfo(x));
        }

        return this.detailLoadInfos;
      }
    }

    public IList<ISheetLoadInfo, SheetLoadInfo> SheetLoadInfos
    {
      get
      {
        if (this.sheetLoadInfos == null)
        {
          sheetLoadInfos = new ObservableCollection<ISheetLoadInfo, SheetLoadInfo, ObservableSheetLoadInfo>(this.projectInfo.SheetLoadInfos, x => new ObservableSheetLoadInfo(x));
        }

        return this.sheetLoadInfos;
      }
    }

    public ISvgNestConfig Config => SvgNest.Config;

    public void Load(string filePath)
    {
      projectInfo.Load(filePath);
      OnPropertyChanged(nameof(DetailLoadInfos));
      OnPropertyChanged(nameof(SheetLoadInfos));
    }

    public void Load(ProjectInfo source)
    {
      projectInfo.Load(source);
    }

    public string ToJson()
    {
      return projectInfo.ToJson();
    }
  }
}
