namespace DeepNestLib
{
  public interface INfpCandidateList
  {
    INfp[] Items { get; }

    INfp Part { get; }

    ISheet Sheet { get; }

    int NumberOfNfps { get; }

    string ToJson();
  }
}