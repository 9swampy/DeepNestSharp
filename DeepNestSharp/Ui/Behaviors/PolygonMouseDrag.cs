namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Shapes;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Models;
  using DeepNestSharp.Ui.ViewModels;

  public class PolygonMouseDrag : Behavior<FrameworkElement>
  {
    private MainViewModel? mainViewModel;
    private ObservablePartPlacement? partPlacement;

    protected override void OnAttached()
    {
      base.OnAttached();
      if (this.AssociatedObject.DataContext is ObservablePartPlacement partPlacement &&
          this.AssociatedObject.GetVisualParent<Window>() is Window window &&
          window.DataContext is MainViewModel mainViewModel)
      {
        this.mainViewModel = mainViewModel;
        this.partPlacement = partPlacement;
      }
    }
  }
}