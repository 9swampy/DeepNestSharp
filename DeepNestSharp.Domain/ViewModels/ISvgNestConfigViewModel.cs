namespace DeepNestSharp.Domain.ViewModels
{
  using System;
  using DeepNestLib;
  using DeepNestSharp.Domain.Docking;

  public interface ISvgNestConfigViewModel : IToolViewModel
  {
    ISvgNestConfig SvgNestConfig { get; }

    event EventHandler NotifyUpdatePropertyGrid;

    void RaiseNotifyUpdatePropertyGrid();
  }
}