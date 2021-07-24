namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface IWindowUnk : IDbCache
  {
    Dictionary<string, List<INfp>> nfpCache { get; }
  }
}