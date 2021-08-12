namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using System.Reflection;
  using DeepNestLib;
  using DeepNestLib.IO;

  internal class ProgressDisplayer : ProgressDisplayerBase, IProgressDisplayer
  {
    private readonly MainViewModel mainViewModel;
    private readonly IMessageService messageService;
    private readonly IDispatcherService dispatcherService;
    private readonly NestMonitorViewModel nestMonitorViewModel;
    
    public ProgressDisplayer(NestMonitorViewModel nestMonitorViewModel, IMessageService messageService, IDispatcherService dispatcherService, MainViewModel mainViewModel)
    {
      this.mainViewModel = mainViewModel;
      this.messageService = messageService;
      this.dispatcherService = dispatcherService;
      this.nestMonitorViewModel = nestMonitorViewModel;
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => DisplayMessageBox(text, caption, icon));
      }

      messageService.DisplayMessageBox(text, caption, icon);
    }

    public override void DisplayProgress(double percentageComplete)
    {
      nestMonitorViewModel.Progress = percentageComplete;
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      DisplayProgress(ProgressDisplayerHelper.CalculatePercentageComplete(placedParts, currentPopulation, SvgNest.Config.PopulationSize, nestMonitorViewModel.TopNestResults.Top.TotalPartsCount));
    }

    public override void DisplayTransientMessage(string message)
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => DisplayTransientMessage(message));
      }

      if (!string.IsNullOrWhiteSpace(message))
      {
        System.Diagnostics.Debug.WriteLine(message);
        nestMonitorViewModel.LastLogMessage = message;
        nestMonitorViewModel.MessageLogBuilder.AppendLine(message);
      }
    }

    public void InitialiseUiForStartNest()
    {
      // NOP
    }

    public void UpdateNestsList()
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => UpdateNestsList());
      }

      nestMonitorViewModel.UpdateNestsList();
    }
  }
}