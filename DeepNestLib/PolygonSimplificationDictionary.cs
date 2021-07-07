namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;

  public class PolygonSimplificationDictionary : Dictionary<PolygonSimplificationKey, SvgPoint[]>
  {
    public PolygonSimplificationDictionary()
      : base(new PolygonSimplificationEqualityComparer())
    {
    }
  }
}
