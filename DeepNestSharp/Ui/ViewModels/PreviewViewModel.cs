namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.Windows.Media;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;

  public class PreviewViewModel : ToolViewModel
  {
    private readonly MainViewModel mainViewModel;
    private readonly PointCollection points = new PointCollection();
    private Func<IPartPlacement>? getSelectedPartPlacementFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PreviewViewModel(MainViewModel mainViewModel)
      : base(nameof(PreviewViewModel))
    {
      this.mainViewModel = mainViewModel;
      this.mainViewModel.ActiveDocumentChanged += MainViewModel_ActiveDocumentChanged;
    }

    public ObservableCollection<object> DrawingContext { get; private set; } = new ObservableCollection<object>();

    public IPartPlacement? SelectedPartPlacement => getSelectedPartPlacementFunc != null ? getSelectedPartPlacementFunc() : default;

    private void MainViewModel_ActiveDocumentChanged(object? sender, EventArgs e)
    {
      if (sender is MainViewModel mainViewModel)
      {
        getSelectedPartPlacementFunc = null;
        this.points?.Clear();
        this.DrawingContext.Clear();

        if (mainViewModel.ActiveDocument is SheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          getSelectedPartPlacementFunc = () => sheetPlacementViewModel.SelectedItem;
          sheetPlacementViewModel.PropertyChanged += SheetPlacementViewModel_PropertyChanged;
          Set(sheetPlacement);
        }
      }
    }

    private void SheetPlacementViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedItem" &&
          sender is SheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        Set(sheetPlacement);
      }
    }

    public void Set(ObservableSheetPlacement item)
    {
      this.DrawingContext.Add(item);
      foreach (var partPlacement in item.PartPlacements)
      {
        this.DrawingContext.Add(partPlacement);
      }

      OnPropertyChanged(nameof(DrawingContext));
    }
  }
}
