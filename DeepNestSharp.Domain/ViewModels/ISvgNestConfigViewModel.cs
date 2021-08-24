namespace DeepNestSharp.Domain.ViewModels
{
  using DeepNestLib;
  using DeepNestSharp.Domain.Docking;
  using System;

  public interface ISvgNestConfigViewModel : IToolViewModel
  {
    ISvgNestConfig SvgNestConfig { get; }

    event EventHandler NotifyUpdatePropertyGrid;

    void RaiseNotifyUpdatePropertyGrid();
  }
}