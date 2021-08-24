namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Docking;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;

  public interface IPreviewViewModel : IToolViewModel
  {
    FileViewModel? ActiveDocument { get; }
    Point? Actual { get; }
    Canvas? Canvas { get; }
    Point CanvasOffset { get; }
    Point? CanvasPosition { get; }
    double CanvasScale { get; set; }
    double CanvasScaleMax { get; }
    double CanvasScaleMin { get; }
    Point DragOffset { get; }
    Point? DragStart { get; }
    ICommand FitAllCommand { get; }
    double HeightBound { get; }
    IPartPlacement? HoverPartPlacement { get; set; }
    bool IsDragging { get; }
    bool IsExperimental { get; set; }
    bool IsTransformSet { get; }
    Point LowerBound { get; }
    IMainViewModel MainViewModel { get; }
    Point MousePosition { get; }
    IPartPlacement? SelectedPartPlacement { get; set; }
    Transform? Transform { get; }
    Point UpperBound { get; }
    Point? Viewport { get; }
    double WidthBound { get; }
    IZoomPreviewDrawingContext ZoomDrawingContext { get; }

    double LimitAbsoluteScale(double proposed);
    double LimitScaleTransform(double proposed);
  }
}