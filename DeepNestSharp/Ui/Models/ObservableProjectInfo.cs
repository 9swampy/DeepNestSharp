namespace DeepNestSharp.Ui.Models
{
  using System;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.ViewModels;
  using Light.GuardClauses;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableProjectInfo : ObservableObject, IProjectInfo
  {
    private readonly MainViewModel mainViewModel;
    private readonly IProjectInfo wrappedProjectInfo;
    private ObservableCollection<IDetailLoadInfo, DetailLoadInfo, ObservableDetailLoadInfo>? detailLoadInfos;
    private ObservableCollection<ISheetLoadInfo, SheetLoadInfo, ObservableSheetLoadInfo>? sheetLoadInfos;
    private ObservableSvgNestConfig? observableConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableProjectInfo"/> class.
    /// </summary>
    /// <param name="projectInfo">The ProjectInfo to wrap.</param>
    public ObservableProjectInfo(MainViewModel mainViewModel)
    {
      this.mainViewModel = mainViewModel;
      this.wrappedProjectInfo = new ProjectInfo(mainViewModel.SvgNestConfigViewModel.SvgNestConfig);
    }

    public event EventHandler? IsDirtyChanged;

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

    public ISvgNestConfig Config
    {
      get
      {
        if (this.observableConfig == null)
        {
          this.observableConfig = (ObservableSvgNestConfig)mainViewModel.SvgNestConfigViewModel.SvgNestConfig;
        }

        return this.observableConfig;
      }
    }

    private void DetailLoadInfos_IsDirtyChanged(object? sender, EventArgs e)
    {
      IsDirtyChanged?.Invoke(this, e);
    }

    public void Load(ISvgNestConfig config, string filePath)
    {
      // This is a fudge; need a better way of injecting Config; loathe to go Locator but AvalonDock's pushing that way.
      config.MustBe(mainViewModel.SvgNestConfigViewModel.SvgNestConfig);
      wrappedProjectInfo.Load(config, filePath);
      this.DetailLoadInfos.Clear();
      this.SheetLoadInfos.Clear();
      OnPropertyChanged(nameof(DetailLoadInfos));
      OnPropertyChanged(nameof(SheetLoadInfos));
      mainViewModel.SvgNestConfigViewModel.RaiseNotifyUpdatePropertyGrid();
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
