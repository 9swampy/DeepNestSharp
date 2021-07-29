namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Windows;
  using System.Windows.Media;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;

  public class PreviewViewModel : ToolViewModel
  {
    private readonly MainViewModel mainViewModel;
    private readonly PointCollection points = new PointCollection();
    private SheetPlacementViewModel lastSheetPlacementViewModel;
    private IPartPlacement hoverPartPlacement;
    private Point mousePosition;
    private Point dragOffset;
    private Point? dragStart;
    private double canvasScale = 1;
    private Point canvasOffset = new Point(0,0);

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

    public IPartPlacement? SelectedPartPlacement
    {
      get
      {
        if (this.mainViewModel.ActiveDocument is SheetPlacementViewModel sheetPlacementViewModel)
        {
          return sheetPlacementViewModel.SelectedItem;
        }

        return default;
      }

      set
      {
        if (this.mainViewModel.ActiveDocument is SheetPlacementViewModel sheetPlacementViewModel)
        {
          sheetPlacementViewModel.SelectedItem = value;
        }
      }
    }

    public IPartPlacement? HoverPartPlacement
    {
      get => hoverPartPlacement;

      set
      {
        hoverPartPlacement = value;
        OnPropertyChanged(nameof(DrawingContext));
        OnPropertyChanged(nameof(SelectedPartPlacement));
        OnPropertyChanged(nameof(HoverPartPlacement));
      }
    }

    public Point MousePosition
    {
      get => mousePosition;
      internal set
      {
        mousePosition = value;
        OnPropertyChanged(nameof(MousePosition));
      }
    }

    public bool IsDragging
    {
      get => dragStart.HasValue;
    }

    public Point DragOffset
    {
      get => dragOffset;
      internal set
      {
        dragOffset = value;
        OnPropertyChanged(nameof(DragOffset));
      }
    }

    public Point? DragStart
    {
      get => dragStart;
      internal set
      {
        dragStart = value;
        OnPropertyChanged(nameof(DragStart));
        OnPropertyChanged(nameof(IsDragging));
      }
    }

    public double CanvasScale
    {
      get => canvasScale;
      internal set
      {
        canvasScale = value;
        OnPropertyChanged(nameof(CanvasScale));
      }
    }

    public Point CanvasOffset
    {
      get => canvasOffset;
      internal set
      {
        canvasOffset = value;
        OnPropertyChanged(nameof(CanvasOffset));
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object? sender, EventArgs e)
    {
      if (lastSheetPlacementViewModel != null)
      {
        lastSheetPlacementViewModel.PropertyChanged -= this.LastSheetPlacementViewModel_PropertyChanged;
        lastSheetPlacementViewModel = null;
      }

      if (sender is MainViewModel mainViewModel)
      {
        ResetDrawingContext();

        if (mainViewModel.ActiveDocument is SheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          lastSheetPlacementViewModel = sheetPlacementViewModel;
          lastSheetPlacementViewModel.PropertyChanged += this.LastSheetPlacementViewModel_PropertyChanged;
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

    private void LastSheetPlacementViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          e.PropertyName == "SelectedItem" &&
          sender is SheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        Set(sheetPlacement);
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
