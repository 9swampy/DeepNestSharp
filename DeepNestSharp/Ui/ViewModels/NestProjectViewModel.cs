namespace DeepNestSharp.Ui.ViewModels
{
  using System.Windows.Input;
  using DeepNestLib.NestProject;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class NestProjectViewModel : FileViewModel, INestProjectViewModel
  {
    private int selectedDetailLoadInfoIndex;
    private IDetailLoadInfo selectedDetailLoadInfo;
    private RelayCommand? executeNestCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public NestProjectViewModel(MainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestProjectViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestProjectViewModel(MainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
      mainViewModel.NestMonitorViewModel.PropertyChanged += this.NestMonitorViewModel_PropertyChanged;
    }

    public ICommand ExecuteNestCommand
    {
      get
      {
        if (executeNestCommand == null)
        {
          executeNestCommand = new RelayCommand(OnExecuteNest, () => !MainViewModel.NestMonitorViewModel.IsRunning);
        }

        return executeNestCommand;
      }
    }

    private void NestMonitorViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == $"{nameof(NestMonitorViewModel.IsRunning)}")
      {
        executeNestCommand?.NotifyCanExecuteChanged();
      }
    }

    public IProjectInfo ProjectInfo { get; } = new ObservableProjectInfo(new ProjectInfo());

    public IDetailLoadInfo SelectedDetailLoadInfo
    {
      get => selectedDetailLoadInfo;
      set => SetProperty(ref selectedDetailLoadInfo, value, nameof(SelectedDetailLoadInfo));
    }

    public int SelectedDetailLoadInfoIndex
    {
      get => selectedDetailLoadInfoIndex;
      set => SetProperty(ref selectedDetailLoadInfoIndex, value);
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

    private void OnExecuteNest()
    {
      System.Diagnostics.Debug.Print("Set the Nest Monitor active and start the Nest.");
      MainViewModel.NestMonitorViewModel.TryStart(this);
      //var nestMonitorViewModel = new NestMonitorViewModel(this, MainViewModel);
    }
  }
}