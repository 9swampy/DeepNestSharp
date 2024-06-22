namespace DeepNestSharp.Ui.UserControls
{
  using System;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;

  internal class HorizontalScrollHandler
  {
    internal void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ListView? listView = sender as ListView;
      if (listView is not null)
      {
        Decorator? border = VisualTreeHelper.GetChild(listView, 0) as Decorator;
        if (border is not null)
        {
          ScrollViewer? scrollViewer = border.Child as ScrollViewer;
          var shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
          if (scrollViewer is not null && shiftPressed)
          {
            var divider = Constants.HorizontalScrollDivider;
            var offset = scrollViewer.HorizontalOffset;
            var delta = e.Delta;
            offset += delta / divider;
            scrollViewer.ScrollToHorizontalOffset(offset);
            e.Handled = true;
          }
        }
      }
    }
  }
}
