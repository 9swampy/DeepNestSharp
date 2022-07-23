namespace DeepNestLib.CiTests
{
  using DeepNestLib.Placement;
  using System.Threading;
  using System.Threading.Tasks;

  public class ProgressTestResponse : IProgressDisplayer
  {
    private bool isVisibleSecondaryProgressBar;

    public AutoResetEvent Are { get; } = new AutoResetEvent(false);

    public void DisplayMessageBox(string text, string caption, MessageBoxIcon icon)
    {
      Are.Set();
    }

    public void DisplayProgress(ProgressBar progressBar, double percentageComplete)
    {
      Are.Set();
    }

    public void DisplayProgress(int currentPopulation, INestResult top)
    {
      Are.Set();
    }

    public void ClearTransientMessage()
    {
      Are.Set();
    }

    public void DisplayTransientMessage(string message)
    {
      Are.Set();
    }

    public async Task IncrementLoopProgress(ProgressBar progressBar)
    {
      Are.Set();
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, string transientMessage, int loopMax)
    {
      Are.Set();
    }

    public void InitialiseLoopProgress(ProgressBar progressBar, int loopMax)
    {
      Are.Set();
    }

    public void InitialiseUiForStartNest()
    {
      throw new System.NotImplementedException();
    }

    public bool IsVisibleSecondaryProgressBar
    {
      get
      {
        return isVisibleSecondaryProgressBar;
      }
      set
      {
        isVisibleSecondaryProgressBar = value;
        Are.Set();
      }
    }

    public void UpdateNestsList()
    {
      Are.Set();
    }
  }
}
