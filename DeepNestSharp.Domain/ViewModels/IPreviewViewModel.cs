namespace DeepNestSharp.Domain.ViewModels
{
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;

  public interface IPreviewViewModel : IToolViewModel
  {
    IFileViewModel ActiveDocument { get; }

    IPointXY Actual { get; }

    IPointXY CanvasPosition { get; }

    double CanvasScale { get; set; }

    double CanvasScaleMax { get; }

    double CanvasScaleMin { get; }

    IPointXY DragOffset { get; }

    IPointXY DragStart { get; }

    ICommand FitAllCommand { get; }

    double HeightBound { get; }

    IPartPlacement HoverPartPlacement { get; set; }

    bool IsDragging { get; }

    bool IsExperimental { get; set; }

    IPointXY LowerBound { get; }

    IMainViewModel MainViewModel { get; }

    IPointXY MousePosition { get; }

    IPartPlacement SelectedPartPlacement { get; set; }

    IPointXY UpperBound { get; }

    IPointXY Viewport { get; }

    double WidthBound { get; }

    IZoomPreviewDrawingContext ZoomDrawingContext { get; }

    double LimitAbsoluteScale(double proposed);

    double LimitScaleTransform(double proposed);
  }
}