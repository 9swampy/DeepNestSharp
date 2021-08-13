namespace DeepNestLib.IO
{
  public abstract class ProgressDisplayerBase
  {
    private double loopIndex;
    private double loopMax;

    private double loopIndexSecondary;
    private double loopMaxSecondary;

    public void IncrementLoopProgress(ProgressBar progressBar)
    {
      switch (progressBar)
      {
        default:
        case ProgressBar.Primary:
          loopIndex++;
          this.DisplayProgress(progressBar, loopIndex / loopMax);
          break;
        case ProgressBar.Secondary:
          loopIndexSecondary++;
          this.DisplayProgress(progressBar, loopIndexSecondary / loopMaxSecondary);
          break;
      }
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, string transientMessage, int loopMax)
    {
      InitialiseLoopProgress(progressBar, loopMax);
      System.Diagnostics.Debug.Print(transientMessage);
      this.DisplayTransientMessage(transientMessage);
      if (progressBar == ProgressBar.Secondary)
      {
        SetIsVisibleSecondaryProgressBar(true);
      }

      this.DisplayProgress(progressBar, 0);
    }

    public abstract void SetIsVisibleSecondaryProgressBar(bool isVisible);

    public abstract void DisplayProgress(ProgressBar progressBar, double percentageComplete);

    public abstract void DisplayTransientMessage(string message);

    protected internal static double CalculatePercentageComplete(int placedParts, int currentPopulation, int populationSize, int totalPartsToPlace)
    {
      double progressPopulation = 0.66f * ((double)currentPopulation / (double)populationSize);
      double progressPlacements = 0.34f * ((double)placedParts / (double)totalPartsToPlace);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }

    private void InitialiseLoopProgress(ProgressBar progressBar, int loopMax)
    {
      switch (progressBar)
      {
        default:
        case ProgressBar.Primary:
          this.loopMax = loopMax;
          this.loopIndex = 0;
          break;
        case ProgressBar.Secondary:
          this.loopMaxSecondary = loopMax;
          this.loopIndexSecondary = 0;
          break;
      }
    }
  }
}