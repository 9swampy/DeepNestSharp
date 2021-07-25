namespace DeepNestSharp.Ui.Models
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using DeepNestLib.NestProject;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableProjectInfo : ObservableObject, IProjectInfo
  {
    private readonly IProjectInfo projectInfo;
    private ObservableCollection<IDetailLoadInfo> detailLoadInfos;
    private ObservableCollection<ISheetLoadInfo> sheetLoadInfos;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableProjectInfo"/> class.
    /// </summary>
    /// <param name="projectInfo">The ProjectInfo to wrap.</param>
    public ObservableProjectInfo(IProjectInfo projectInfo) => this.projectInfo = projectInfo;

    public IList<IDetailLoadInfo> DetailLoadInfos
    {
      get
      {
        if (this.detailLoadInfos == null)
        {
          detailLoadInfos = new ObservableCollection<IDetailLoadInfo>();
          foreach (var detailLoadInfo in this.projectInfo.DetailLoadInfos)
          {
            detailLoadInfos.Add(new ObservableDetailLoadInfo(detailLoadInfo));
          }
        }

        return this.detailLoadInfos;
      }
    }

    public IList<ISheetLoadInfo> SheetLoadInfos
    {
      get
      {
        if (this.sheetLoadInfos == null)
        {
          sheetLoadInfos = new ObservableCollection<ISheetLoadInfo>();
          foreach (var sheetLoadInfo in this.projectInfo.SheetLoadInfos)
          {
            sheetLoadInfos.Add(sheetLoadInfo);
          }
        }

        return this.sheetLoadInfos;
      }
    }

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
