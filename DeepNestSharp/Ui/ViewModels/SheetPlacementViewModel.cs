namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Windows.Input;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class SheetPlacementViewModel : FileViewModel, ISheetPlacementViewModel
  {
    private int selectedIndex;
    private IPartPlacement selectedItem;
    private ISheetPlacement sheetPlacement;
    private RelayCommand? loadPartFileCommand = null;

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

    public ICommand LoadPartFileCommand
    {
      get
      {
        if (loadPartFileCommand == null)
        {
          loadPartFileCommand = new RelayCommand(OnLoadPartFile, () => new FileInfo(this.SelectedItem.Part.Name).Exists);
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
    }

    private void OnLoadPartFile()
    {
      this.MainViewModel.LoadPart(SelectedItem.Part.Name);
    }
  }
}
