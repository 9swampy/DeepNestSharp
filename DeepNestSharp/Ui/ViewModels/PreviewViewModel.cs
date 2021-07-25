namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Windows.Media;
  using DeepNestLib;
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
      : base("Preview")
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
        ResetDrawingContext();

        if (mainViewModel.ActiveDocument is SheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          getSelectedPartPlacementFunc = () => sheetPlacementViewModel.SelectedItem;
          sheetPlacementViewModel.PropertyChanged += SheetPlacementViewModel_PropertyChanged;
          Set(sheetPlacement);
        }
        else if (mainViewModel.ActiveDocument is NestProjectViewModel nestProjectViewModel)
        {
          nestProjectViewModel.PropertyChanged += NestProjectViewModel_PropertyChanged;
          if (nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
          {
            Set(detailLoadInfo);
          }
        }
      }
    }

    private void NestProjectViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedDetailLoadInfo" &&
          sender is NestProjectViewModel nestProjectViewModel &&
          nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
      {
        Set(detailLoadInfo);
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
      ResetDrawingContext();
      this.DrawingContext.Add(item);
      foreach (var partPlacement in item.PartPlacements)
      {
        this.DrawingContext.Add(partPlacement);
      }

      OnPropertyChanged(nameof(DrawingContext));
    }

    private void ResetDrawingContext()
    {
      this.points?.Clear();
      this.DrawingContext.Clear();
    }

    public void Set(ObservableDetailLoadInfo item)
    {
      ResetDrawingContext();
      var polygon = item.Load();
      this.DrawingContext.Add(new ObservableNfp(polygon));
      foreach (var child in polygon.Children)
      {
        this.DrawingContext.Add(new ObservableNfp(child));
      }

      OnPropertyChanged(nameof(DrawingContext));
    }
  }
}
