namespace DeepNestLib
{
  using System.Threading.Tasks;
  using DeepNestLib.Placement;

  public interface INestingContext : INestingContextState
  {
    void AddRectanglePart(int src, int ww = 50, int hh = 80);

    void AssignPlacement(INestResult plcpr);

    int GetNextSheetSource();

    int GetNextSource();

    void LoadSampleData();

    void LoadXml(string v);

    Task NestIterate(ISvgNestConfig config);

    void ReorderSheets();

    void Reset();

    void ResumeNest();

    Task StartNest();

    void StopNest();
  }
}