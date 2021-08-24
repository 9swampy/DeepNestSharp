namespace DeepNestSharp.Domain.ViewModels
{
  using DeepNestSharp.Domain.Docking;

  public interface IPropertiesViewModel : IToolViewModel
  {
    object? SelectedObject { get; set; }
  }
}