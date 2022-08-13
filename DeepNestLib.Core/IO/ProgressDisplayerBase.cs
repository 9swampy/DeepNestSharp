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

    protected INestState State => state ?? (state = stateFactory());

    public abstract bool IsVisibleSecondaryProgressBar { get; set; }

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
        IsVisibleSecondaryProgressBar = true;
      }

      this.DisplayProgress(progressBar, 0);
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, string transientMessage, int loopMax)
    {
      if (State.AverageThreadNestTime == 0 || State.AverageThreadNestTime > 2500)
      {
        this.DisplayTransientMessage(transientMessage);
      }

      System.Diagnostics.Debug.Print(transientMessage);
      InitialiseLoopProgress(progressBar, loopMax);
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