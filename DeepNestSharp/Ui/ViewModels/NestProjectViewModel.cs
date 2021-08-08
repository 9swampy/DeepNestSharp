namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class NestProjectViewModel : FileViewModel, INestProjectViewModel
  {
    private readonly ObservableProjectInfo observableProjectInfo = new ObservableProjectInfo(new ProjectInfo());

    private int selectedDetailLoadInfoIndex;
    private IDetailLoadInfo? selectedDetailLoadInfo;
    private RelayCommand executeNestCommand;
    private AsyncRelayCommand addPartCommand;
    private IFileIoService fileIoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public NestProjectViewModel(MainViewModel mainViewModel, IFileIoService fileIoService)
      : base(mainViewModel)
    {
      Initialise(mainViewModel, fileIoService);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestProjectViewModel(MainViewModel mainViewModel, string filePath, IFileIoService fileIoService)
      : base(mainViewModel, filePath)
    {
      Initialise(mainViewModel, fileIoService);
    }

    public IAsyncRelayCommand AddPartCommand => addPartCommand;

    public ICommand ExecuteNestCommand => executeNestCommand;

    public override string FileDialogFilter => DeepNestLib.NestProject.ProjectInfo.FileDialogFilter;

    public IProjectInfo ProjectInfo => observableProjectInfo;

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

    public override string TextContent { get => this.ProjectInfo.ToJson(); }

    private void Initialise(MainViewModel mainViewModel, IFileIoService fileIoService)
    {
      executeNestCommand = new RelayCommand(OnExecuteNest, () => !MainViewModel.NestMonitorViewModel.IsRunning);
      addPartCommand = new AsyncRelayCommand(OnAddPartAsync);
      mainViewModel.NestMonitorViewModel.PropertyChanged += this.NestMonitorViewModel_PropertyChanged;
      this.fileIoService = fileIoService;
    }

    protected override void LoadContent()
    {
      this.ProjectInfo.Load(this.FilePath);
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(ProjectInfo));
      OnPropertyChanged(nameof(SelectedDetailLoadInfoIndex));
      OnPropertyChanged(nameof(SelectedDetailLoadInfo));
    }

    private void NestMonitorViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (MainViewModel.DispatcherService.InvokeRequired)
      {
        MainViewModel.DispatcherService.Invoke(() => NestMonitorViewModel_PropertyChanged(sender, e));
      }

      if (e.PropertyName == $"{nameof(NestMonitorViewModel.IsRunning)}")
      {
        MainViewModel.DispatcherService.Invoke(() => executeNestCommand?.NotifyCanExecuteChanged());
      }
    }

    private async Task OnAddPartAsync()
    {
      var filePath = this.fileIoService.GetOpenFilePath(NFP.FileDialogFilter);
      if (!string.IsNullOrWhiteSpace(filePath) && this.fileIoService.Exists(filePath))
      {
        var newPart = new DetailLoadInfo()
        {
          Path = filePath,
        };

        observableProjectInfo.DetailLoadInfos.Add(newPart);
        OnPropertyChanged(nameof(ProjectInfo));
        this.IsDirty = true;
      }
    }

    private void OnExecuteNest()
    {
      MainViewModel.NestMonitorViewModel.IsActive = true;
      MainViewModel.NestMonitorViewModel.TryStart(this);
    }
  }
}