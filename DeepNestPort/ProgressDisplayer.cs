namespace DeepNestPort
{
  using System;
  using DeepNestLib;
  using DeepNestLib.IO;

  public class ProgressDisplayer : ProgressDisplayerBase, IProgressDisplayer
  {
    private readonly Form1 form;
    private readonly Action initialiseUiForStartNest;
    private readonly IMessageService messageService;

    public ProgressDisplayer(Form1 form, Action initialiseUiForStartNest, IMessageService messageService)
    {
      this.form = form;
      this.initialiseUiForStartNest = initialiseUiForStartNest;
      this.messageService = messageService;
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      var percentageComplete = ProgressDisplayerHelper.CalculatePercentageComplete(placedParts, currentPopulation, SvgNest.Config.PopulationSize, this.form.Context.Polygons.Count);
      this.form.DisplayProgress(percentageComplete);
    }

    public override void DisplayProgress(double percentageComplete)
    {
      this.form.DisplayProgress(percentageComplete);
    }

    public override void DisplayTransientMessage(string message)
    {
      this.form.ToolStripMessage = message;
    }

    public void DisplayMessageBox(string text, string caption, DeepNestLib.MessageBoxIcon icon)
    {
      messageService.DisplayMessageBox(text, caption, icon);
    }

    public void UpdateNestsList()
    {
      this.form.UpdateNestsList();
    }

    public void InitialiseUiForStartNest()
    {
      this.initialiseUiForStartNest();
    }
  }
}
