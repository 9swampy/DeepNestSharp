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

    public ProgressDisplayer(Form1 form, Action initialiseUiForStartNest, IMessageService messageService, Func<INestState> stateFactory)
      : base(stateFactory)
    {
      this.form = form;
      this.initialiseUiForStartNest = initialiseUiForStartNest;
      this.messageService = messageService;
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      this.form.DisplayProgress(CalculatePercentageComplete(placedParts, currentPopulation, SvgNest.Config.PopulationSize, this.form.Context.Polygons.Count));
    }

    public override void DisplayProgress(ProgressBar progressBar, double percentageComplete)
    {
      if (progressBar == ProgressBar.Primary)
      {
        this.form.DisplayProgress(percentageComplete);
      }
    }

    public override void DisplayTransientMessage(string message)
    {
      this.form.ToolStripMessage = message;
    }

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
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

    public override void SetIsVisibleSecondaryProgressBar(bool isVisible)
    {
      // NOP.
    }
  }
}
