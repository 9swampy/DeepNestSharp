namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.Docking;

  public class SheetPlacementViewModel : FileViewModel, ISheetPlacementViewModel
  {
    private readonly IMouseCursorService mouseCursorService;
    private int selectedIndex;
    private IPartPlacement selectedItem;
    private ObservableSheetPlacement observableSheetPlacement;
    private RelayCommand loadPartFileCommand = null;
    private AsyncRelayCommand loadAllExactCommand;
    private AsyncRelayCommand exportSheetPlacementCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SheetPlacementViewModel(IMainViewModel mainViewModel, IMouseCursorService mouseCursorService)
      : base(mainViewModel)
    {
      this.mouseCursorService = mouseCursorService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SheetPlacementViewModel(IMainViewModel mainViewModel, ISheetPlacement sheetPlacement, IMouseCursorService mouseCursorService)
      : this(mainViewModel, mouseCursorService)
    {
      if (sheetPlacement is ObservableSheetPlacement observableSheetPlacement)
      {
        this.observableSheetPlacement = observableSheetPlacement;
      }
      else
      {
        this.observableSheetPlacement = new ObservableSheetPlacement((SheetPlacement)sheetPlacement);
      }

      this.observableSheetPlacement.PropertyChanged += this.SheetPlacement_PropertyChanged;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public SheetPlacementViewModel(IMainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public ICommand ExportSheetPlacementCommand => this.exportSheetPlacementCommand ?? (this.exportSheetPlacementCommand = new AsyncRelayCommand(this.OnExportSheetPlacement));

    public override string FileDialogFilter => DeepNestLib.Placement.SheetPlacement.FileDialogFilter;

    public ICommand LoadAllExactCommand => this.loadAllExactCommand ?? (this.loadAllExactCommand = new AsyncRelayCommand(this.OnLoadAllExactAsync, () => true)); // this.SheetPlacement.PartPlacements.Any(p => !p.Part.IsExact)));

    public ICommand LoadPartFileCommand => this.loadPartFileCommand ?? (this.loadPartFileCommand = new RelayCommand(this.OnLoadPartFile, () => new FileInfo(this.SelectedItem.Part.Name).Exists));

    public ISheetPlacement SheetPlacement => this.observableSheetPlacement;

    public int SelectedIndex
    {
      get => this.selectedIndex;
      set => this.SetProperty(ref this.selectedIndex, value);
    }

    public IPartPlacement SelectedItem
    {
      get => this.selectedItem;
      set
      {
        if (value is ObservablePartPlacement observablePartPlacement)
        {
          this.SetProperty(ref this.selectedItem, observablePartPlacement, nameof(this.SelectedItem));
        }
        else
        {
          throw new ArgumentException(nameof(this.SelectedItem));
        }
      }
    }

    public override string TextContent => this.SheetPlacement.ToJson();

    public void RaiseDrawingContext()
    {
      // This makes the drag render holes correctly but seriously kills the drag.
      this.OnPropertyChanged(nameof(this.SelectedItem));
    }

    protected override void LoadContent()
    {
      ObservableSheetPlacement sheetPlacement = new ObservableSheetPlacement(DeepNestLib.Placement.SheetPlacement.LoadFromFile(this.FilePath));
      Debug.Print("Force Exact=false on SheetPlacement load.");
      foreach (IPartPlacement pp in sheetPlacement.PartPlacements)
      {
        pp.Part.Points.First().Exact = false;
      }

      sheetPlacement.PropertyChanged += this.SheetPlacement_PropertyChanged;
      this.observableSheetPlacement = sheetPlacement;
      this.OnPropertyChanged(nameof(this.SheetPlacement));
    }

    private void SheetPlacement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      this.NotifyContentUpdated();
    }

    protected override void NotifyContentUpdated()
    {
      this.OnPropertyChanged(nameof(this.SheetPlacement));
      this.OnPropertyChanged(nameof(this.IsDirty));
    }

    private async Task OnExportSheetPlacement()
    {
      await this.MainViewModel.ExportSheetPlacementAsync(this.SheetPlacement).ConfigureAwait(false);
    }

    private async Task OnLoadAllExactAsync()
    {
      this.mouseCursorService.OverrideCursor = Cursors.Wait;
      System.Collections.Generic.List<ObservablePartPlacement> partPlacementList = this.observableSheetPlacement.PartPlacements.Cast<ObservablePartPlacement>().ToList();
      foreach (ObservablePartPlacement pp in partPlacementList)
      {
        await pp.OnLoadExact();
      }

      this.IsDirty = false;
      this.NotifyContentUpdated();
      this.loadAllExactCommand?.NotifyCanExecuteChanged();
      this.mouseCursorService.OverrideCursor = Cursors.Null;
    }

    private void OnLoadPartFile()
    {
      this.MainViewModel.LoadPart(this.SelectedItem.Part.Name);
    }

    protected override void SaveState()
    {
      this.observableSheetPlacement.SaveState();
    }
  }
}
