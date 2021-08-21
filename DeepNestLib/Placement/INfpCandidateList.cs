namespace DeepNestLib
{
  public interface INfpCandidateList
  {
    INfp Part { get; }

    ISheet Sheet { get; }

    string ToJson();
  }
}