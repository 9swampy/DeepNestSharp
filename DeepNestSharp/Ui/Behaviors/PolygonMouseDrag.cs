namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Interactivity;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.ViewModels;

  public class PolygonMouseDrag : Behavior<FrameworkElement>
  {
    private IMainViewModel? mainViewModel;
    private ObservablePartPlacement? partPlacement;

    protected override void OnAttached()
    {
      base.OnAttached();
      if (this.AssociatedObject.DataContext is ObservablePartPlacement partPlacement &&
          this.AssociatedObject.GetVisualParent<Window>() is Window window &&
          window.DataContext is IMainViewModel mainViewModel)
      {
        this.mainViewModel = mainViewModel;
        this.partPlacement = partPlacement;
      }
    }
  }
}