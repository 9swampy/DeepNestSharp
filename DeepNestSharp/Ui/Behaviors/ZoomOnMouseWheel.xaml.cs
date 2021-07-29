namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Media;
  using System.Windows.Shapes;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.ViewModels;

  public class PanZoomOnMouseWheel : Behavior<FrameworkElement>
  {
    private Transform? transform;
    private bool isDragging;
    private Point pos;
    private Grid grid;
    private ItemsControl itemsControl;
    private Window window;
    private Canvas canvas;
    private ScrollViewer scrollViewer;
    private Point capturePoint;

    public Key? ModifierKey { get; set; } = Key.RightCtrl;

    public TransformMode TranformMode { get; set; } = TransformMode.Render;

    protected override void OnAttached()
    {
      if (this.AssociatedObject is Canvas canvas &&
          canvas.GetVisualParent<Grid>() is Grid grid &&
          canvas.GetVisualParent<ItemsControl>() is ItemsControl itemsControl &&
          canvas.GetVisualParent<Window>() is Window window &&
          canvas.GetVisualParent<ScrollViewer>() is ScrollViewer scrollViewer)
      {
        this.grid = grid;
        this.itemsControl = itemsControl;
        this.window = window;
        this.canvas = canvas;
        this.scrollViewer = scrollViewer;

        if (this.TranformMode == TransformMode.Render)
        {
          this.transform = this.canvas.RenderTransform = new MatrixTransform();
        }
        else
        {
          this.transform = this.canvas.LayoutTransform = new MatrixTransform();
        }
      }

      this.grid.MouseWheel += AssociatedObject_MouseWheel;
      this.grid.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
      this.grid.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
      this.grid.MouseMove += AssociatedObject_MouseMove;
      this.grid.MouseEnter += this.Grid_MouseEnter;
      //this.scrollViewer.PreviewMouseLeftButtonDown += this.ScrollViewer_PreviewMouseLeftButtonDown;
      //this.scrollViewer.MouseUp += this.ScrollViewer_MouseUp;
      //this.scrollViewer.MouseMove += this.ScrollViewer_MouseMove;
    }

    private void Grid_MouseEnter(object sender, MouseEventArgs e)
    {
      this.grid.Focus();
    }

    protected override void OnDetaching()
    {
      this.grid.MouseWheel -= this.AssociatedObject_MouseWheel;
      this.grid.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
      this.grid.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
      this.grid.MouseMove -= AssociatedObject_MouseMove;
    }

    private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (this.transform == null || !(this.transform is MatrixTransform matrixTransform))
      {
        return;
      }

      if ((this.ModifierKey.HasValue && Keyboard.IsKeyDown(this.ModifierKey.Value)) || !this.ModifierKey.HasValue)
      {
        canvas.CaptureMouse();
        var pos1 = e.GetPosition(grid);
        var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;
        var mat = matrixTransform.Matrix;
        mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
        matrixTransform.Matrix = mat;

        if (itemsControl.DataContext is PreviewViewModel previewViewModel)
        {
          previewViewModel.CanvasScale = mat.M11;
          previewViewModel.CanvasOffset = new Point(mat.OffsetX, mat.OffsetY);
        }

        //https://stackoverflow.com/questions/26140303/wpf-zoom-scrollbar/26141271
        e.Handled = true;
      }
    }

    private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      canvas.CaptureMouse();
      this.pos = e.GetPosition(grid);
      this.isDragging = true;
    }

    private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      canvas.ReleaseMouseCapture();
      isDragging = false;
    }

    private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
    {
      if (isDragging == false)
      {
        return;
      }

      if (this.transform == null || !(this.transform is MatrixTransform matrixTransform))
      {
        return;
      }

      if ((this.ModifierKey.HasValue && Keyboard.IsKeyDown(this.ModifierKey.Value)) || !this.ModifierKey.HasValue)
      {
        if (e.LeftButton == MouseButtonState.Pressed && canvas.IsMouseCaptured)
        {
          var pos = e.GetPosition(grid);
          var matrix = matrixTransform.Matrix; // it's a struct
          matrix.Translate(pos.X - this.pos.X, pos.Y - this.pos.Y);
          matrixTransform.Matrix = matrix;
          this.pos = pos;
        }
      }
    }

    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      //scrollViewer.CaptureMouse();
      capturePoint = e.MouseDevice.GetPosition(scrollViewer);
    }

    private void ScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
    {
      //scrollViewer.ReleaseMouseCapture();
    }

    private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
    {
      //if (!scrollViewer.IsMouseCaptured) return;
      Point currentPoint = e.MouseDevice.GetPosition(scrollViewer);
      var deltaX = capturePoint.X - currentPoint.X;
      var deltaY = capturePoint.Y - currentPoint.Y;
      scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + deltaX);
      scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + deltaY);
    }
  }
}