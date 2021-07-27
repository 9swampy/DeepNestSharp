namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Media;

  public class ZoomOnMouseWheel : Behavior<FrameworkElement>
  {
    private Transform? transform;

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
    }

    protected override void OnDetaching()
    {
      this.AssociatedObject.MouseWheel -= this.AssociatedObject_MouseWheel;
    }

    private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (this.transform == null || !(this.transform is MatrixTransform matrixTransform))
      {
        return;
      }

      if ((this.ModifierKey.HasValue && Keyboard.IsKeyDown(this.ModifierKey.Value)) || !this.ModifierKey.HasValue)
      {
        var pos1 = e.GetPosition(this.AssociatedObject);
        var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;
        var mat = matrixTransform.Matrix;
        mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
        matrixTransform.Matrix = mat;
        e.Handled = true;
      }
    }
  }
}