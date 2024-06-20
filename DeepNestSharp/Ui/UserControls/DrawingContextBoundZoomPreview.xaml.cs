namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Diagnostics;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using DeepNestLib;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Domain.ViewModels;
  using DeepNestSharp.Ui.Behaviors;

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
    private System.Windows.Shapes.Polygon? capturePolygon;

    public DrawingContextBoundZoomPreview()
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

    private static void BringToFront(Canvas parent, System.Windows.Shapes.Polygon polygonToMove)
    {
      try
      {
        int currentIndex = Panel.GetZIndex(polygonToMove);
        int zIndex = 0;
        int maxZ = 0;
        for (int i = 0; i < parent.Children.Count; i++)
        {
          if (parent.Children[i] is System.Windows.Shapes.Polygon child &&
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
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      Point mousePos = e.GetPosition(this.scrollViewer);
      if (IsScrollModifierPressed)
      {
        if (this.CanUseScrollbars(ref mousePos))
        {
          this.scrollViewer.Cursor = Cursors.SizeAll;
          this.lastDragPoint = mousePos;
          Mouse.Capture(this.scrollViewer);
        }
      }
      else if (this.DataContext is PreviewViewModel vm &&
            sender is ScrollViewer senderScrollViewer &&
            senderScrollViewer.InputHitTest(mousePos) is System.Windows.Shapes.Polygon polygon &&
            polygon.GetVisualParent<Canvas>() is Canvas canvas &&
            polygon.DataContext is ObservablePartPlacement partPlacement)
      {
        vm.SelectedPartPlacement = partPlacement;
        BringToFront(canvas, polygon);
        if (IsDragModifierPressed && vm.MainViewModel.ActiveDocument is SheetPlacementViewModel)
        {
          vm.DragStart = new SvgPoint(mousePos.X, mousePos.Y);
          this.scrollViewer.Cursor = Cursors.Hand;
          this.partPlacementStartPos = new Point(vm.SelectedPartPlacement.X, vm.SelectedPartPlacement.Y);
          Debug.Print($"Drag start set@{vm.DragStart?.X},{vm.DragStart?.Y}. {vm.IsDragging}");
          this.capturePartPlacement = partPlacement;
          partPlacement.IsDragging = true;
          this.capturePolygon = polygon;
          this.capturePolygon.CaptureMouse();
          e.Handled = true;
        }
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

    private void Polygon_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("Polygon_MouseUp");
      if (this.DataContext is PreviewViewModel vm)
      {
        Point mousePos = e.GetPosition(this.scrollViewer);
        vm.MousePosition = new SvgPoint(mousePos.X, mousePos.Y);
        this.MouseUpHandler(vm);
      }
    }

    private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
    {
      if (sender is ItemsControl itemsControl &&
          itemsControl.GetChildOfType<Canvas>() is Canvas canvas &&
          this.DataContext is PreviewViewModel vm)
      {
        Point mousePos = e.GetPosition(this.scrollViewer);
        vm.MousePosition = new SvgPoint(mousePos.X, mousePos.Y);
        Point canvasPos = e.GetPosition(canvas);
        vm.CanvasPosition = new SvgPoint(canvasPos.X, canvasPos.Y);
        if (vm.IsDragging &&
            vm.DragStart != null &&
            this.capturePartPlacement != null)
        {
          if (IsDragModifierPressed)
          {
            IPointXY dragStart = vm.DragStart;
            vm.DragOffset = new SvgPoint((vm.MousePosition.X - dragStart.X) / this.scaleTransform.ScaleX, (vm.MousePosition.Y - dragStart.Y) / this.scaleTransform.ScaleY);

            // System.Diagnostics.Debug.Print($"DragOffset={vm.DragOffset:N2}");
            this.capturePartPlacement.X = this.partPlacementStartPos.X + vm.DragOffset.X;
            this.capturePartPlacement.Y = this.partPlacementStartPos.Y + vm.DragOffset.Y;
          }
          else
          {
            System.Diagnostics.Debug.Print("Drag cancel MouseMove:IsDragModifierPressed.");
            this.capturePartPlacement.X = this.partPlacementStartPos.X;
            this.capturePartPlacement.Y = this.partPlacementStartPos.Y;
            this.capturePolygon?.ReleaseMouseCapture();
            vm.DragStart = null;
            this.capturePartPlacement.IsDragging = false;
          }

          vm.RaiseDrawingContext();
          this.InvalidateArrange();
        }
      }
    }

    private void MouseUpHandler(PreviewViewModel vm)
    {
      System.Diagnostics.Debug.Print("Handle MouseUp");
      this.scrollViewer.Cursor = Cursors.Arrow;
      if (vm.IsDragging && IsDragModifierPressed && vm.DragStart != null)
      {
        IPointXY dragStart = vm.DragStart;
        vm.DragOffset = new SvgPoint((vm.MousePosition.X - dragStart.X) / this.scaleTransform.ScaleX, (vm.MousePosition.Y - dragStart.Y) / this.scaleTransform.ScaleY);
        System.Diagnostics.Debug.Print($"Drag commit@{vm.DragOffset.X:N2},{vm.DragOffset.Y:N2}");
        if (this.capturePartPlacement != null)
        {
          this.capturePartPlacement.X = this.partPlacementStartPos.X + vm.DragOffset.X;
          this.capturePartPlacement.Y = this.partPlacementStartPos.Y + vm.DragOffset.Y;
        }
      }

      this.capturePolygon?.ReleaseMouseCapture();
      vm.DragStart = null;
      if (this.capturePartPlacement != null)
      {
        this.capturePartPlacement.IsDragging = false;
      }

      vm.RaiseSelectItem();
      vm.RaiseDrawingContext();
      System.Diagnostics.Debug.Print("Force ItemsControl.UpdateTarget");
      this.itemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();
      this.itemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateSource();
      this.InvalidateVisual();
    }
  }
}
