namespace DeepNestSharp.Ui.Behaviors
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Media;
  using DeepNestLib;
  using DeepNestSharp.Ui.ViewModels;

  public class PanZoomOnMouseWheel : Behavior<FrameworkElement>
  {
    private Transform? transform;
    private bool isDragging;
    private Point pos;
    private Grid? grid;
    private ItemsControl? itemsControl;
    private Canvas? canvas;
    private ScrollViewer? scrollViewer;
    private Point capturePoint;

    public Key? ModifierKey { get; set; } = Key.RightCtrl;

    public TransformMode TranformMode { get; set; } = TransformMode.Render;

    protected override void OnAttached()
    {
      if (this.AssociatedObject is Canvas canvas &&
          canvas.DataContext is PreviewViewModel previewViewModel &&
          canvas.GetVisualParent<Grid>() is Grid grid &&
          canvas.GetVisualParent<ItemsControl>() is ItemsControl itemsControl &&
          canvas.GetVisualParent<ScrollViewer>() is ScrollViewer scrollViewer)
      {
        this.grid = grid;
        this.itemsControl = itemsControl;
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

        this.grid.MouseWheel += AssociatedObject_MouseWheel;
        this.grid.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        this.grid.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        this.grid.MouseMove += AssociatedObject_MouseMove;
        this.grid.MouseEnter += this.Grid_MouseEnter;
      }
      else
      {
        throw new InvalidOperationException($"Failed to attach {nameof(PanZoomOnMouseWheel)}");
      }
    }

    protected override void OnDetaching()
    {
      if (this.grid != null)
      {
        this.grid.MouseWheel -= AssociatedObject_MouseWheel;
        this.grid.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        this.grid.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
        this.grid.MouseMove -= AssociatedObject_MouseMove;
      }
    }

    private void Grid_MouseEnter(object sender, MouseEventArgs e)
    {
      this.grid?.Focus();
    }

    private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (this.transform == null || !(this.transform is MatrixTransform matrixTransform))
      {
        return;
      }

      if (((this.ModifierKey.HasValue && Keyboard.IsKeyDown(this.ModifierKey.Value)) || !this.ModifierKey.HasValue) &&
            this.itemsControl != null &&
            this.itemsControl.DataContext is PreviewViewModel previewViewModel)
      {
        canvas?.CaptureMouse();
        var pos1 = e.GetPosition(grid);
        var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;
        var mat = matrixTransform.Matrix;
        scale = previewViewModel.LimitScaleTransform(scale);
        mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
        matrixTransform.Matrix = mat;

        previewViewModel.CanvasScale = mat.M11;
        previewViewModel.CanvasOffset = new SvgPoint(mat.OffsetX, mat.OffsetY);

        // https://stackoverflow.com/questions/26140303/wpf-zoom-scrollbar/26141271
        e.Handled = true;
      }
    }

    private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      canvas?.CaptureMouse();
      this.pos = e.GetPosition(grid);
      this.isDragging = true;
    }

    private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      canvas?.ReleaseMouseCapture();
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
        if (e.LeftButton == MouseButtonState.Pressed &&
            this.canvas != null &&
            canvas.IsMouseCaptured)
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
      capturePoint = e.MouseDevice.GetPosition(scrollViewer);
    }

    private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
    {
      Point currentPoint = e.MouseDevice.GetPosition(scrollViewer);
      var deltaX = capturePoint.X - currentPoint.X;
      var deltaY = capturePoint.Y - currentPoint.Y;
      scrollViewer?.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + deltaX);
      scrollViewer?.ScrollToVerticalOffset(scrollViewer.VerticalOffset + deltaY);
    }
  }
}