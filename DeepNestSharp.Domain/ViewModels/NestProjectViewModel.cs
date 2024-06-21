namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Light.GuardClauses;

  public class NestProjectViewModel : FileViewModel, INestProjectViewModel
  {
    private int selectedDetailLoadInfoIndex;
    private IDetailLoadInfo selectedDetailLoadInfo;
    private int selectedSheetLoadInfoIndex;
    private ISheetLoadInfo selectedSheetLoadInfo;
    private AsyncRelayCommand executeNestCommand;
    private AsyncRelayCommand addPartCommand;
    private AsyncRelayCommand addArbitrarySheetCommand;
    private RelayCommand addRectangleSheetCommand;
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
      this.Initialise(mainViewModel, fileIoService);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestProjectViewModel(IMainViewModel mainViewModel, string filePath, IFileIoService fileIoService)
      : base(mainViewModel, filePath)
    {
      this.Initialise(mainViewModel, fileIoService);
    }

    public IAsyncRelayCommand AddPartCommand => this.addPartCommand ?? (this.addPartCommand = new AsyncRelayCommand(this.OnAddPartAsync));

    public IAsyncRelayCommand AddArbitrarySheetCommand => this.addArbitrarySheetCommand ?? (this.addArbitrarySheetCommand = new AsyncRelayCommand(this.OnAddArbitrarySheetAsync));

    public IRelayCommand AddRectangleSheetCommand => this.addRectangleSheetCommand ?? (this.addRectangleSheetCommand = new RelayCommand(this.OnAddRectangleSheet));

    public IRelayCommand ClearPartsCommand => this.clearPartsCommand ?? (this.clearPartsCommand = new RelayCommand(this.OnClearParts));

    public IRelayCommand<IDetailLoadInfo> RemovePartCommand => this.removePartCommand ?? (this.removePartCommand = new RelayCommand<IDetailLoadInfo>(this.OnRemovePart));

    public IRelayCommand<ISheetLoadInfo> RemoveSheetCommand => this.removeSheetCommand ?? (this.removeSheetCommand = new RelayCommand<ISheetLoadInfo>(this.OnRemoveSheet));

    public AsyncRelayCommand ExecuteNestCommand => this.executeNestCommand ?? (this.executeNestCommand = new AsyncRelayCommand(this.OnExecuteNest, this.CanExecuteNest));

    public override string FileDialogFilter => DeepNestLib.NestProject.ProjectInfo.FileDialogFilter;

    public IRelayCommand<string> LoadPartCommand => this.loadPartCommand ?? (this.loadPartCommand = new RelayCommand<string>(this.OnLoadPart));

    public IProjectInfo ProjectInfo => this.observableProjectInfo ?? (this.observableProjectInfo = new ObservableProjectInfo(this.MainViewModel));

    public IDetailLoadInfo SelectedDetailLoadInfo
    {
      get => this.selectedDetailLoadInfo;
      set => this.SetProperty(ref this.selectedDetailLoadInfo, value, nameof(this.SelectedDetailLoadInfo));
    }

    public int SelectedDetailLoadInfoIndex
    {
      get => this.selectedDetailLoadInfoIndex;
      set => this.SetProperty(ref this.selectedDetailLoadInfoIndex, value);
    }

    public ISheetLoadInfo SelectedSheetLoadInfo
    {
      get => this.selectedSheetLoadInfo;
      set => this.SetProperty(ref this.selectedSheetLoadInfo, value, nameof(this.SelectedSheetLoadInfo));
    }

    public int SelectedSheetLoadInfoIndex
    {
      get => this.selectedSheetLoadInfoIndex;
      set => this.SetProperty(ref this.selectedSheetLoadInfoIndex, value);
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
      this.Contextualise();
      this.OnPropertyChanged(nameof(this.SelectedDetailLoadInfoIndex));
      this.OnPropertyChanged(nameof(this.SelectedDetailLoadInfo));
      this.OnPropertyChanged(nameof(this.SelectedSheetLoadInfoIndex));
      this.OnPropertyChanged(nameof(this.SelectedSheetLoadInfo));
    }

    protected override void SaveState()
    {
      this.observableProjectInfo.SaveState();
    }

    private bool CanExecuteNest()
    {
      if (this.MainViewModel.NestMonitorViewModel.IsRunning || this.ProjectInfo.DetailLoadInfos.Count == 0)
      {
        return false;
      }

      return !this.ProjectInfo.DetailLoadInfos.Any(o => o is ObservableDetailLoadInfo cast && !cast.IsValid);
    }

    private void Initialise(IMainViewModel mainViewModel, IFileIoService fileIoService)
    {
      this.ProjectInfo.MustBe(this.observableProjectInfo);
      if (this.observableProjectInfo != null)
      {
        this.observableProjectInfo.IsDirtyChanged += this.ObservableProjectInfo_IsDirtyChanged;
      }

      mainViewModel.NestMonitorViewModel.PropertyChanged += this.NestMonitorViewModel_PropertyChanged;
      this.fileIoService = fileIoService;
    }

    private void NestMonitorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (this.MainViewModel.DispatcherService.InvokeRequired)
      {
        this.MainViewModel.DispatcherService.Invoke(() => this.NestMonitorViewModel_PropertyChanged(sender, e));
      }

      if (e.PropertyName == $"{nameof(INestMonitorViewModel.IsRunning)}")
      {
        this.MainViewModel.DispatcherService.Invoke(() => this.executeNestCommand?.NotifyCanExecuteChanged());
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
          DetailLoadInfo newPart = new DetailLoadInfo()
          {
            Path = filePath,
          };

          this.observableProjectInfo?.DetailLoadInfos.Add(newPart);
        }
      }

      this.Contextualise();
      this.IsDirty = true;
    }

    private async Task OnAddArbitrarySheetAsync()
    {
      var filePaths = await this.fileIoService.GetOpenFilePathsAsync(NoFitPolygon.FileDialogFilter);
      foreach (var filePath in filePaths)
      {
        if (!string.IsNullOrWhiteSpace(filePath) && this.fileIoService.Exists(filePath))
        {
          SheetLoadInfo newSheet = new SheetLoadInfo(filePath, this.ProjectInfo.Config.SheetQuantity);
          this.observableProjectInfo?.SheetLoadInfos.Add(newSheet);
        }
      }

      this.Contextualise();
      this.IsDirty = true;
    }

    private void OnAddRectangleSheet()
    {
      SheetLoadInfo newSheet = new SheetLoadInfo(this.ProjectInfo.Config);
      this.observableProjectInfo?.SheetLoadInfos.Add(newSheet);

      this.Contextualise();
      this.IsDirty = true;
    }

    private void OnClearParts()
    {
      this.observableProjectInfo?.DetailLoadInfos.Clear();
      this.Contextualise();
      this.IsDirty = true;
    }

    private void Contextualise()
    {
      this.OnPropertyChanged(nameof(this.ProjectInfo));
      this.executeNestCommand.NotifyCanExecuteChanged();
    }

    private async Task OnExecuteNest()
    {
      this.MainViewModel.SetSelectedToolView(this);
      await this.MainViewModel.NestMonitorViewModel.TryStartAsync(this).ConfigureAwait(false);
    }

    private void OnLoadPart(string path)
    {
      if (!string.IsNullOrWhiteSpace(path))
      {
        this.MainViewModel.LoadPart(path);
      }
    }

    private void OnRemovePart(IDetailLoadInfo arg)
    {
      if (arg != null)
      {
        this.ProjectInfo.DetailLoadInfos.Remove(arg);
        this.Contextualise();
      }
    }

    private void OnRemoveSheet(ISheetLoadInfo arg)
    {
      if (arg != null)
      {
        this.ProjectInfo.SheetLoadInfos.Remove(arg);
        this.Contextualise();
      }
    }
  }
}