namespace DeepNestSharp.Ui.Docking
{
  public class ToolViewModel : PaneViewModel, IToolViewModel
  {
    private bool isVisible = true;

    public ToolViewModel()
    { }

    public ToolViewModel(string name)
    {
      Name = name;
      Title = name;
    }

    public string Name { get; private set; }

    public bool IsVisible
    {
      get => isVisible;
      set
      {
        if (isVisible != value)
        {
          isVisible = value;
          OnPropertyChanged(nameof(IsVisible));
        }
      }
    }
  }
}
