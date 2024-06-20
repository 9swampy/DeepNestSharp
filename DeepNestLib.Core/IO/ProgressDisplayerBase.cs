namespace DeepNestLib.IO
{
  using System;
  using System.Threading.Tasks;
  using DeepNestLib.Placement;

  public abstract class ProgressDisplayerBase
  {
    private readonly Func<INestState> stateFactory;

    private double loopIndex;
    private double loopMax;

    private double loopIndexSecondary;
    private double loopMaxSecondary;
    private INestState state;

    protected ProgressDisplayerBase(Func<INestState> stateFactory)
    {
      this.stateFactory = stateFactory;
    }

    protected INestState State => this.state ?? (this.state = this.stateFactory());

    public abstract bool IsVisibleSecondaryProgressBar { get; set; }

    public async Task IncrementLoopProgress(ProgressBar progressBar)
    {
      switch (progressBar)
      {
        default:
        case ProgressBar.Primary:
          this.loopIndex++;
          this.DisplayProgress(progressBar, this.loopIndex / this.loopMax);
          break;
        case ProgressBar.Secondary:
          this.loopIndexSecondary++;
          this.DisplayProgress(progressBar, this.loopIndexSecondary / this.loopMaxSecondary);
          break;
      }
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, int loopMax)
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

      if (progressBar == ProgressBar.Secondary)
      {
        this.IsVisibleSecondaryProgressBar = true;
      }

      this.DisplayProgress(progressBar, 0);
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, string transientMessage, int loopMax)
    {
      if (this.State.AverageNestTime == 0 || this.State.AverageNestTime > 2500)
      {
        this.DisplayTransientMessage(transientMessage);
      }

      System.Diagnostics.Debug.Print(transientMessage);
      this.InitialiseLoopProgress(progressBar, loopMax);
    }

    public abstract void DisplayProgress(ProgressBar progressBar, double percentageComplete);

    public abstract void ClearTransientMessage();

    public abstract void DisplayTransientMessage(string message);

    protected internal static double CalculatePercentageComplete(int placedParts, int currentPopulation, int populationSize, int totalPartsToPlace)
    {
      double progressPopulation = 0.66f * ((double)currentPopulation / populationSize);
      double progressPlacements = 0.34f * ((double)placedParts / totalPartsToPlace);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }

    protected internal static double CalculatePercentageComplete(INestResult topNest)
    {
      double progressPopulation = 0.66f * ((double)topNest.MaterialUtilization);
      double progressPlacements = 0.34f * ((double)topNest.TotalPlacedCount / topNest.TotalPartsCount);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }
  }
}