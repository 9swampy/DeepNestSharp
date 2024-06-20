namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Threading.Tasks;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;

  public class NestResultViewModel : FileViewModel
  {
    private ObservableNestResult nestResult;
    private int selectedIndex;
    private ObservableSheetPlacement selectedItem;
    private RelayCommand<ISheetPlacement> loadSheetPlacementCommand;
    private AsyncRelayCommand<ISheetPlacement> exportSheetPlacementCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestResultViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public NestResultViewModel(IMainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NestResultViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public NestResultViewModel(IMainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public NestResultViewModel(IMainViewModel mainViewModel, INestResult nestResult)
      : this(mainViewModel)
    {
      if (nestResult is ObservableNestResult obs)
      {
        this.nestResult = obs;
      }
      else
      {
        this.nestResult = new ObservableNestResult(nestResult);
      }
    }

    public IRelayCommand<ISheetPlacement> ExportSheetPlacementCommand => this.exportSheetPlacementCommand ?? (this.exportSheetPlacementCommand = new AsyncRelayCommand<ISheetPlacement>(this.OnExportSheetPlacementAsync));

    public override string FileDialogFilter => DeepNestLib.Placement.NestResult.FileDialogFilter;

    public IRelayCommand<ISheetPlacement> LoadSheetPlacementCommand => this.loadSheetPlacementCommand ?? (this.loadSheetPlacementCommand = new RelayCommand<ISheetPlacement>(this.OnLoadSheetPlacement));

    public INestResult NestResult => this.nestResult;

    public override string TextContent => this.NestResult?.ToJson() ?? string.Empty;

    public int SelectedIndex
    {
      get => this.selectedIndex;
      set => this.SetProperty(ref this.selectedIndex, value);
    }

    public ObservableSheetPlacement SelectedItem
    {
      get => this.selectedItem;
      set
      {
        if (value is ObservableSheetPlacement observableSheetPlacement)
        {
          this.SetProperty(ref this.selectedItem, observableSheetPlacement, nameof(this.SelectedItem));
        }
        else
        {
          throw new ArgumentException(nameof(this.SelectedItem));
        }
      }
    }

    protected override void LoadContent()
    {
      NestResult nestResult = DeepNestLib.Placement.NestResult.LoadFromFile(this.FilePath);
      this.nestResult = new ObservableNestResult(nestResult);

      this.nestResult.PropertyChanged += this.NestResult_PropertyChanged;
      this.NotifyContentUpdated();
    }

    protected override void NotifyContentUpdated()
    {
      this.OnPropertyChanged(nameof(this.NestResult));
    }

    protected override void SaveState()
    {
      // Don't do anything, DeepNestSharp only consumes and can be used to inspect Part files.
    }

    private void NestResult_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      this.NotifyContentUpdated();
    }

    private async Task OnExportSheetPlacementAsync(ISheetPlacement sheetPlacement)
    {
      await this.MainViewModel.ExportSheetPlacementAsync(sheetPlacement).ConfigureAwait(false);
    }

    private void OnLoadSheetPlacement(ISheetPlacement sheetPlacement)
    {
      if (sheetPlacement != null)
      {
        this.MainViewModel.LoadSheetPlacement(sheetPlacement);
      }
    }
  }
}