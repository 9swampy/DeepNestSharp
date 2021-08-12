namespace DeepNestLib.IO
{
  public abstract class ProgressDisplayerBase
  {
    private double loopIndex;
    private double loopMax;

    public void IncrementLoopProgress()
    {
      loopIndex++;
      this.DisplayProgress(loopIndex / loopMax);
    }

    public void InitialiseLoopProgress(string transientMessage, int loopMax)
    {
      this.loopMax = loopMax;
      this.loopIndex = 0;
      this.DisplayTransientMessage(transientMessage);
      this.DisplayProgress(0);
    }

    public abstract void DisplayProgress(double percentageComplete);

    public abstract void DisplayTransientMessage(string message);
  }
}