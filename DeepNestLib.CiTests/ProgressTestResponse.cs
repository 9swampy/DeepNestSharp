namespace DeepNestLib.CiTests
{
  using System.Threading;

  public class ProgressTestResponse : IProgressDisplayer
  {
    public AutoResetEvent Are { get; } = new AutoResetEvent(false);

    public void DisplayProgress(float percentageComplete)
    {
      Are.Set();
    }

    public void DisplayProgress(int placedParts, int currentPopulation)
    {
      Are.Set();
    }

    public void DisplayToolStripMessage(string message)
    {
      Are.Set();
    }

    public void UpdateNestsList()
    {
      Are.Set();
    }
  }
}
