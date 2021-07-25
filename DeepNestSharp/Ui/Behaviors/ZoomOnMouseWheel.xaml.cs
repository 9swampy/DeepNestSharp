namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Media;

  public class PanZoomOnMouseWheel : Behavior<FrameworkElement>
  {
    private Transform? transform;
    private bool isDragging;
    private Point pos;

    public Key? ModifierKey { get; set; } = null;

    public TransformMode TranformMode { get; set; } = TransformMode.Render;

    protected override void OnAttached()
    {
      if (this.TranformMode == TransformMode.Render)
      {
        this.transform = this.AssociatedObject.RenderTransform = new MatrixTransform();
      }
      else
      {
        this.transform = this.AssociatedObject.LayoutTransform = new MatrixTransform();
      }

      this.AssociatedObject.MouseWheel += this.AssociatedObject_MouseWheel;
      this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
      this.AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
      this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.MouseWheel -= this.AssociatedObject_MouseWheel;
      this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
      this.AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
    }

    private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (this.transform == null || !(this.transform is MatrixTransform matrixTransform))
      {
        return;
      }

      if ((this.ModifierKey.HasValue && Keyboard.IsKeyDown(this.ModifierKey.Value)) || !this.ModifierKey.HasValue)
      {
        if (sender is Canvas canvas && GetVisualParent<Window>(canvas) is Window window)
        {
          var pos1 = e.GetPosition(canvas);
          var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;
          var mat = matrixTransform.Matrix;
          mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
          matrixTransform.Matrix = mat;
          e.Handled = true;
        }
      }
    }

    private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is Canvas canvas && GetVisualParent<Window>(canvas) is Window window)
      {
        //Mouse.Capture(canvas, CaptureMode.Element);
        canvas.CaptureMouse();
        this.pos = e.GetPosition(window);
        this.isDragging = true;
      }
    }

    public static T GetVisualParent<T>(DependencyObject element)
      where T : DependencyObject
    {
      while (element != null && !(element is T))
      {
        element = VisualTreeHelper.GetParent(element);
      }

      return (T)element;
    }

    private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (sender is Canvas canvas)
      {
        canvas.ReleaseMouseCapture();
      }

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

      if (sender is Canvas canvas && GetVisualParent<Window>(canvas) is Window window)
      {
        if (e.LeftButton == MouseButtonState.Pressed && canvas.IsMouseCaptured)
        {
          var pos = e.GetPosition(window);
          var matrix = matrixTransform.Matrix; // it's a struct
          matrix.Translate(pos.X - this.pos.X, pos.Y - this.pos.Y);
          matrixTransform.Matrix = matrix;
          this.pos = pos;
        }
      }
    }
  }
}