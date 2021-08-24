namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Ui.Docking;
  using System;

  public interface ISvgNestConfigViewModel : IToolViewModel
  {
    ISvgNestConfig SvgNestConfig { get; }

    event EventHandler NotifyUpdatePropertyGrid;

    void RaiseNotifyUpdatePropertyGrid();
  }
}