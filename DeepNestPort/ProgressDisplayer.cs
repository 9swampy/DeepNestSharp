namespace DeepNestPort
{
  using System.Windows.Forms;
  using DeepNestLib;

  public class ProgressDisplayer : IProgressDisplayer
  {
    private readonly Form1 form;

    public ProgressDisplayer(Form1 form)
    {
      this.form = form;
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      float progressPopulation = 0.66f * ((float)currentPopulation / (float)SvgNest.Config.PopulationSize);
      float progressPlacements = 0.34f * ((float)placedParts / (float)this.form.Polygons.Count);
      this.form.DisplayProgress(progressPopulation + progressPlacements);
    }

    public void DisplayProgress(float percentageComplete)
    {
      this.form.DisplayProgress(percentageComplete);
    }

    public void DisplayToolStripMessage(string message)
    {
      this.form.ToolStripMessage = message;
    }

    public void UpdateNestsList()
    {
      this.form.UpdateNestsList();
    }
  }
}
