namespace DeepNestLib.CiTests
{
  using System.Threading;

  public class ProgressTestResponse : IProgressDisplayer
  {
    public AutoResetEvent Are { get; } = new AutoResetEvent(false);

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      Are.Set();
    }

    public void DisplayProgress(ProgressBar progressBar, double percentageComplete)
    {
      Are.Set();
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      Are.Set();
    }

    public void DisplayTransientMessage(string message)
    {
      Are.Set();
    }

    public void IncrementLoopProgress(ProgressBar progressBar)
    {
      Are.Set();
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, string transientMessage, int loopMax)
    {
      Are.Set();
    }

    public void InitialiseUiForStartNest()
    {
      throw new System.NotImplementedException();
    }

    public void SetIsVisibleSecondaryProgressBar(bool isVisible)
    {
      Are.Set();
    }

    public void UpdateNestsList()
    {
      Are.Set();
    }
  }
}
