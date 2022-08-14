namespace DeepNestLib
{
  using System.Threading.Tasks;
  using DeepNestLib.Placement;

  public interface IProgressDisplayer
  {
    bool IsVisibleSecondaryProgressBar { get; set; }

    /// <summary>
    /// DisplayProgress on a percentage scale.
    /// </summary>
    /// <param name="progressBar">The progressBar to update.</param>
    /// <param name="percentageComplete">A number bettwen 0 (0%) and 1 (100%).</param>
    void DisplayProgress(ProgressBar progressBar, double percentageComplete);

    void DisplayProgress(int currentPopulation, INestResult topNest);

    void ClearTransientMessage();

    void DisplayTransientMessage(string message);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);

    void UpdateNestsList();

    void InitialiseUiForStartNest();

    void IncrementLoopProgress(ProgressBar progressBar);

    void InitialiseLoopProgress(ProgressBar progressBar, int loopMax);

    void InitialiseLoopProgress(ProgressBar progressBar, string transientMessage, int loopMax);
  }
}