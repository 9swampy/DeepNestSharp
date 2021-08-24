namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.ViewModels;
  using DeepNestSharp.Ui.Docking;
  using Microsoft.Toolkit.Mvvm.Input;

  public class SheetPlacementViewModel : FileViewModel, ISheetPlacementViewModel
  {
    private int selectedIndex;
    private IPartPlacement? selectedItem;
    private ObservableSheetPlacement? observableSheetPlacement;
    private RelayCommand? loadPartFileCommand = null;
    private AsyncRelayCommand? loadAllExactCommand;
    private AsyncRelayCommand? exportSheetPlacementCommand;


    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SheetPlacementViewModel(IMainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SheetPlacementViewModel(IMainViewModel mainViewModel, ISheetPlacement sheetPlacement)
      : this(mainViewModel)
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

    public ICommand ExportSheetPlacementCommand => exportSheetPlacementCommand ?? (exportSheetPlacementCommand = new AsyncRelayCommand(OnExportSheetPlacement));

    public override string FileDialogFilter => DeepNestLib.Placement.SheetPlacement.FileDialogFilter;

    public ICommand LoadAllExactCommand => loadAllExactCommand ?? (loadAllExactCommand = new AsyncRelayCommand(OnLoadAllExactAsync, () => true)); // this.SheetPlacement.PartPlacements.Any(p => !p.Part.IsExact)));

    public ICommand LoadPartFileCommand => loadPartFileCommand ?? (loadPartFileCommand = new RelayCommand(OnLoadPartFile, () => new FileInfo(this.SelectedItem.Part.Name).Exists));

    public ISheetPlacement SheetPlacement => observableSheetPlacement;

    public int SelectedIndex
    {
      get => selectedIndex;
      set => SetProperty(ref selectedIndex, value);
    }

    public IPartPlacement SelectedItem
    {
      get => selectedItem;
      set
      {
        if (value is ObservablePartPlacement observablePartPlacement)
        {
          SetProperty(ref selectedItem, observablePartPlacement, nameof(SelectedItem));
        }
        else
        {
          throw new ArgumentException(nameof(SelectedItem));
        }
      }
    }

    public override string TextContent => this.SheetPlacement.ToJson();

    internal void RaiseDrawingContext()
    {
      // This makes the drag render holes correctly but seriously kills the drag.
      OnPropertyChanged(nameof(SelectedItem));
    }

    protected override void LoadContent()
    {
      var sheetPlacement = new ObservableSheetPlacement(DeepNestLib.Placement.SheetPlacement.LoadFromFile(this.FilePath));
      Debug.Print("Force Exact=false on SheetPlacement load.");
      foreach (var pp in sheetPlacement.PartPlacements)
      {
        pp.Part.Points.First().Exact = false;
      }

      sheetPlacement.PropertyChanged += this.SheetPlacement_PropertyChanged;
      this.observableSheetPlacement = sheetPlacement;
      OnPropertyChanged(nameof(SheetPlacement));
    }

    private void SheetPlacement_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      NotifyContentUpdated();
    }

    protected override void NotifyContentUpdated()
    {
      OnPropertyChanged(nameof(SheetPlacement));
      OnPropertyChanged(nameof(IsDirty));
    }

    private async Task OnExportSheetPlacement()
    {
      await MainViewModel.ExportSheetPlacementAsync(this.SheetPlacement).ConfigureAwait(false);
    }

    private async Task OnLoadAllExactAsync()
    {
      Mouse.OverrideCursor = Cursors.Wait;
      var partPlacementList = this.observableSheetPlacement.PartPlacements.Cast<ObservablePartPlacement>().ToList();
      foreach (var pp in partPlacementList)
      {
        await pp.OnLoadExact();
      }

      this.IsDirty = false;
      NotifyContentUpdated();
      loadAllExactCommand?.NotifyCanExecuteChanged();
      Mouse.OverrideCursor = null;
    }

    private void OnLoadPartFile()
    {
      this.MainViewModel.LoadPart(SelectedItem.Part.Name);
    }

    protected override void SaveState()
    {
      this.observableSheetPlacement.SaveState();
    }
  }
}
