namespace DeepNestSharp.Ui.ViewModels
{
  using System;
  using DeepNestLib;
  using DeepNestLib.IO;

  internal class ProgressDisplayer : ProgressDisplayerBase, IProgressDisplayer
  {
    private readonly IMessageService messageService;
    private readonly IDispatcherService dispatcherService;
    private readonly NestMonitorViewModel nestMonitorViewModel;

    public ProgressDisplayer(NestMonitorViewModel nestMonitorViewModel, IMessageService messageService, IDispatcherService dispatcherService)
      : base(() => nestMonitorViewModel.State)
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
      else
      {
        messageService.DisplayMessageBox(text, caption, icon);
      }
    }

    public override void DisplayProgress(ProgressBar progressBar, double percentageComplete)
    {
      if (dispatcherService.InvokeRequired)
      {
        if (progressBar == ProgressBar.Primary)
        {
          var frame = new System.Diagnostics.StackTrace().GetFrame(2);
          System.Diagnostics.Debug.Print($"{progressBar} {percentageComplete:0.0} {frame.GetMethod().DeclaringType.FullName}.{frame.GetMethod().Name} {frame.GetFileLineNumber()}");
        }

        dispatcherService.Invoke(() => DisplayProgress(progressBar, percentageComplete));
      }
      else
      {
        switch (progressBar)
        {
          case ProgressBar.Primary:
          default:
            nestMonitorViewModel.Progress = percentageComplete;
            break;
          case ProgressBar.Secondary:
            nestMonitorViewModel.ProgressSecondary = percentageComplete;
            break;
        }
      }
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      DisplayProgress(ProgressBar.Primary, CalculatePercentageComplete(placedParts, currentPopulation, SvgNest.Config.PopulationSize, nestMonitorViewModel.TopNestResults.Top.TotalPartsCount));
    }

    public override void DisplayTransientMessage(string message)
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => DisplayTransientMessage(message));
      }
      else
      {
        if (!string.IsNullOrWhiteSpace(message))
        {
          nestMonitorViewModel.LastLogMessage = message;
          nestMonitorViewModel.MessageLogBuilder.AppendLine(message);
        }
      }
    }

    public void InitialiseUiForStartNest()
    {
      // NOP
    }

    public override void SetIsVisibleSecondaryProgressBar(bool isVisible)
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => SetIsVisibleSecondaryProgressBar(isVisible));
      }
      else
      {
        nestMonitorViewModel.IsSecondaryProgressVisible = isVisible;
      }
    }

    public void UpdateNestsList()
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => UpdateNestsList());
      }
      else
      {
        nestMonitorViewModel.UpdateNestsList();
      }
    }
  }
}