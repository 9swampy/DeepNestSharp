namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.ComponentModel;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Models;
  using Microsoft.Toolkit.Mvvm.Input;

  public class PreviewViewModel : ToolViewModel, IPreviewViewModel
  {
    private readonly IMainViewModel mainViewModel;
    private IFileViewModel lastActiveViewModel;
    private IPartPlacement hoverPartPlacement;
    private IPointXY mousePosition;
    private IPointXY dragOffset;
    private IPointXY dragStart;
    private double canvasScale = 1;
    private IPointXY viewport;
    private RelayCommand fitAllCommand = null;
    private IPointXY actual;
    private IPointXY canvasPosition;
    private bool isExperimental;

    /// <summary>
    /// Initializes a new instance of the <see cref="PreviewViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public PreviewViewModel(IMainViewModel mainViewModel)
      : base("Preview")
    {
      this.mainViewModel = mainViewModel;
      this.mainViewModel.ActiveDocumentChanged += MainViewModel_ActiveDocumentChanged;
      this.PropertyChanged += this.PreviewViewModel_PropertyChanged;
    }

    public IFileViewModel ActiveDocument => mainViewModel.ActiveDocument;

    public IZoomPreviewDrawingContext ZoomDrawingContext { get; } = new ZoomPreviewDrawingContext();

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

    public IMainViewModel MainViewModel => mainViewModel;

    public IPartPlacement SelectedPartPlacement
    {
      get
      {
        if (this.mainViewModel.ActiveDocument is ISheetPlacementViewModel sheetPlacementViewModel)
        {
          return sheetPlacementViewModel.SelectedItem;
        }

        return default;
      }

      set
      {
        if (this.mainViewModel.ActiveDocument is ISheetPlacementViewModel sheetPlacementViewModel)
        {
          sheetPlacementViewModel.SelectedItem = value;
          OnPropertyChanged(nameof(SelectedPartPlacement));
        }
      }
    }

    public IPartPlacement HoverPartPlacement
    {
      get => hoverPartPlacement;

      set
      {
        hoverPartPlacement = value;
        OnPropertyChanged(nameof(ZoomDrawingContext));
        OnPropertyChanged(nameof(SelectedPartPlacement));
        OnPropertyChanged(nameof(HoverPartPlacement));
      }
    }

    public IPointXY MousePosition
    {
      get => mousePosition;
      set
      {
        mousePosition = value;
        OnPropertyChanged(nameof(MousePosition));
      }
    }

    public IPointXY CanvasPosition
    {
      get => canvasPosition;
      set
      {
        SetProperty(ref canvasPosition, value, nameof(CanvasPosition));
      }
    }

    public bool IsDragging
    {
      get => dragStart != null;
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

    public IPointXY DragOffset
    {
      get => dragOffset;
      set
      {
        SetProperty(ref dragOffset, value, nameof(DragOffset));
      }
    }

    public IPointXY DragStart
    {
      get => dragStart;
      set
      {
        SetProperty(ref dragStart, value, nameof(DragStart));
        OnPropertyChanged(nameof(IsDragging));
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

    public double CanvasScaleMax => 10;

    public double CanvasScaleMin => 0.5;

    public IPointXY LowerBound
    {
      get
      {
        return new SvgPoint(ZoomDrawingContext.Extremum(MinMax.Min, XY.X), ZoomDrawingContext.Extremum(MinMax.Min, XY.Y));
      }
    }

    public IPointXY UpperBound
    {
      get
      {
        return new SvgPoint(ZoomDrawingContext.Extremum(MinMax.Max, XY.X), ZoomDrawingContext.Extremum(MinMax.Max, XY.Y));
      }
    }

    public double WidthBound
    {
      get
      {
        return ZoomDrawingContext.Extremum(MinMax.Max, XY.X) - ZoomDrawingContext.Extremum(MinMax.Min, XY.X);
      }
    }

    public double HeightBound
    {
      get
      {
        return ZoomDrawingContext.Extremum(MinMax.Max, XY.Y) - ZoomDrawingContext.Extremum(MinMax.Min, XY.Y);
      }
    }

    public IPointXY Actual
    {
      get => actual;
      set
      {
        SetProperty(ref actual, value, nameof(Actual));
      }
    }

    public IPointXY Viewport
    {
      get => viewport;
      set
      {
        SetProperty(ref viewport, value, nameof(Viewport));
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

    public void RaiseSelectItem()
    {
      System.Diagnostics.Debug.Print("Force RaiseSelectItem");
      OnPropertyChanged(nameof(SelectedPartPlacement));
    }

    public void RaiseDrawingContext()
    {
      // System.Diagnostics.Debug.Print("Force RaiseDrawingContext");
      OnPropertyChanged(nameof(ZoomDrawingContext));
      if (lastActiveViewModel is ISheetPlacementViewModel sheetPlacementViewModel)
      {
        sheetPlacementViewModel?.RaiseDrawingContext();
      }
    }

    private void InitialiseDrawingContext(object sender)
    {
      if (lastActiveViewModel != null)
      {
        lastActiveViewModel.PropertyChanged -= this.ActiveViewModel_PropertyChanged;
        lastActiveViewModel = null;
      }

      if (sender is IMainViewModel mainViewModel)
      {
        this.ZoomDrawingContext.Clear();

        if (mainViewModel.ActiveDocument is ISheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          lastActiveViewModel = sheetPlacementViewModel;
          this.ZoomDrawingContext.For(sheetPlacement);
        }
        else if (mainViewModel.ActiveDocument is INestProjectViewModel nestProjectViewModel)
        {
          lastActiveViewModel = nestProjectViewModel;
          if (nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
          {
            Set(detailLoadInfo);
          }
        }
        else if (mainViewModel.ActiveDocument is PartEditorViewModel partViewModel)
        {
          lastActiveViewModel = partViewModel;
          if (partViewModel.Part is ObservableNfp nfp)
          {
            this.ZoomDrawingContext.For(nfp);
          }
        }
        else if (mainViewModel.ActiveDocument is NestResultViewModel nestResultViewModel)
        {
          lastActiveViewModel = nestResultViewModel;
          if (nestResultViewModel.SelectedItem is ObservableSheetPlacement nestResultSheetPlacement)
          {
            this.ZoomDrawingContext.For(nestResultSheetPlacement);
          }
        }
        else if (mainViewModel.ActiveDocument is NfpCandidateListViewModel sheetNfpViewModel)
        {
          lastActiveViewModel = sheetNfpViewModel;
          if (sheetNfpViewModel.SelectedItem is INfp sheetNfpItem)
          {
            Set(sheetNfpViewModel, sheetNfpItem);
          }
        }

        OnPropertyChanged(nameof(ZoomDrawingContext));

        if (lastActiveViewModel != null)
        {
          lastActiveViewModel.PropertyChanged += this.ActiveViewModel_PropertyChanged;
        }
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object sender, EventArgs e)
    {
      InitialiseDrawingContext(sender);
      OnPropertyChanged(nameof(ActiveDocument));
    }

    private void OnFitAll()
    {
      CanvasScale = Math.Min(
        Actual?.X / (ZoomDrawingContext.Extremum(MinMax.Max, XY.X) - ZoomDrawingContext.Extremum(MinMax.Min, XY.X)) ?? 5,
        Actual?.Y / (ZoomDrawingContext.Extremum(MinMax.Max, XY.Y) - ZoomDrawingContext.Extremum(MinMax.Min, XY.Y)) ?? 5);
    }

    private void PreviewViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ZoomDrawingContext))
      {
        OnPropertyChanged(nameof(UpperBound));
        OnPropertyChanged(nameof(LowerBound));
        OnPropertyChanged(nameof(WidthBound));
        OnPropertyChanged(nameof(HeightBound));
      }
    }

    private void ActiveViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender == mainViewModel.ActiveDocument)
      {
        if (sender is ISheetPlacementViewModel sheetPlacementViewModel &&
            (e.PropertyName == nameof(ISheetPlacementViewModel.SelectedItem) ||
             e.PropertyName == nameof(ISheetPlacementViewModel.SheetPlacement)) &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          this.ZoomDrawingContext.For(sheetPlacement);
        }
        else if (sender is NestResultViewModel nestResultViewModel &&
                 e.PropertyName == nameof(NestResultViewModel.SelectedItem) &&
                 nestResultViewModel.SelectedItem is ObservableSheetPlacement nestResultSheetPlacement)
        {
          this.ZoomDrawingContext.For(nestResultSheetPlacement);
        }
        else if (sender is NestProjectViewModel nestProjectViewModel &&
                 e.PropertyName == nameof(NestProjectViewModel.SelectedDetailLoadInfo) &&
                 nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
        {
          Set(detailLoadInfo);
          this.IsSelected = true;
        }
        else if (sender is PartEditorViewModel partViewModel &&
                 e.PropertyName == nameof(PartEditorViewModel.Part) &&
                 partViewModel.Part is ObservableNfp nfp)
        {
          this.ZoomDrawingContext.For(nfp);
        }
        else if (sender is NfpCandidateListViewModel sheetNfpViewModel &&
                 e.PropertyName == nameof(NfpCandidateListViewModel.SelectedItem) &&
                 sheetNfpViewModel.SelectedItem is INfp sheetNfpItem)
        {
          Set(sheetNfpViewModel, sheetNfpItem);
        }

        OnPropertyChanged(nameof(ZoomDrawingContext));
      }
    }

    /// <summary>
    /// Clear DrawingContext and set for Sheet with Part as a Frame including Point of origin.
    /// </summary>
    /// <param name="sheetNfpViewModel"></param>
    /// <param name="sheetNfpItem"></param>
    private void Set(NfpCandidateListViewModel sheetNfpViewModel, INfp sheetNfpItem)
    {
      var sheet = new Sheet(sheetNfpViewModel.NfpCandidateList?.Sheet, WithChildren.Included);
      sheet.Children.Add(sheetNfpItem);
      var part = new NoFitPolygon(sheetNfpViewModel.NfpCandidateList?.Part, WithChildren.Included);
      this.ZoomDrawingContext.For(new ObservableNfp(sheet));
      this.ZoomDrawingContext.AppendChild(new ObservableFrame(part));
      this.ZoomDrawingContext.AppendChild(new ObservablePoint(part));
    }

    /// <summary>
    /// Clear DrawingContext and set for Part as a Frame including Point of origin.
    /// </summary>
    private void Set(ObservableDetailLoadInfo item)
    {
      var polygon = item.LoadAsync().Result;
      var shiftedPart = polygon.Shift(-polygon.MinX, -polygon.MinY);
      this.ZoomDrawingContext.For(new ObservableNfp(shiftedPart));
    }
  }
}
