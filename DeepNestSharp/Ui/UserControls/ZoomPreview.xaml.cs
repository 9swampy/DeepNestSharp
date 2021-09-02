namespace DeepNestSharp.Ui.UserControls
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;

  /// <summary>
  /// Interaction logic for ZoomPreview.xaml.
  /// </summary>
  public partial class ZoomPreview : UserControl
  {
    private Point? lastCenterPositionOnTarget;
    private Point? lastMousePositionOnTarget;
    private Point? lastDragPoint;

    public ZoomPreview()
    {
      InitializeComponent();

      scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
      scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
      scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
      scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

      scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
      scrollViewer.MouseMove += OnMouseMove;

      slider.ValueChanged += OnSliderValueChanged;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (lastDragPoint.HasValue)
      {
        Point posNow = e.GetPosition(scrollViewer);

        double dX = posNow.X - lastDragPoint.Value.X;
        double dY = posNow.Y - lastDragPoint.Value.Y;

        lastDragPoint = posNow;

        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
      }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var mousePos = e.GetPosition(scrollViewer);
      if (CanUseScrollbars(ref mousePos))
      {
        scrollViewer.Cursor = Cursors.SizeAll;
        lastDragPoint = mousePos;
        Mouse.Capture(scrollViewer);
      }
    }

    private bool CanUseScrollbars(ref Point mousePos)
    {
      return mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      lastMousePositionOnTarget = Mouse.GetPosition(grid);

      if (e.Delta > 0)
      {
        slider.Value += 1;
      }

      if (e.Delta < 0)
      {
        slider.Value -= 1;
      }

      e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      scrollViewer.Cursor = Cursors.Arrow;
      scrollViewer.ReleaseMouseCapture();
      lastDragPoint = null;
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      scaleTransform.ScaleX = e.NewValue;
      scaleTransform.ScaleY = e.NewValue;

      var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
      lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
    }

    private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
      {
        Point? targetBefore = null;
        Point? targetNow = null;

        if (!lastMousePositionOnTarget.HasValue)
        {
          if (lastCenterPositionOnTarget.HasValue)
          {
            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);

            targetBefore = lastCenterPositionOnTarget;
            targetNow = centerOfTargetNow;
          }
        }
        else
        {
          targetBefore = lastMousePositionOnTarget;
          targetNow = Mouse.GetPosition(grid);

          lastMousePositionOnTarget = null;
        }

        if (targetBefore.HasValue && targetNow.HasValue)
        {
          var dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
          var dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

          var multiplicatorX = e.ExtentWidth / grid.Width;
          var multiplicatorY = e.ExtentHeight / grid.Height;

          var newOffsetX = scrollViewer.HorizontalOffset - (dXInTargetPixels * multiplicatorX);
          var newOffsetY = scrollViewer.VerticalOffset - (dYInTargetPixels * multiplicatorY);

          if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
          {
            return;
          }

          scrollViewer.ScrollToHorizontalOffset(newOffsetX);
          scrollViewer.ScrollToVerticalOffset(newOffsetY);
        }
      }
    }
  }
}
