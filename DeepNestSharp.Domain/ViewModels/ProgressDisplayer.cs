namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;
  using DeepNestSharp.Domain.ViewModels;

  public class ProgressDisplayer : ProgressDisplayerBase, IProgressDisplayer
  {
    private readonly IMessageService messageService;
    private readonly IDispatcherService dispatcherService;
    private readonly INestMonitorViewModel nestMonitorViewModel;

    public ProgressDisplayer(INestMonitorViewModel nestMonitorViewModel, IMessageService messageService, IDispatcherService dispatcherService)
      : base(() => nestMonitorViewModel.State)
    {
      this.messageService = messageService;
      this.dispatcherService = dispatcherService;
      this.nestMonitorViewModel = nestMonitorViewModel;
    }

    public override bool IsVisibleSecondaryProgressBar
    {
      get
      {
        return nestMonitorViewModel.IsSecondaryProgressVisible;
      }

      set
      {
        if (dispatcherService.InvokeRequired)
        {
          dispatcherService.Invoke(() => IsVisibleSecondaryProgressBar = value);
        }
        else
        {
          nestMonitorViewModel.IsSecondaryProgressVisible = value;
        }
      }
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

        if (progressBar == ProgressBar.Primary ||
            (progressBar == ProgressBar.Secondary && nestMonitorViewModel.IsSecondaryProgressVisible))
        {
          dispatcherService.Invoke(() => DisplayProgress(progressBar, percentageComplete));
        }
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

        System.Diagnostics.Debug.Print($"{progressBar} {percentageComplete}%");
      }
    }

    public void DisplayProgress(int currentPopulation, INestResult topNest)
    {
      DisplayProgress(ProgressBar.Primary, CalculatePercentageComplete(topNest));
    }

    public override void ClearTransientMessage()
    {
      if (dispatcherService.InvokeRequired)
      {
        dispatcherService.Invoke(() => ClearTransientMessage());
      }
      else
      {
        SetTransientMessage(string.Empty);
      }
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
          SetTransientMessage(message);
        }
      }
    }

    private void SetTransientMessage(string message)
    {
      nestMonitorViewModel.LastLogMessage = message;
      nestMonitorViewModel.MessageLogBuilder.AppendLine(message);
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
      else
      {
        nestMonitorViewModel.UpdateNestsList();
      }
    }
  }
}