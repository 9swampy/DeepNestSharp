using DeepNestLib;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

namespace DeepNestSharp.Ui.ViewModels
{
  internal class ProgressDisplayer : IProgressDisplayer
  {
    private readonly IMessageService messageService;

    public ProgressDisplayer(IMessageService messageService)
    {
      this.messageService = messageService;
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      messageService.DisplayMessageBox(text, caption, icon);
    }

    public void DisplayProgress(double percentageComplete)
    {
      messageService.DisplayMessage(percentageComplete.ToString());
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      messageService.DisplayMessage($"{placedParts}/{currentPopulation}");
    }

    public void DisplayToolStripMessage(string message)
    {
      messageService.DisplayMessage(message);
    }

    public void InitialiseUiForStartNest()
    {
      // NOP
    }

    public void UpdateNestsList()
    {
      // NOP
    }
  }
}