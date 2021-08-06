namespace DeepNestSharp.Ui.UserControls
{
using DeepNestSharp.Ui.Behaviors;
using DeepNestSharp.Ui.Models;
using DeepNestSharp.Ui.ViewModels;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;
  using System.Windows.Documents;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using System.Windows.Navigation;
  using System.Windows.Shapes;

  /// <summary>
  /// Interaction logic for ZoomPreview.xaml
  /// </summary>
  public partial class DrawingContextBoundZoomPreview : UserControl
  {
    private Point? lastCenterPositionOnTarget;
    private Point? lastMousePositionOnTarget;
    private Point? lastDragPoint;

    private Point partPlacementStartPos;
    private ObservablePartPlacement? capturePartPlacement;
    private Polygon? capturePolygon;

    public DrawingContextBoundZoomPreview()
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

    private static bool IsDragModifierPressed
    {
      get
      {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
      }
    }

    private void Polygon_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("Polygon_MouseUp");
      if (sender is Polygon polygon &&
          polygon.GetVisualParent<ItemsControl>() is ItemsControl itemsControl &&
          DataContext is PreviewViewModel vm)
      {
        MouseUpHandler(vm, itemsControl);
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
            capturePolygon.CaptureMouse();
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
          itemsControl.GetChildOfType<Canvas>() is Canvas canvas &&
          DataContext is PreviewViewModel vm)
      {
        vm.MousePosition = e.GetPosition(itemsControl);
        vm.CanvasPosition = e.GetPosition(canvas);
        if (vm.IsDragging &&
            vm.DragStart != null &&
            capturePartPlacement != null)
        {
          if (IsDragModifierPressed)
          {
            var dragStart = vm.DragStart.Value;
            vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / vm.CanvasScale, (vm.MousePosition.Y - dragStart.Y) / vm.CanvasScale);

            // System.Diagnostics.Debug.Print($"DragOffset={vm.DragOffset:N2}");
            capturePartPlacement.X = partPlacementStartPos.X + vm.DragOffset.X;
            capturePartPlacement.Y = partPlacementStartPos.Y + vm.DragOffset.Y;
          }
          else
          {
            System.Diagnostics.Debug.Print("Drag cancel MouseMove:IsDragModifierPressed.");
            capturePartPlacement.X = partPlacementStartPos.X;
            capturePartPlacement.Y = partPlacementStartPos.Y;
            capturePolygon?.ReleaseMouseCapture();
            vm.DragStart = null;
          }

          vm.RaiseDrawingContext();
          this.InvalidateArrange();
        }
      }
    }

    private void ItemsControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("ItemsControl_MouseUp");
      if (sender is ItemsControl itemsControl &&
          DataContext is PreviewViewModel vm)
      {
        MouseUpHandler(vm, itemsControl);
      }
    }

    private void MouseUpHandler(PreviewViewModel vm, ItemsControl itemsControl)
    {
      System.Diagnostics.Debug.Print("Handle MouseUp");
      if (vm.IsDragging && IsDragModifierPressed && vm.DragStart.HasValue)
      {
        var dragStart = vm.DragStart.Value;
        vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / vm.CanvasScale, (vm.MousePosition.Y - dragStart.Y) / vm.CanvasScale);
        System.Diagnostics.Debug.Print($"Drag commit@{vm.DragOffset.X:N2},{vm.DragOffset.Y:N2}");
        if (capturePartPlacement != null)
        {
          capturePartPlacement.X = partPlacementStartPos.X + vm.DragOffset.X;
          capturePartPlacement.Y = partPlacementStartPos.Y + vm.DragOffset.Y;
        }
      }

      capturePolygon?.ReleaseMouseCapture();
      vm.DragStart = null;
      vm.RaiseSelectItem();
      vm.RaiseDrawingContext();
      System.Diagnostics.Debug.Print("Force ItemsControl.UpdateTarget");
      itemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();
      itemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateSource();
      this.InvalidateVisual();
    }
  }
}
