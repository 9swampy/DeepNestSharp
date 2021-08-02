namespace DeepNestPort
{
  using System;
  using System.Windows.Forms;
  using DeepNestLib;

  public class ProgressDisplayer : IProgressDisplayer
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
      double progressPopulation = 0.66f * ((double)currentPopulation / (double)SvgNest.Config.PopulationSize);
      double progressPlacements = 0.34f * ((double)placedParts / (double)this.form.Polygons.Count);
      this.form.DisplayProgress(progressPopulation + progressPlacements);
    }

    public void DisplayProgress(double percentageComplete)
    {
      this.form.DisplayProgress(percentageComplete);
    }

    public void DisplayToolStripMessage(string message)
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
