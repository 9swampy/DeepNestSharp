namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class PreviewViewModel : ToolViewModel
  {
    private const double Gap = 10;
    private readonly MainViewModel mainViewModel;
    private SheetPlacementViewModel? lastSheetPlacementViewModel;
    private IPartPlacement? hoverPartPlacement;
    private Point mousePosition;
    private Point dragOffset;
    private Point? dragStart;
    private double canvasScale = 1;
    private Point canvasOffset = new Point(0, 0);
    private Point? viewport;
    private Transform? transform;
    private RelayCommand? fitAllCommand = null;
    private Point? actual;
    private Point? canvasPosition;
    private bool isExperimental;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PreviewViewModel(MainViewModel mainViewModel)
      : base("Preview")
    {
      this.mainViewModel = mainViewModel;
      this.mainViewModel.ActiveDocumentChanged += MainViewModel_ActiveDocumentChanged;
      this.PropertyChanged += this.PreviewViewModel_PropertyChanged;
    }

    public FileViewModel? ActiveDocument => mainViewModel.ActiveDocument;

    public ZoomPreviewDrawingContext ZoomDrawingContext { get; } = new ZoomPreviewDrawingContext();

    public ObservableCollection<object> DrawingContext { get; } = new ObservableCollection<object>();

    public ICommand FitAllCommand
    {
      get
      {
        if (fitAllCommand == null)
        {
          fitAllCommand = new RelayCommand(OnFitAll);
        }

        return fitAllCommand;
      }
    }

    public MainViewModel MainViewModel => mainViewModel;

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
          OnPropertyChanged(nameof(SelectedPartPlacement));
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
        OnPropertyChanged(nameof(ZoomDrawingContext));
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

    public Point? CanvasPosition
    {
      get => canvasPosition;
      internal set
      {
        SetProperty(ref canvasPosition, value, nameof(CanvasPosition));
      }
    }

    public bool IsDragging
    {
      get => dragStart.HasValue;
    }

    public bool IsExperimental
    {
      get => isExperimental;
      set
      {
        SetProperty(ref isExperimental, value, nameof(IsExperimental));
        InitialiseDrawingContext(mainViewModel);
      }
    }

    public Point DragOffset
    {
      get => dragOffset;
      internal set
      {
        SetProperty(ref dragOffset, value, nameof(DragOffset));
      }
    }

    public Point? DragStart
    {
      get => dragStart;
      internal set
      {
        SetProperty(ref dragStart, value, nameof(DragStart));
        OnPropertyChanged(nameof(IsDragging));
        OnPropertyChanged(nameof(DrawingContext));
        OnPropertyChanged(nameof(ZoomDrawingContext));
      }
    }

    public double CanvasScale
    {
      get => canvasScale;
      set
      {
        SetProperty(ref canvasScale, value, nameof(CanvasScale));
      }
    }

    public bool IsTransformSet => this.Transform != null;

    public double CanvasScaleMax => 10;

    public double CanvasScaleMin => 0.5;

    public Point CanvasOffset
    {
      get => canvasOffset;
      internal set
      {
        canvasOffset = value;
        if (this.Transform is MatrixTransform matrixTransform)
        {
          var mat = matrixTransform.Matrix;
          mat.Translate(canvasOffset.X - mat.OffsetX, canvasOffset.Y - mat.OffsetY);
          matrixTransform.Matrix = mat;
        }

        OnPropertyChanged(nameof(CanvasOffset));
      }
    }

    public Canvas? Canvas
    {
      get;
      internal set;
    }

    public Point LowerBound
    {
      get
      {
        return new Point(DrawingContext.Extremum(MinMax.Min, XY.X), DrawingContext.Extremum(MinMax.Min, XY.Y));
      }
    }

    public Point UpperBound
    {
      get
      {
        return new Point(DrawingContext.Extremum(MinMax.Max, XY.X), DrawingContext.Extremum(MinMax.Max, XY.Y));
      }
    }

    public double WidthBound
    {
      get
      {
        return DrawingContext.Extremum(MinMax.Max, XY.X) - DrawingContext.Extremum(MinMax.Min, XY.X);
      }
    }

    public double HeightBound
    {
      get
      {
        return DrawingContext.Extremum(MinMax.Max, XY.Y) - DrawingContext.Extremum(MinMax.Min, XY.Y);
      }
    }

    public Point? Actual
    {
      get => actual;
      internal set
      {
        SetProperty(ref actual, value, nameof(Actual));
      }
    }

    public Point? Viewport
    {
      get => viewport;
      internal set
      {
        SetProperty(ref viewport, value, nameof(Viewport));
      }
    }

    public Transform? Transform
    {
      get => transform;
      internal set
      {
        SetProperty(ref transform, value, nameof(Transform));
        OnPropertyChanged(nameof(IsTransformSet));
      }
    }

    public double LimitAbsoluteScale(double proposed)
    {
      if (proposed > CanvasScaleMax)
      {
        return CanvasScaleMax;
      }
      else if (proposed < CanvasScaleMin)
      {
        return CanvasScaleMin;
      }

      return proposed;
    }

    /// <summary>
    /// If proposed scale breaches the limits then limit the scale to that which will scale to the limit.
    /// </summary>
    /// <param name="proposed">Proposed scale.</param>
    /// <returns>Permissible scale within limits.</returns>
    public double LimitScaleTransform(double proposed)
    {
      if (proposed * CanvasScale > CanvasScaleMax)
      {
        proposed = CanvasScaleMax / CanvasScale;
      }
      else if (proposed * CanvasScale < CanvasScaleMin)
      {
        proposed = CanvasScaleMin / CanvasScale;
      }

      return proposed;
    }

    internal void RaiseSelectItem()
    {
      System.Diagnostics.Debug.Print("Force RaiseSelectItem");
      OnPropertyChanged(nameof(SelectedPartPlacement));
    }

    internal void RaiseDrawingContext()
    {
      // System.Diagnostics.Debug.Print("Force RaiseDrawingContext");
      OnPropertyChanged(nameof(DrawingContext));
      lastSheetPlacementViewModel?.RaiseDrawingContext();
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

    private void InitialiseDrawingContext(object? sender)
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
        else if (mainViewModel.ActiveDocument is PartEditorViewModel partViewModel)
        {
          if (partViewModel.Part is ObservableNfp nfp)
          {
            Set(nfp);
          }
        }
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object? sender, EventArgs e)
    {
      InitialiseDrawingContext(sender);
      OnPropertyChanged(nameof(ActiveDocument));
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

    private void OnFitAll()
    {
      CanvasScale = Math.Min(
        (Actual?.X) / (ZoomDrawingContext.Extremum(MinMax.Max, XY.X) - ZoomDrawingContext.Extremum(MinMax.Min, XY.X)) ?? 5,
        (Actual?.Y) / (ZoomDrawingContext.Extremum(MinMax.Max, XY.Y) - ZoomDrawingContext.Extremum(MinMax.Min, XY.Y)) ?? 5);
    }

    private void PreviewViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(DrawingContext))
      {
        OnPropertyChanged(nameof(UpperBound));
        OnPropertyChanged(nameof(LowerBound));
        OnPropertyChanged(nameof(WidthBound));
        OnPropertyChanged(nameof(HeightBound));
      }
    }

    private void ResetDrawingContext()
    {
      this.DrawingContext.Clear();
      this.ZoomDrawingContext.Clear();
    }

    private void SheetPlacementViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument &&
          (e.PropertyName == nameof(SheetPlacementViewModel.SelectedItem) ||
           e.PropertyName == nameof(SheetPlacementViewModel.SheetPlacement)) &&
          sender is SheetPlacementViewModel sheetPlacementViewModel &&
          sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
      {
        Set(sheetPlacement);
      }
    }

    private void Set(ObservableSheetPlacement item)
    {
      ResetDrawingContext();
      this.DrawingContext.Add(item);
      this.ZoomDrawingContext.Set(item);
      foreach (var partPlacement in item.PartPlacements)
      {
        INfp part = partPlacement.Part;
        this.DrawingContext.Add(partPlacement);
        foreach (var child in part.Children)
        {
          Set(new ObservableHole((ObservablePartPlacement)partPlacement, Background.ShiftPolygon(child, partPlacement)));
        }
      }

      OnPropertyChanged(nameof(DrawingContext));
      OnPropertyChanged(nameof(ZoomDrawingContext));
    }

    private void Set(ObservableDetailLoadInfo item)
    {
      ResetDrawingContext();
      var polygon = item.LoadAsync().Result;
      var shiftedPart = Background.ShiftPolygon(polygon, -polygon.MinX, -polygon.MinY);
      Set(new ObservableNfp(shiftedPart));
    }

    /// <summary>
    /// Top level for each part added should be Observable.
    /// </summary>
    /// <param name="polygon">Ultimate parent of the part.</param>
    private void Set(ObservableNfp polygon)
    {
      this.DrawingContext.Add(polygon);
      this.ZoomDrawingContext.For(polygon);
      foreach (var child in polygon.Children)
      {
        if (child is ObservableNfp observableChild)
        {
          throw new InvalidOperationException("In the abscence of tests prevent this, not anticipated - wouldn't it mess rendering?");
        }
        else
        {
          Set(new ObservableHole(child));
        }
      }

      OnPropertyChanged(nameof(DrawingContext));
      OnPropertyChanged(nameof(ZoomDrawingContext));
    }

    /// <summary>
    /// Adding in children as <see cref="ObservableHoles"/> so can fill differently.
    /// </summary>
    /// <param name="polygon"></param>
    private void Set(ObservableHole child)
    {
      this.DrawingContext.Add(child);
      foreach (var c in child.Children)
      {
        Set(new ObservableHole(c));
      }

      OnPropertyChanged(nameof(DrawingContext));
    }
  }
}
