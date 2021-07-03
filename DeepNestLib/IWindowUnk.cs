namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface IWindowUnk : IDbCache
  {
    Dictionary<string, List<NFP>> nfpCache { get; }
  }
}