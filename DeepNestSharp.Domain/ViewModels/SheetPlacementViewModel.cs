namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.Services;
  using DeepNestSharp.Ui.Docking;
  using Light.GuardClauses;
  using Microsoft.Toolkit.Mvvm.Input;

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
    public SheetPlacementViewModel(IMainViewModel mainViewModel, IMouseCursorService mouseCursorService, IRelativePathHelper relativePathHelper)
      : base(mainViewModel, relativePathHelper)
    {
      this.mouseCursorService = mouseCursorService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SheetPlacementViewModel(IMainViewModel mainViewModel, ISheetPlacement sheetPlacement, IMouseCursorService mouseCursorService, IRelativePathHelper relativePathHelper)
      : this(mainViewModel, mouseCursorService, relativePathHelper)
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
    public SheetPlacementViewModel(IMainViewModel mainViewModel, string filePath, IRelativePathHelper relativePathHelper)
      : base(mainViewModel, filePath, relativePathHelper)
    {
    }

    public ICommand ExportSheetPlacementCommand => exportSheetPlacementCommand ?? (exportSheetPlacementCommand = new AsyncRelayCommand(OnExportSheetPlacement));

    public override string FileDialogFilter => DeepNestLib.Placement.SheetPlacement.FileDialogFilter;

    public ICommand LoadAllExactCommand => loadAllExactCommand ?? (loadAllExactCommand = new AsyncRelayCommand(OnLoadAllExactAsync, () => true)); // this.SheetPlacement.PartPlacements.Any(p => !p.Part.IsExact)));

    public ICommand LoadPartFileCommand => loadPartFileCommand ?? (loadPartFileCommand = new RelayCommand(OnLoadPartFile, () => FileExists));

    private bool FileExists
    {
      get
      {
        return !string.IsNullOrEmpty(this.SelectedItem.Part.Name) && new FileInfo(this.SelectedItem.Part.Name).Exists;
      }
    }

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

    public void RaiseDrawingContext()
    {
      // This makes the drag render holes correctly but seriously kills the drag.
      OnPropertyChanged(nameof(SelectedItem));
    }

    protected override void LoadContent(IRelativePathHelper relativePathHelper)
    {
      relativePathHelper.MustNotBeNull();
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

    private void SheetPlacement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
      mouseCursorService.OverrideCursor = Cursors.Wait;
      var partPlacementList = this.observableSheetPlacement.PartPlacements.Cast<ObservablePartPlacement>().ToList();
      foreach (var pp in partPlacementList)
      {
        await pp.OnLoadExact();
      }

      this.IsDirty = false;
      NotifyContentUpdated();
      loadAllExactCommand?.NotifyCanExecuteChanged();
      mouseCursorService.OverrideCursor = Cursors.Null;
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
