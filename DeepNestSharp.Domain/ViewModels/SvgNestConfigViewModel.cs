namespace DeepNestSharp.Domain.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Domain.Docking;
  using DeepNestSharp.Domain.Models;
  using System;

  public class SvgNestConfigViewModel : ToolViewModel, ISvgNestConfigViewModel
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