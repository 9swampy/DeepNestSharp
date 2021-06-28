namespace DeepNestLib
{
  using System;
  using System.Collections;
  using System.Collections.Generic;

  public class PolygonSimplificationDictionary : Dictionary<Tuple<SvgPoint[], double?, bool, bool>, SvgPoint[]>
  {
    public PolygonSimplificationDictionary()
      : base(new PolygonSimplificationEqualityComparer())
    {
    }
  }
}
