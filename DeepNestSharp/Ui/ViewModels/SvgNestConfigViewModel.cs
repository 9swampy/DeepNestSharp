namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Ui.Docking;
  using DeepNestSharp.Ui.Models;

  public class SvgNestConfigViewModel : ToolViewModel
  {
    private readonly MainViewModel mainViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="SvgNestConfigViewModel"/> class.
    /// </summary>
    /// <param name="mainViewModel">MainViewModel singleton; the primary context; access this via the activeDocument property.</param>
    public SvgNestConfigViewModel(MainViewModel mainViewModel)
      : base("Settings")
    {
      this.mainViewModel = mainViewModel;
    }

    public ISvgNestConfig SvgNestConfig { get; } = new ObservableSvgNestConfig(SvgNest.Config);
  }
}