namespace DeepNestLib
{
  public interface IProgressDisplayer
  {
    /// <summary>
    /// DisplayProgress on a percentage scale.
    /// </summary>
    /// <param name="percentageComplete">A number bettwen 0 (0%) and 1 (100%).</param>
    void DisplayProgress(double percentageComplete);

    void DisplayProgress(int placedParts, int currentPopulation);

    void DisplayToolStripMessage(string message);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);

    void UpdateNestsList();

    void InitialiseUiForStartNest();
  }
}