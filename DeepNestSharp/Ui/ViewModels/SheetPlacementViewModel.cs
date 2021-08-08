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
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class SheetPlacementViewModel : FileViewModel, ISheetPlacementViewModel
  {
    private int selectedIndex;
    private IPartPlacement selectedItem;
    private ISheetPlacement sheetPlacement;
    private RelayCommand? loadPartFileCommand = null;
    private AsyncRelayCommand? loadAllExactCommand = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SheetPlacementViewModel(MainViewModel mainViewModel)
      : base(mainViewModel)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SheetPlacementViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    /// <param name="filePath">Path to the file to open.</param>
    public SheetPlacementViewModel(MainViewModel mainViewModel, string filePath)
      : base(mainViewModel, filePath)
    {
    }

    public override string FileDialogFilter => DeepNestLib.Placement.SheetPlacement.FileDialogFilter;

    public ICommand LoadAllExactCommand
    {
      get
      {
        if (loadAllExactCommand == null)
        {
          loadAllExactCommand = new AsyncRelayCommand(OnLoadAllExact, () => this.SheetPlacement.PartPlacements.Any(p => !p.Part.IsExact));
          this.IsDirty = false;
        }

        return loadAllExactCommand;
      }
    }

    public ICommand LoadPartFileCommand
    {
      get
      {
        if (loadPartFileCommand == null)
        {
          loadPartFileCommand = new RelayCommand(OnLoadPartFile, () => new FileInfo(this.SelectedItem.Part.Name).Exists);
          this.IsDirty = false;
        }

        return loadPartFileCommand;
      }
    }

    public ISheetPlacement SheetPlacement
    {
      get => sheetPlacement;
      set => SetProperty(ref sheetPlacement, value);
    }

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
      this.SheetPlacement = sheetPlacement;
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

    private async Task OnLoadAllExact()
    {
      Mouse.OverrideCursor = Cursors.Wait;
      var partPlacementList = this.sheetPlacement.PartPlacements.Cast<ObservablePartPlacement>().ToList();
      foreach (var pp in partPlacementList)
      {
        await pp.LoadExactCommand.ExecuteAsync(null);
      }

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
      throw new NotImplementedException();
    }
  }
}
