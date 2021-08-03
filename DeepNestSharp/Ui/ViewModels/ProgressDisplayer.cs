namespace DeepNestSharp.Ui.ViewModels
{
  using DeepNestLib;
  using static System.Net.Mime.MediaTypeNames;
  using System.Drawing;

  internal class ProgressDisplayer : IProgressDisplayer
  {
    private readonly IMessageService messageService;
    private readonly NestMonitorViewModel nestMonitorViewModel;

    public ProgressDisplayer(NestMonitorViewModel nestMonitorViewModel, IMessageService messageService)
    {
      this.messageService = messageService;
      this.nestMonitorViewModel = nestMonitorViewModel;
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      messageService.DisplayMessageBox(text, caption, icon);
    }

    public void DisplayProgress(double percentageComplete)
    {
      DisplayTransientMessage(percentageComplete.ToString());
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      DisplayTransientMessage($"{placedParts}/{currentPopulation}");
    }

    public void DisplayTransientMessage(string message)
    {
      if (!string.IsNullOrWhiteSpace(message))
      {
        nestMonitorViewModel.MessageLogBuilder.AppendLine(message);
      }
    }

    public void InitialiseUiForStartNest()
    {
      // NOP
    }

    public void UpdateNestsList()
    {
      nestMonitorViewModel.UpdateNestsList();
    }
  }
}