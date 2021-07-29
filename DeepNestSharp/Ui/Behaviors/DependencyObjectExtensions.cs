namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Media;

  public static class DependencyObjectExtensions
  {
    public static T GetVisualParent<T>(this DependencyObject element)
        where T : DependencyObject
    {
      while (element != null && !(element is T))
      {
        element = VisualTreeHelper.GetParent(element);
      }

      return (T)element;
    }

    public static T GetChildOfType<T>(this DependencyObject depObj)
    where T : DependencyObject
    {
      if (depObj == null)
      {
        return null;
      }

      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
      {
        var child = VisualTreeHelper.GetChild(depObj, i);

        var result = (child as T) ?? GetChildOfType<T>(child);
        if (result != null)
        {
          return result;
        }
      }

      return null;
    }
  }
}