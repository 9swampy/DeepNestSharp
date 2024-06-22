namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using System.ComponentModel;
  using System.Windows.Input;
  using CommunityToolkit.Mvvm.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Models;

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
      this.mainViewModel.ActiveDocumentChanged += this.MainViewModel_ActiveDocumentChanged;
      this.PropertyChanged += this.PreviewViewModel_PropertyChanged;
    }

    public IFileViewModel ActiveDocument => this.mainViewModel.ActiveDocument;

    public IZoomPreviewDrawingContext ZoomDrawingContext { get; } = new ZoomPreviewDrawingContext();

    public ICommand FitAllCommand
    {
      get
      {
        if (this.fitAllCommand == null)
        {
          this.fitAllCommand = new RelayCommand(this.OnFitAll);
        }

        return this.fitAllCommand;
      }
    }

    public IMainViewModel MainViewModel => this.mainViewModel;

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
          this.OnPropertyChanged(nameof(this.SelectedPartPlacement));
        }
      }
    }

    public IPartPlacement HoverPartPlacement
    {
      get => this.hoverPartPlacement;

      set
      {
        this.hoverPartPlacement = value;
        this.OnPropertyChanged(nameof(this.ZoomDrawingContext));
        this.OnPropertyChanged(nameof(this.SelectedPartPlacement));
        this.OnPropertyChanged(nameof(this.HoverPartPlacement));
      }
    }

    public IPointXY MousePosition
    {
      get => this.mousePosition;
      set
      {
        this.mousePosition = value;
        this.OnPropertyChanged(nameof(this.MousePosition));
      }
    }

    public IPointXY CanvasPosition
    {
      get => this.canvasPosition;
      set
      {
        this.SetProperty(ref this.canvasPosition, value, nameof(this.CanvasPosition));
      }
    }

    public bool IsDragging
    {
      get => this.dragStart != null;
    }

    public bool IsExperimental
    {
      get => this.isExperimental;
      set
      {
        this.SetProperty(ref this.isExperimental, value, nameof(this.IsExperimental));
        this.InitialiseDrawingContext(this.mainViewModel);
      }
    }

    public IPointXY DragOffset
    {
      get => this.dragOffset;
      set
      {
        this.SetProperty(ref this.dragOffset, value, nameof(this.DragOffset));
      }
    }

    public IPointXY DragStart
    {
      get => this.dragStart;
      set
      {
        this.SetProperty(ref this.dragStart, value, nameof(this.DragStart));
        this.OnPropertyChanged(nameof(this.IsDragging));
        this.OnPropertyChanged(nameof(this.ZoomDrawingContext));
      }
    }

    public double CanvasScale
    {
      get => this.canvasScale;
      set
      {
        this.SetProperty(ref this.canvasScale, value, nameof(this.CanvasScale));
      }
    }

    public double CanvasScaleMax => 10;

    public double CanvasScaleMin => 0.5;

    public IPointXY LowerBound
    {
      get
      {
        return new SvgPoint(this.ZoomDrawingContext.Extremum(MinMax.Min, XY.X), this.ZoomDrawingContext.Extremum(MinMax.Min, XY.Y));
      }
    }

    public IPointXY UpperBound
    {
      get
      {
        return new SvgPoint(this.ZoomDrawingContext.Extremum(MinMax.Max, XY.X), this.ZoomDrawingContext.Extremum(MinMax.Max, XY.Y));
      }
    }

    public double WidthBound
    {
      get
      {
        return this.ZoomDrawingContext.Extremum(MinMax.Max, XY.X) - this.ZoomDrawingContext.Extremum(MinMax.Min, XY.X);
      }
    }

    public double HeightBound
    {
      get
      {
        return this.ZoomDrawingContext.Extremum(MinMax.Max, XY.Y) - this.ZoomDrawingContext.Extremum(MinMax.Min, XY.Y);
      }
    }

    public IPointXY Actual
    {
      get => this.actual;
      set
      {
        this.SetProperty(ref this.actual, value, nameof(this.Actual));
      }
    }

    public IPointXY Viewport
    {
      get => this.viewport;
      set
      {
        this.SetProperty(ref this.viewport, value, nameof(this.Viewport));
      }
    }

    public double LimitAbsoluteScale(double proposed)
    {
      if (proposed > this.CanvasScaleMax)
      {
        return this.CanvasScaleMax;
      }
      else if (proposed < this.CanvasScaleMin)
      {
        return this.CanvasScaleMin;
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
      if (proposed * this.CanvasScale > this.CanvasScaleMax)
      {
        proposed = this.CanvasScaleMax / this.CanvasScale;
      }
      else if (proposed * this.CanvasScale < this.CanvasScaleMin)
      {
        proposed = this.CanvasScaleMin / this.CanvasScale;
      }

      return proposed;
    }

    public void RaiseSelectItem()
    {
      System.Diagnostics.Debug.Print("Force RaiseSelectItem");
      this.OnPropertyChanged(nameof(this.SelectedPartPlacement));
    }

    public void RaiseDrawingContext()
    {
      // System.Diagnostics.Debug.Print("Force RaiseDrawingContext");
      this.OnPropertyChanged(nameof(this.ZoomDrawingContext));
      if (this.lastActiveViewModel is ISheetPlacementViewModel sheetPlacementViewModel)
      {
        sheetPlacementViewModel?.RaiseDrawingContext();
      }
    }

    private void InitialiseDrawingContext(object sender)
    {
      if (this.lastActiveViewModel != null)
      {
        this.lastActiveViewModel.PropertyChanged -= this.ActiveViewModel_PropertyChanged; // unsubscribing ActiveViewModel_PropertyChanged from the PropertyChanged notification of lastActiveViewModel.
        this.lastActiveViewModel = null;
      }

      if (sender is IMainViewModel mainViewModel)
      {
        this.ZoomDrawingContext.Clear();

        if (mainViewModel.ActiveDocument is ISheetPlacementViewModel sheetPlacementViewModel &&
            sheetPlacementViewModel.SheetPlacement is ObservableSheetPlacement sheetPlacement)
        {
          this.lastActiveViewModel = sheetPlacementViewModel;
          this.ZoomDrawingContext.For(sheetPlacement);
        }
        else if (mainViewModel.ActiveDocument is INestProjectViewModel nestProjectViewModel)
        {
          this.lastActiveViewModel = nestProjectViewModel;
          if (nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
          {
            this.Set(detailLoadInfo);
          }
        }
        else if (mainViewModel.ActiveDocument is PartEditorViewModel partViewModel)
        {
          this.lastActiveViewModel = partViewModel;
          if (partViewModel.Part is ObservableNfp nfp)
          {
            this.ZoomDrawingContext.For(nfp);
          }
        }
        else if (mainViewModel.ActiveDocument is NestResultViewModel nestResultViewModel)
        {
          this.lastActiveViewModel = nestResultViewModel;
          if (nestResultViewModel.SelectedItem is ObservableSheetPlacement nestResultSheetPlacement)
          {
            this.ZoomDrawingContext.For(nestResultSheetPlacement);
          }
        }
        else if (mainViewModel.ActiveDocument is NfpCandidateListViewModel sheetNfpViewModel)
        {
          this.lastActiveViewModel = sheetNfpViewModel;
          if (sheetNfpViewModel.SelectedItem is INfp sheetNfpItem)
          {
            this.Set(sheetNfpViewModel, sheetNfpItem);
          }
        }

        this.OnPropertyChanged(nameof(this.ZoomDrawingContext));

        if (this.lastActiveViewModel != null)
        {
          this.lastActiveViewModel.PropertyChanged += this.ActiveViewModel_PropertyChanged;
        }
      }
    }

    private void MainViewModel_ActiveDocumentChanged(object sender, EventArgs e)
    {
      this.InitialiseDrawingContext(sender);
      this.OnPropertyChanged(nameof(this.ActiveDocument));
    }

    private void OnFitAll()
    {
      //Actual.X: The width, in pixels, of the preview graphics window
      //Actual.Y: The height, in pixels, of the preview graphics window
      //ZoomDrawingContext.Extremum(...): seems to calculate extrema of the previewed object in the physical units represented in the dxf file
      this.CanvasScale = Math.Min(
        this.Actual?.X / (this.ZoomDrawingContext.Extremum(MinMax.Max, XY.X) - this.ZoomDrawingContext.Extremum(MinMax.Min, XY.X)) ?? 5,
        this.Actual?.Y / (this.ZoomDrawingContext.Extremum(MinMax.Max, XY.Y) - this.ZoomDrawingContext.Extremum(MinMax.Min, XY.Y)) ?? 5);
    }

    private void PreviewViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(this.ZoomDrawingContext))
      {
        this.OnPropertyChanged(nameof(this.UpperBound));
        this.OnPropertyChanged(nameof(this.LowerBound));
        this.OnPropertyChanged(nameof(this.WidthBound));
        this.OnPropertyChanged(nameof(this.HeightBound));
      }
    }

    private void ActiveViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender == this.mainViewModel.ActiveDocument)
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
        else if (sender is NestProjectViewModel nestProjectViewModel)
        {
          if (e.PropertyName == nameof(NestProjectViewModel.SelectedDetailLoadInfo) &&
                 nestProjectViewModel.SelectedDetailLoadInfo is ObservableDetailLoadInfo detailLoadInfo)
          {
            this.Set(detailLoadInfo);
            this.IsSelected = true;
          }
          else if (e.PropertyName == nameof(NestProjectViewModel.SelectedSheetLoadInfo) &&
                 nestProjectViewModel.SelectedSheetLoadInfo is ObservableSheetLoadInfo sheetLoadInfo)
          {
            this.Set(sheetLoadInfo);
            this.IsSelected = true;
          }
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
          this.Set(sheetNfpViewModel, sheetNfpItem);
        }

        this.OnPropertyChanged(nameof(this.ZoomDrawingContext));
      }
    }

    /// <summary>
    /// Clear DrawingContext and set for Sheet with Part as a Frame including Point of origin.
    /// </summary>
    /// <param name="sheetNfpViewModel"></param>
    /// <param name="sheetNfpItem"></param>
    private void Set(NfpCandidateListViewModel sheetNfpViewModel, INfp sheetNfpItem)
    {
      Sheet sheet = new Sheet(sheetNfpViewModel.NfpCandidateList?.Sheet, WithChildren.Included);
      sheet.Children.Add(sheetNfpItem);
      NoFitPolygon part = new NoFitPolygon(sheetNfpViewModel.NfpCandidateList?.Part, WithChildren.Included);
      this.ZoomDrawingContext.For(new ObservableNfp(sheet));
      this.ZoomDrawingContext.AppendChild(new ObservableFrame(part));
      this.ZoomDrawingContext.AppendChild(new ObservablePoint(part));
    }

    /// <summary>
    /// Clear DrawingContext and set for Part as a Frame including Point of origin.
    /// </summary>
    private void Set(ObservableDetailLoadInfo item)
    {
      INfp polygon = item.LoadAsync().Result;
      INfp shiftedPart = polygon.ShiftToOrigin();
      this.ZoomDrawingContext.For(new ObservableNfp(shiftedPart));
    }

    /// <summary>
    /// Clear DrawingContext and set for Sheet as a Frame including Point of origin.
    /// </summary>
    private void Set(ObservableSheetLoadInfo item)
    {
      INfp polygon = item.LoadAsync().Result;
      INfp shiftedPart = polygon.ShiftToOrigin();
      this.ZoomDrawingContext.For(new ObservableNfp(shiftedPart));
    }
  }
}
