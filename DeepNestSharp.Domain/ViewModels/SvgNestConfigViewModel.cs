namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Domain.Models;
  using DeepNestSharp.Ui.Docking;
  using System;

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

    public event EventHandler NotifyUpdatePropertyGrid;

    public ISvgNestConfig SvgNestConfig { get; }

    public void RaiseNotifyUpdatePropertyGrid()
    {
      NotifyUpdatePropertyGrid?.Invoke(this, EventArgs.Empty);
    }
  }
}