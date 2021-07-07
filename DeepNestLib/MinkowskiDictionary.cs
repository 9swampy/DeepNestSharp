namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;

  public class MinkowskiDictionary : Dictionary<MinkowskiKey, NFP>
  {
    public MinkowskiDictionary()
      : base(new MinkowskiEqualityComparer())
    {
    }
  }
}
