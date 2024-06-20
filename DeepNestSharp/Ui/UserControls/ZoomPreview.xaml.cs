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
      this.InitializeComponent();

      this.scrollViewer.ScrollChanged += this.OnScrollViewerScrollChanged;
      this.scrollViewer.MouseLeftButtonUp += this.OnMouseLeftButtonUp;
      this.scrollViewer.PreviewMouseLeftButtonUp += this.OnMouseLeftButtonUp;
      this.scrollViewer.PreviewMouseWheel += this.OnPreviewMouseWheel;

      this.scrollViewer.PreviewMouseLeftButtonDown += this.OnMouseLeftButtonDown;
      this.scrollViewer.MouseMove += this.OnMouseMove;

      this.slider.ValueChanged += this.OnSliderValueChanged;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (this.lastDragPoint.HasValue)
      {
        Point posNow = e.GetPosition(this.scrollViewer);

        double dX = posNow.X - this.lastDragPoint.Value.X;
        double dY = posNow.Y - this.lastDragPoint.Value.Y;

        this.lastDragPoint = posNow;

        this.scrollViewer.ScrollToHorizontalOffset(this.scrollViewer.HorizontalOffset - dX);
        this.scrollViewer.ScrollToVerticalOffset(this.scrollViewer.VerticalOffset - dY);
      }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      Point mousePos = e.GetPosition(this.scrollViewer);
      if (this.CanUseScrollbars(ref mousePos))
      {
        this.scrollViewer.Cursor = Cursors.SizeAll;
        this.lastDragPoint = mousePos;
        Mouse.Capture(this.scrollViewer);
      }
    }

    private bool CanUseScrollbars(ref Point mousePos)
    {
      return mousePos.X <= this.scrollViewer.ViewportWidth && mousePos.Y < this.scrollViewer.ViewportHeight;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      this.lastMousePositionOnTarget = Mouse.GetPosition(this.grid);

      if (e.Delta > 0)
      {
        this.slider.Value += 0.2;
      }

      if (e.Delta < 0)
      {
        this.slider.Value -= 0.2;
      }

      e.Handled = true;
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.scrollViewer.Cursor = Cursors.Arrow;
      this.scrollViewer.ReleaseMouseCapture();
      this.lastDragPoint = null;
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      this.scaleTransform.ScaleX = e.NewValue;
      this.scaleTransform.ScaleY = e.NewValue;

      Point centerOfViewport = new Point(this.scrollViewer.ViewportWidth / 2, this.scrollViewer.ViewportHeight / 2);
      this.lastCenterPositionOnTarget = this.scrollViewer.TranslatePoint(centerOfViewport, this.grid);
    }

    private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
      {
        Point? targetBefore = null;
        Point? targetNow = null;

        if (!this.lastMousePositionOnTarget.HasValue)
        {
          if (this.lastCenterPositionOnTarget.HasValue)
          {
            Point centerOfViewport = new Point(this.scrollViewer.ViewportWidth / 2, this.scrollViewer.ViewportHeight / 2);
            Point centerOfTargetNow = this.scrollViewer.TranslatePoint(centerOfViewport, this.grid);

            targetBefore = this.lastCenterPositionOnTarget;
            targetNow = centerOfTargetNow;
          }
        }
        else
        {
          targetBefore = this.lastMousePositionOnTarget;
          targetNow = Mouse.GetPosition(this.grid);

          this.lastMousePositionOnTarget = null;
        }

        if (targetBefore.HasValue && targetNow.HasValue)
        {
          var dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
          var dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

          var multiplicatorX = e.ExtentWidth / this.grid.Width;
          var multiplicatorY = e.ExtentHeight / this.grid.Height;

          var newOffsetX = this.scrollViewer.HorizontalOffset - (dXInTargetPixels * multiplicatorX);
          var newOffsetY = this.scrollViewer.VerticalOffset - (dYInTargetPixels * multiplicatorY);

          if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
          {
            return;
          }

          this.scrollViewer.ScrollToHorizontalOffset(newOffsetX);
          this.scrollViewer.ScrollToVerticalOffset(newOffsetY);
        }
      }
    }
  }
}
