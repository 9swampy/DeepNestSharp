namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;

  public class PolygonSimplificationKey : Tuple<SvgPoint[], double?, bool, bool>
  {
    public PolygonSimplificationKey(SvgPoint[] points, double? dataB, bool dataC, bool dataD)
      : base(points, dataB, dataC, dataD)
    { }

    public SvgPoint[] Points => Item1;

    public double? DataB => Item2;

    public bool DataC => Item3;

    public bool DataD => Item3;
  }
}
