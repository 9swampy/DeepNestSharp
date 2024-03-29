﻿namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Light.GuardClauses;
  using Microsoft.Toolkit.Mvvm.Input;

  public class NestProjectViewModel : FileViewModel, INestProjectViewModel
  {
    private int selectedDetailLoadInfoIndex;
    private IDetailLoadInfo selectedDetailLoadInfo;
    private int selectedSheetLoadInfoIndex;
    private ISheetLoadInfo selectedSheetLoadInfo;
    private AsyncRelayCommand executeNestCommand;
    private AsyncRelayCommand addPartCommand;
    private RelayCommand addSheetCommand;
    private RelayCommand clearPartsCommand;
    private RelayCommand<IDetailLoadInfo> removePartCommand;
    private RelayCommand<ISheetLoadInfo> removeSheetCommand;
    private RelayCommand<string> loadPartCommand;
    private IFileIoService fileIoService;
    private ObservableProjectInfo observableProjectInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public NestProjectViewModel(IMainViewModel mainViewModel, IFileIoService fileIoService)
      : base(mainViewModel)
    {
      Initialise(mainViewModel, fileIoService);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestProjectViewModel(IMainViewModel mainViewModel, string filePath, IFileIoService fileIoService)
      : base(mainViewModel, filePath)
    {
      Initialise(mainViewModel, fileIoService);
    }

    public IAsyncRelayCommand AddPartCommand => addPartCommand ?? (addPartCommand = new AsyncRelayCommand(OnAddPartAsync));

    public IRelayCommand AddSheetCommand => addSheetCommand ?? (addSheetCommand = new RelayCommand(OnAddSheet));

    public IRelayCommand ClearPartsCommand => clearPartsCommand ?? (clearPartsCommand = new RelayCommand(OnClearParts));

    public IRelayCommand<IDetailLoadInfo> RemovePartCommand => removePartCommand ?? (removePartCommand = new RelayCommand<IDetailLoadInfo>(OnRemovePart));

    public IRelayCommand<ISheetLoadInfo> RemoveSheetCommand => removeSheetCommand ?? (removeSheetCommand = new RelayCommand<ISheetLoadInfo>(OnRemoveSheet));

    public AsyncRelayCommand ExecuteNestCommand => this.executeNestCommand ?? (this.executeNestCommand = new AsyncRelayCommand(this.OnExecuteNest, CanExecuteNest));

    private bool CanExecuteNest()
    {
      if (MainViewModel.NestMonitorViewModel.IsRunning || this.ProjectInfo.DetailLoadInfos.Count == 0)
      {
        return false;
      }

      return !this.ProjectInfo.DetailLoadInfos.Any(o => o is ObservableDetailLoadInfo cast && !cast.IsValid);
    }

    public override string FileDialogFilter => DeepNestLib.NestProject.ProjectInfo.FileDialogFilter;

    public IRelayCommand<string> LoadPartCommand => loadPartCommand ?? (loadPartCommand = new RelayCommand<string>(OnLoadPart));

    public IProjectInfo ProjectInfo => observableProjectInfo ?? (observableProjectInfo = new ObservableProjectInfo(MainViewModel));

    public IDetailLoadInfo SelectedDetailLoadInfo
    {
#pragma warning disable CS8603 // Possible null reference return.
      get => selectedDetailLoadInfo;
#pragma warning restore CS8603 // Possible null reference return.
      set => SetProperty(ref selectedDetailLoadInfo, value, nameof(SelectedDetailLoadInfo));
    }

    public int SelectedDetailLoadInfoIndex
    {
      get => selectedDetailLoadInfoIndex;
      set => SetProperty(ref selectedDetailLoadInfoIndex, value);
    }

    public ISheetLoadInfo SelectedSheetLoadInfo
    {
#pragma warning disable CS8603 // Possible null reference return.
      get => selectedSheetLoadInfo;
#pragma warning restore CS8603 // Possible null reference return.
      set => SetProperty(ref selectedSheetLoadInfo, value, nameof(SelectedSheetLoadInfo));
    }

    public int SelectedSheetLoadInfoIndex
    {
      get => selectedSheetLoadInfoIndex;
      set => SetProperty(ref selectedSheetLoadInfoIndex, value);
    }

    public override string TextContent { get => this.ProjectInfo.ToJson(); }

    public bool UsePriority => this.MainViewModel.SvgNestConfigViewModel.SvgNestConfig.UsePriority;

    protected override void LoadContent()
    {
      this.ProjectInfo.Load(this.MainViewModel.SvgNestConfigViewModel.SvgNestConfig, this.FilePath);
      this.ExecuteNestCommand.MustNotBeNull();
      this.executeNestCommand.NotifyCanExecuteChanged();
    }

    protected override void NotifyContentUpdated()
    {
      Contextualise();
      OnPropertyChanged(nameof(SelectedDetailLoadInfoIndex));
      OnPropertyChanged(nameof(SelectedDetailLoadInfo));
    }

    private void Initialise(IMainViewModel mainViewModel, IFileIoService fileIoService)
    {
      this.ProjectInfo.MustBe(observableProjectInfo);
      if (this.observableProjectInfo != null)
      {
        this.observableProjectInfo.IsDirtyChanged += this.ObservableProjectInfo_IsDirtyChanged;
      }

      mainViewModel.NestMonitorViewModel.PropertyChanged += this.NestMonitorViewModel_PropertyChanged;
      this.fileIoService = fileIoService;
    }

    private void NestMonitorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (MainViewModel.DispatcherService.InvokeRequired)
      {
        MainViewModel.DispatcherService.Invoke(() => NestMonitorViewModel_PropertyChanged(sender, e));
      }

      if (e.PropertyName == $"{nameof(INestMonitorViewModel.IsRunning)}")
      {
        MainViewModel.DispatcherService.Invoke(() => executeNestCommand?.NotifyCanExecuteChanged());
      }
    }

    private void ObservableProjectInfo_IsDirtyChanged(object sender, EventArgs e)
    {
      this.IsDirty = true;
    }

    private async Task OnAddPartAsync()
    {
      var filePaths = await this.fileIoService.GetOpenFilePathsAsync(NoFitPolygon.FileDialogFilter);
      foreach (var filePath in filePaths)
      {
        if (!string.IsNullOrWhiteSpace(filePath) && this.fileIoService.Exists(filePath))
        {
          var newPart = new DetailLoadInfo()
          {
            Path = filePath,
          };

          observableProjectInfo?.DetailLoadInfos.Add(newPart);
        }
      }

      Contextualise();
      this.IsDirty = true;
    }

    private void OnAddSheet()
    {
      var newSheet = new SheetLoadInfo(this.ProjectInfo.Config);
      observableProjectInfo?.SheetLoadInfos.Add(newSheet);

      Contextualise();
      this.IsDirty = true;
    }

    private void OnClearParts()
    {
      observableProjectInfo?.DetailLoadInfos.Clear();
      Contextualise();
      this.IsDirty = true;
    }

    private void Contextualise()
    {
      OnPropertyChanged(nameof(ProjectInfo));
      this.executeNestCommand.NotifyCanExecuteChanged();
    }

    private async Task OnExecuteNest()
    {
      MainViewModel.SetSelectedToolView(this);
      await MainViewModel.NestMonitorViewModel.TryStartAsync(this).ConfigureAwait(false);
    }

    private void OnLoadPart(string path)
    {
      if (!string.IsNullOrWhiteSpace(path))
      {
        MainViewModel.LoadPart(path);
      }
    }

    private void OnRemovePart(IDetailLoadInfo arg)
    {
      if (arg != null)
      {
        this.ProjectInfo.DetailLoadInfos.Remove(arg);
        Contextualise();
      }
    }

    private void OnRemoveSheet(ISheetLoadInfo arg)
    {
      if (arg != null)
      {
        this.ProjectInfo.SheetLoadInfos.Remove(arg);
        Contextualise();
      }
    }

    protected override void SaveState()
    {
      observableProjectInfo.SaveState();
    }
  }
}