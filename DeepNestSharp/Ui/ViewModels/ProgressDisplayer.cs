namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using DeepNestLib;

  internal class ProgressDisplayer : IProgressDisplayer
  {
    private readonly IMessageService messageService;
    private readonly IDispatcherService dispatcherService;
    private readonly NestMonitorViewModel nestMonitorViewModel;

    public ProgressDisplayer(NestMonitorViewModel nestMonitorViewModel, IMessageService messageService, IDispatcherService dispatcherService)
    {
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

    public void DisplayProgress(double percentageComplete)
    {
      nestMonitorViewModel.Progress = percentageComplete;
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      nestMonitorViewModel.Progress = (double)placedParts / currentPopulation;
    }

    public void DisplayTransientMessage(string message)
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