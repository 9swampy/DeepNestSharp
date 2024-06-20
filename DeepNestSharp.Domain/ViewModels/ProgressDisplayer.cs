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
        return this.nestMonitorViewModel.IsSecondaryProgressVisible;
      }

      set
      {
        if (this.dispatcherService.InvokeRequired)
        {
          this.dispatcherService.Invoke(() => this.IsVisibleSecondaryProgressBar = value);
        }
        else
        {
          this.nestMonitorViewModel.IsSecondaryProgressVisible = value;
        }
      }
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => this.DisplayMessageBox(text, caption, icon));
      }
      else
      {
        this.messageService.DisplayMessageBox(text, caption, icon);
      }
    }

    public override void DisplayProgress(ProgressBar progressBar, double percentageComplete)
    {
      if (this.dispatcherService.InvokeRequired)
      {
        if (progressBar == ProgressBar.Primary)
        {
          System.Diagnostics.StackFrame frame = new System.Diagnostics.StackTrace().GetFrame(2);
          System.Diagnostics.Debug.Print($"{progressBar} {percentageComplete:0.0} {frame.GetMethod().DeclaringType.FullName}.{frame.GetMethod().Name} {frame.GetFileLineNumber()}");
        }

        if (progressBar == ProgressBar.Primary ||
            (progressBar == ProgressBar.Secondary && this.nestMonitorViewModel.IsSecondaryProgressVisible))
        {
          this.dispatcherService.Invoke(() => this.DisplayProgress(progressBar, percentageComplete));
        }
      }
      else
      {
        switch (progressBar)
        {
          case ProgressBar.Primary:
          default:
            this.nestMonitorViewModel.Progress = percentageComplete;
            break;
          case ProgressBar.Secondary:
            this.nestMonitorViewModel.ProgressSecondary = percentageComplete;
            break;
        }

        System.Diagnostics.Debug.Print($"{progressBar} {percentageComplete}%");
      }
    }

    public void DisplayProgress(int currentPopulation, INestResult topNest)
    {
      this.DisplayProgress(ProgressBar.Primary, CalculatePercentageComplete(topNest));
    }

    public override void ClearTransientMessage()
    {
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => this.ClearTransientMessage());
      }
      else
      {
        this.SetTransientMessage(string.Empty);
      }
    }

    public override void DisplayTransientMessage(string message)
    {
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => this.DisplayTransientMessage(message));
      }
      else
      {
        if (!string.IsNullOrWhiteSpace(message))
        {
          this.SetTransientMessage(message);
        }
      }
    }

    private void SetTransientMessage(string message)
    {
      this.nestMonitorViewModel.LastLogMessage = message;
      this.nestMonitorViewModel.MessageLogBuilder.AppendLine(message);
    }

    public void InitialiseUiForStartNest()
    {
      // NOP
    }

    public void UpdateNestsList()
    {
      if (this.dispatcherService.InvokeRequired)
      {
        this.dispatcherService.Invoke(() => this.UpdateNestsList());
      }
      else
      {
        this.nestMonitorViewModel.UpdateNestsList();
      }
    }
  }
}