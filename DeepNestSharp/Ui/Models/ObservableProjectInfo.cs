namespace DeepNestSharp.Ui.Models
{
  using System;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain.Models;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableProjectInfo : ObservableObject, IProjectInfo
  {
    private readonly IProjectInfo wrappedProjectInfo;
    private ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>? detailLoadInfos;
    private ObservableCollection<ISheetLoadInfo, SheetLoadInfo, ObservableSheetLoadInfo>? sheetLoadInfos;

    public event EventHandler IsDirtyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableProjectInfo"/> class.
    /// </summary>
    /// <param name="projectInfo">The ProjectInfo to wrap.</param>
    public ObservableProjectInfo(IProjectInfo projectInfo) => this.wrappedProjectInfo = projectInfo;

    public IList<IDetailLoadInfo, DetailLoadInfo> DetailLoadInfos
    {
      get
      {
        if (this.detailLoadInfos == null || this.detailLoadInfos.Count == 0)
        {
          this.detailLoadInfos = new ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>(this.wrappedProjectInfo.DetailLoadInfos, x => new ObservableDetailLoadInfo(x));
          this.detailLoadInfos.IsDirtyChanged += this.DetailLoadInfos_IsDirtyChanged;
        }

        return this.detailLoadInfos;
      }
    }

    private void DetailLoadInfos_IsDirtyChanged(object? sender, EventArgs e)
    {
      IsDirtyChanged?.Invoke(this, e);
    }

    public IList<ISheetLoadInfo, SheetLoadInfo> SheetLoadInfos
    {
      get
      {
        if (this.sheetLoadInfos == null || this.sheetLoadInfos.Count == 0)
        {
          sheetLoadInfos = new ObservableCollection<ISheetLoadInfo, SheetLoadInfo, ObservableSheetLoadInfo>(this.wrappedProjectInfo.SheetLoadInfos, x => new ObservableSheetLoadInfo(x));
        }

        return this.sheetLoadInfos;
      }
    }

    public ISvgNestConfig Config => SvgNest.Config;

    public void Load(string filePath)
    {
      wrappedProjectInfo.Load(filePath);
      this.DetailLoadInfos.Clear();
      this.SheetLoadInfos.Clear();
      OnPropertyChanged(nameof(DetailLoadInfos));
      OnPropertyChanged(nameof(SheetLoadInfos));
    }

    public void Load(ProjectInfo source)
    {
      wrappedProjectInfo.Load(source);
    }

    public string ToJson()
    {
      return wrappedProjectInfo.ToJson();
    }

    internal void SaveState()
    {
      detailLoadInfos?.SaveState();
    }
  }
}
