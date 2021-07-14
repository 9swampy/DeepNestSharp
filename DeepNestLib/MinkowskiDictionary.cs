namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;

  public class MinkowskiDictionary : Dictionary<MinkowskiKey, INfp>
  {
    public MinkowskiDictionary()
      : base(new MinkowskiEqualityComparer())
    {
    }
  }
}
