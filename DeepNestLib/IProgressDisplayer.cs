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

    void DisplayTransientMessage(string message);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);

    void UpdateNestsList();

    void InitialiseUiForStartNest();
  }

  public class ProgressDisplayerHelper
  {
    public static double CalculatePercentageComplete(int placedParts, int currentPopulation, int populationSize, int totalPartsToPlace)
    {
      double progressPopulation = 0.66f * ((double)currentPopulation / (double)populationSize);
      double progressPlacements = 0.34f * ((double)placedParts / (double)totalPartsToPlace);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }
  }
}