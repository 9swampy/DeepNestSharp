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

    public void DisplayProgress(double percentageComplete)
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

    public void InitialiseUiForStartNest()
    {
      throw new System.NotImplementedException();
    }

    public void UpdateNestsList()
    {
      Are.Set();
    }
  }
}
