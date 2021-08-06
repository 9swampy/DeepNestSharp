namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Shapes;
  using DeepNestSharp.Ui.Behaviors;
  using DeepNestSharp.Ui.Models;
  using DeepNestSharp.Ui.ViewModels;

  /// <summary>
  /// Interaction logic for ZoomPreview.xaml.
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

    private static bool IsScrollModifierPressed
    {
      get
      {
        return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
      }
    }

    private static bool IsDragModifierPressed
    {
      get
      {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
      }
    }

    private static void BringToFront(Canvas parent, Polygon polygonToMove)
    {
      try
      {
        int currentIndex = Panel.GetZIndex(polygonToMove);
        int zIndex = 0;
        int maxZ = 0;
        for (int i = 0; i < parent.Children.Count; i++)
        {
          if (parent.Children[i] is Polygon child &&
              parent.Children[i] != polygonToMove)
          {
            zIndex = Panel.GetZIndex(child);
            maxZ = Math.Max(maxZ, zIndex);
            if (zIndex > currentIndex)
            {
              Panel.SetZIndex(child, zIndex - 1);
            }
          }
        }

        Panel.SetZIndex(polygonToMove, maxZ);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Print($"{ex.Message}/n{ex.StackTrace}");
      }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (IsScrollModifierPressed)
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
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var mousePos = e.GetPosition(scrollViewer);
      if (IsScrollModifierPressed)
      {
        if (CanUseScrollbars(ref mousePos))
        {
          scrollViewer.Cursor = Cursors.SizeAll;
          lastDragPoint = mousePos;
          Mouse.Capture(scrollViewer);
        }
      }
      else if (DataContext is PreviewViewModel vm &&
            sender is ScrollViewer senderScrollViewer &&
            senderScrollViewer.InputHitTest(mousePos) is Polygon polygon &&
            polygon.GetVisualParent<Canvas>() is Canvas canvas &&
            polygon.DataContext is ObservablePartPlacement partPlacement)
      {
        vm.SelectedPartPlacement = partPlacement;
        BringToFront(canvas, polygon);
        if (IsDragModifierPressed)
        {
          vm.DragStart = mousePos;
          scrollViewer.Cursor = Cursors.Hand;
          partPlacementStartPos = new Point(vm.SelectedPartPlacement.X, vm.SelectedPartPlacement.Y);
          System.Diagnostics.Debug.Print($"Drag start set@{vm.DragStart?.X},{vm.DragStart?.Y}. {vm.IsDragging}");
          capturePartPlacement = partPlacement;
          capturePolygon = polygon;
          capturePolygon.CaptureMouse();
          e.Handled = true;
        }
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

    private void Polygon_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("Polygon_MouseUp");
      if (DataContext is PreviewViewModel vm)
      {
        vm.MousePosition = e.GetPosition(scrollViewer);
        MouseUpHandler(vm);
      }
    }

    private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
    {
      if (sender is ItemsControl itemsControl &&
          itemsControl.GetChildOfType<Canvas>() is Canvas canvas &&
          DataContext is PreviewViewModel vm)
      {
        vm.MousePosition = e.GetPosition(scrollViewer);
        vm.CanvasPosition = e.GetPosition(canvas);
        if (vm.IsDragging &&
            vm.DragStart != null &&
            capturePartPlacement != null)
        {
          if (IsDragModifierPressed)
          {
            var dragStart = vm.DragStart.Value;
            vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / scaleTransform.ScaleX, (vm.MousePosition.Y - dragStart.Y) / scaleTransform.ScaleY);

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

    private void MouseUpHandler(PreviewViewModel vm)
    {
      System.Diagnostics.Debug.Print("Handle MouseUp");
      scrollViewer.Cursor = Cursors.Arrow;
      if (vm.IsDragging && IsDragModifierPressed && vm.DragStart.HasValue)
      {
        var dragStart = vm.DragStart.Value;
        vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / scaleTransform.ScaleX, (vm.MousePosition.Y - dragStart.Y) / scaleTransform.ScaleY);
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
