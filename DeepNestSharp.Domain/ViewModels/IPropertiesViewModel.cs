namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestSharp.Ui.Docking;

  public interface IPropertiesViewModel : IToolViewModel
  {
    object? SelectedObject { get; set; }
  }
}