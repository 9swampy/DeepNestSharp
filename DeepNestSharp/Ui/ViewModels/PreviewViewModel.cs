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
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class PreviewViewModel : ToolViewModel
  {
    private const double Gap = 10;
    private readonly MainViewModel mainViewModel;
    private readonly PointCollection points = new PointCollection();
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

    public ObservableCollection<object> DrawingContext { get; private set; } = new ObservableCollection<object>();

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
      set
      {
        canvasScale = value;
        if (this.Transform is MatrixTransform matrixTransform)
        {
          var scale = canvasScale / matrixTransform.Matrix.M11;
          var mat = matrixTransform.Matrix;
          mat.Scale(scale, scale);
          matrixTransform.Matrix = mat;
        }

        OnPropertyChanged(nameof(CanvasScale));
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
        return new Point(Extremum(o => o.MinX), Extremum(o => o.MinY));
      }
    }

    public Point UpperBound
    {
      get
      {
        return new Point(Extremum(o => o.MaxX), Extremum(o => o.MaxY));
      }
    }

    public double WidthBound
    {
      get
      {
        return Extremum(o => o.MaxX) - Extremum(o => o.MinX);
      }
    }

    public double HeightBound
    {
      get
      {
        return Extremum(o => o.MaxY) - Extremum(o => o.MinY);
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
        else if (mainViewModel.ActiveDocument is PartViewModel partViewModel)
        {
          if (partViewModel.Part is ObservableNfp nfp)
          {
            Set(nfp);
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

    private void OnFitAll()
    {
      if (this.Viewport.HasValue)
      {
        var maxX = Extremum(p => p.MaxX);
        var minX = Extremum(p => p.MinX);
        var maxY = Extremum(p => p.MaxY);
        var minY = Extremum(p => p.MinY);

        var width = this.Viewport.Value.X;
        var height = this.Viewport.Value.Y;

        var deltaX = maxX - minX;
        var scaleX = width / deltaX;
        var deltaY = maxY - minY;
        var scaleY = height / deltaY;

        // var oz = this.CanvasScale;
        // var sz1 = new System.Drawing.Size((int)(deltaX * scaleX), (int)(deltaY * scaleX));
        // var sz2 = new System.Drawing.Size((int)(deltaX * scaleY), (int)(deltaY * scaleY));
        this.CanvasScale = Math.Min(scaleX, scaleY);

        var x = (deltaX / 2) + minX;
        var y = (deltaY / 2) + minY;

        this.CanvasOffset = new Point((width / 2F / this.CanvasScale) - x, -((height / 2F / this.CanvasScale) + y));
      }
    }

    private double Extremum(Func<IMinMaxXY, double> accessor)
    {
      return this.DrawingContext.Max(o =>
      {
        if (o is IMinMaxXY item)
        {
          return accessor(item);
        }

        throw new NotSupportedException($"{o.GetType().Name} could not be handled.");
      });
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

    private void Set(ObservableSheetPlacement item)
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

    private void Set(ObservableDetailLoadInfo item)
    {
      ResetDrawingContext();
      var polygon = item.Load();
      Set(new ObservableNfp(polygon));
    }

    /// <summary>
    /// Top level for each part added should be Observable.
    /// </summary>
    /// <param name="polygon">Ultimate parent of the part.</param>
    private void Set(ObservableNfp polygon)
    {
      this.DrawingContext.Add(polygon);
      foreach (var child in polygon.Children)
      {
        if (child is ObservableNfp observableChild)
        {
          Set(observableChild);
        }
        else
        {
          Set(child);
        }
      }

      OnPropertyChanged(nameof(DrawingContext));
    }

    /// <summary>
    /// Adding in children as not ObservableHoles so can fill differently.
    /// </summary>
    /// <param name="polygon"></param>
    private void Set(INfp polygon)
    {
      this.DrawingContext.Add(new ObservableHole(polygon));
      foreach (var child in polygon.Children)
      {
        Set(child);
      }

      OnPropertyChanged(nameof(DrawingContext));
    }
  }
}
