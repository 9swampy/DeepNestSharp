namespace DeepNestSharp.Domain.Docking
{
  public class ToolViewModel : PaneViewModel, IToolViewModel
  {
    private bool isVisible = true;

    public ToolViewModel()
    { }

    public ToolViewModel(string name)
    {
      this.Name = name;
      this.Title = name;
    }

    public string Name { get; private set; }

    public bool IsVisible
    {
      get => this.isVisible;
      set
      {
        if (this.isVisible != value)
        {
          this.isVisible = value;
          this.OnPropertyChanged(nameof(this.IsVisible));
        }
      }
    }
  }
}
