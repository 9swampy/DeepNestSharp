namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.ComponentModel;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Models;

  public class PropertiesViewModel : ToolViewModel, IPropertiesViewModel
  {
    private readonly IMainViewModel mainViewModel;

    private ISheetPlacementViewModel lastSheetPlacementViewModel;
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

    public object SelectedObject
    {
      get
      {
        return this.selectedObject;
      }

      set
      {
        this.SetProperty(ref this.selectedObject, value, nameof(this.SelectedObject));
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object sender, System.EventArgs e)
    {
      if (this.lastSheetPlacementViewModel != null)
      {
        this.lastSheetPlacementViewModel.PropertyChanged -= this.LastSheetPlacementViewModel_PropertyChanged;
        this.lastSheetPlacementViewModel = null;
      }

      if (sender is IMainViewModel mainViewModel)
      {
        this.SelectedObject = null;

        if (mainViewModel.ActiveDocument is ISheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          this.lastSheetPlacementViewModel = sheetPlacementViewModel;
          this.lastSheetPlacementViewModel.PropertyChanged += this.LastSheetPlacementViewModel_PropertyChanged;
          sheetPlacementViewModel.PropertyChanged += this.SheetPlacementViewModel_PropertyChanged;
          this.Set(sheetPlacementViewModel);
        }
        else if (mainViewModel.ActiveDocument is INestProjectViewModel nestProjectViewModel)
        {
          nestProjectViewModel.PropertyChanged += this.NestProjectViewModel_PropertyChanged;
          if (nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
          {
            this.SelectedObject = detailLoadInfo;
          }
        }
      }
    }

    private void LastSheetPlacementViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender == this.mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedItem" &&
          sender is ISheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        this.Set(sheetPlacementViewModel);
      }
    }

    private void NestProjectViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is INestProjectViewModel nestProjectViewModel)
      {
        if (sender == this.mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedDetailLoadInfo" &&
          nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
        {
          this.SelectedObject = detailLoadInfo;
        }
        else if (sender == this.mainViewModel.ActiveDocument &&
            e.PropertyName == "SelectedSheetLoadInfo" &&
            nestProjectViewModel.SelectedSheetLoadInfo is ObservableSheetLoadInfo sheetLoadInfo)
        {
          this.SelectedObject = sheetLoadInfo;
        }
      }
    }

    private void Set(ISheetPlacementViewModel sheetPlacementViewModel)
    {
      if (sheetPlacementViewModel.SelectedItem == null)
      {
        this.SelectedObject = sheetPlacementViewModel.SheetPlacement;
      }
      else
      {
        this.SelectedObject = sheetPlacementViewModel.SelectedItem;
      }
    }

    private void SheetPlacementViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender == this.mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedItem" &&
          sender is ISheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        this.Set(sheetPlacementViewModel);
      }
    }
  }
}