namespace DeepNestSharp.Ui.UserControls
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Shapes;
  using DeepNestSharp.Ui.Behaviors;
  using DeepNestSharp.Ui.Models;
  using DeepNestSharp.Ui.ViewModels;

  /// <summary>
  /// Interaction logic for Preview.xaml.
  /// </summary>
  public partial class Preview : UserControl
  {
    private Point partPlacementStartPos;
    private ObservablePartPlacement capturePartPlacement;
    private Polygon capturePolygon;

    public Preview() => InitializeComponent();

    private static bool IsDragModifierPressed
    {
      get
      {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
      }
    }

    private void Polygon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        if (sender is Polygon polygon &&
            polygon.GetVisualParent<Grid>() is Grid grid &&
            DataContext is PreviewViewModel vm &&
            polygon.DataContext is ObservablePartPlacement partPlacement)
        {
          vm.SelectedPartPlacement = partPlacement;
          if (IsDragModifierPressed)
          {
            System.Diagnostics.Debug.WriteLine($"partPlacement:{partPlacement.Id}");
            vm.DragStart = e.GetPosition(grid);
            partPlacementStartPos = new Point(vm.SelectedPartPlacement.X, vm.SelectedPartPlacement.Y);
            System.Diagnostics.Debug.Print($"Drag start set@{vm.DragStart?.X},{vm.DragStart?.Y}. {vm.IsDragging}");
            capturePartPlacement = partPlacement;
            capturePolygon = polygon;
            polygon.CaptureMouse();
            e.Handled = true;
          }
        }
      }
      catch (System.Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex);
        throw;
      }
    }

    private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
    {
      if (sender is ItemsControl itemsControl &&
          DataContext is PreviewViewModel vm)
      {
        vm.MousePosition = e.GetPosition(itemsControl);
        if (vm.IsDragging && vm.DragStart != null)
        {
          if (IsDragModifierPressed)
          {
            var dragStart = vm.DragStart.Value;
            vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / vm.CanvasScale, (vm.MousePosition.Y - dragStart.Y) / vm.CanvasScale);
            System.Diagnostics.Debug.Print($"DragOffset={vm.DragOffset:N2}");
            capturePartPlacement.X = partPlacementStartPos.X + vm.DragOffset.X;
            capturePartPlacement.Y = partPlacementStartPos.Y + vm.DragOffset.Y;
            this.InvalidateArrange();
          }
          else
          {
            System.Diagnostics.Debug.Print("Drag cancel MouseMove:IsDragModifierPressed.");
            capturePartPlacement.X = partPlacementStartPos.X;
            capturePartPlacement.Y = partPlacementStartPos.Y;
            capturePolygon.ReleaseMouseCapture();
            vm.DragStart = null;
          }
        }
      }
    }

    private void ItemsControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("ItemsControl_MouseUp");
      if (DataContext is PreviewViewModel vm &&
          vm.IsDragging)
      {
        if (IsDragModifierPressed && vm.DragStart.HasValue)
        {
          var dragStart = vm.DragStart.Value;
          vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / vm.CanvasScale, (vm.MousePosition.Y - dragStart.Y) / vm.CanvasScale);
          System.Diagnostics.Debug.Print($"Do drag commit@{vm.DragOffset.X:N2},{vm.DragOffset.Y:N2}");
          capturePartPlacement.X = partPlacementStartPos.X + vm.DragOffset.X;
          capturePartPlacement.Y = partPlacementStartPos.Y + vm.DragOffset.Y;
          capturePolygon.ReleaseMouseCapture();
          vm.DragStart = null;
        }
        else
        {
          System.Diagnostics.Debug.Print("Drag cancel MouseUp:IsDragModifierPressed.");
          capturePolygon.ReleaseMouseCapture();
          vm.DragStart = null;
        }

        this.InvalidateVisual();
      }
    }
  }
}
