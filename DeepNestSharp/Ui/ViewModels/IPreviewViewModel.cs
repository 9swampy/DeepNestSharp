namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.ViewModels;
  using DeepNestSharp.Ui.Docking;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;

  public struct SimplePoint : IPointXY
  {
    public SimplePoint(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    public double X { get; }
    public double Y { get; }
  }

  public interface IPreviewViewModel : IToolViewModel
  {
    IFileViewModel? ActiveDocument { get; }
    IPointXY? Actual { get; }
    Canvas? Canvas { get; }
    IPointXY CanvasOffset { get; }
    IPointXY? CanvasPosition { get; }
    double CanvasScale { get; set; }
    double CanvasScaleMax { get; }
    double CanvasScaleMin { get; }
    IPointXY DragOffset { get; }
    IPointXY? DragStart { get; }
    ICommand FitAllCommand { get; }
    double HeightBound { get; }
    IPartPlacement? HoverPartPlacement { get; set; }
    bool IsDragging { get; }
    bool IsExperimental { get; set; }
    bool IsTransformSet { get; }
    IPointXY LowerBound { get; }
    IMainViewModel MainViewModel { get; }
    IPointXY MousePosition { get; }
    IPartPlacement? SelectedPartPlacement { get; set; }
    Transform? Transform { get; }
    IPointXY UpperBound { get; }
    IPointXY? Viewport { get; }
    double WidthBound { get; }
    IZoomPreviewDrawingContext ZoomDrawingContext { get; }

    double LimitAbsoluteScale(double proposed);
    double LimitScaleTransform(double proposed);
  }
}