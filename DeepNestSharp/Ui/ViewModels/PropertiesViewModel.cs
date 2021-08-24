namespace DeepNestSharp.Ui.ViewModels
{
  using System.ComponentModel;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.ViewModels;

  public class PropertiesViewModel : ToolViewModel, IPropertiesViewModel
  {
    private readonly IMainViewModel mainViewModel;

    private SheetPlacementViewModel? lastSheetPlacementViewModel;
    private object selectedObject;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertiesViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PropertiesViewModel(IMainViewModel mainViewModel)
      : base("Properties")
    {
      this.mainViewModel = mainViewModel;
      this.mainViewModel.ActiveDocumentChanged += this.MainViewModel_ActiveDocumentChanged;
    }

    public object? SelectedObject
    {
      get
      {
        return this.selectedObject;
      }

      set
      {
        SetProperty(ref selectedObject, value, nameof(SelectedObject));
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object? sender, System.EventArgs e)
    {
      if (lastSheetPlacementViewModel != null)
      {
        lastSheetPlacementViewModel.PropertyChanged -= this.LastSheetPlacementViewModel_PropertyChanged;
        lastSheetPlacementViewModel = null;
      }

      if (sender is IMainViewModel mainViewModel)
      {
        SelectedObject = null;

        if (mainViewModel.ActiveDocument is SheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          lastSheetPlacementViewModel = sheetPlacementViewModel;
          lastSheetPlacementViewModel.PropertyChanged += this.LastSheetPlacementViewModel_PropertyChanged;
          sheetPlacementViewModel.PropertyChanged += SheetPlacementViewModel_PropertyChanged;
          Set(sheetPlacementViewModel);
        }
        else if (mainViewModel.ActiveDocument is NestProjectViewModel nestProjectViewModel)
        {
          nestProjectViewModel.PropertyChanged += NestProjectViewModel_PropertyChanged;
          if (nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
          {
            SelectedObject = detailLoadInfo;
          }
        }
      }
    }

    private void LastSheetPlacementViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedItem" &&
          sender is SheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        Set(sheetPlacementViewModel);
      }
    }

    private void NestProjectViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedDetailLoadInfo" &&
          sender is NestProjectViewModel nestProjectViewModel &&
          nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
      {
        SelectedObject = detailLoadInfo;
      }
    }

    private void Set(SheetPlacementViewModel sheetPlacementViewModel)
    {
      if (sheetPlacementViewModel.SelectedItem == null)
      {
        SelectedObject = sheetPlacementViewModel.SheetPlacement;
      }
      else
      {
        SelectedObject = sheetPlacementViewModel.SelectedItem;
      }
    }

    private void SheetPlacementViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedItem" &&
          sender is SheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        Set(sheetPlacementViewModel);
      }
    }
  }
}