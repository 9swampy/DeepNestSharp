namespace DeepNestLib
{
  public interface INfpCandidateList
  {
    INfp Part { get; }

    ISheet Sheet { get; }

    int NumberOfNfps { get; }

    string ToJson();
  }
}