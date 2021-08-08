namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;
  
  public class SvgNestConfigViewModel : ToolViewModel
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SvgNestConfigViewModel"/> class.
    /// </summary>
    public SvgNestConfigViewModel(ISvgNestConfig config)
      : base("Settings")
    {
      this.SvgNestConfig = new ObservableSvgNestConfig(config);
    }

    public ISvgNestConfig SvgNestConfig { get; }
  }
}