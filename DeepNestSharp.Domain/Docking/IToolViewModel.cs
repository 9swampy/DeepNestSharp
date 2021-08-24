namespace DeepNestSharp.Ui.Docking
{
  public interface IToolViewModel
  {
    bool IsVisible { get; set; }
    string Name { get; }
  }
}