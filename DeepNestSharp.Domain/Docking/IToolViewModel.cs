namespace DeepNestSharp.Domain.Docking
{
  public interface IToolViewModel
  {
    bool IsActive { get; set; }

    bool IsSelected { get; set; }

    bool IsVisible { get; set; }

    string Name { get; }
  }
}