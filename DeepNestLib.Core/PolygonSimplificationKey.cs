namespace DeepNestLib
{
  using System;

  public class PolygonSimplificationKey : Tuple<SvgPoint[], double?, bool, bool>
  {
    public PolygonSimplificationKey(SvgPoint[] points, double? dataB, bool dataC, bool dataD)
      : base(points, dataB, dataC, dataD)
    {
    }

    public SvgPoint[] Points => this.Item1;

    public double? DataB => this.Item2;

    public bool DataC => this.Item3;

    public bool DataD => this.Item3;
  }
}
