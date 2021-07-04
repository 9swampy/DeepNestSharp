namespace DeepNestPort
{
  using System;
  using DeepNestLib;

  public partial class Form1
  {
    public class ProgressDisplayer : IProgressDisplayer
    {
      private readonly Form1 form;

      public ProgressDisplayer(Form1 form)
      {
        this.form = form;
      }

      public void DisplayProgress(int placedParts, int currentPopulation)
      {
        float progressPopulation = (0.66f * ((float)currentPopulation / (float)SvgNest.Config.PopulationSize));
        float progressPlacements = (0.34f * ((float)placedParts / (float)form.polygons.Count));
        form.displayProgress(progressPopulation + progressPlacements);
      }

      public void DisplayProgress(float percentageComplete)
      {
        form.displayProgress(percentageComplete);
      }
    }
  }
}
