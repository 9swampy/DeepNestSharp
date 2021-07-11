namespace DeepNestLib
{
  using System.Collections.Generic;

  public interface INfp
  {
    SvgPoint this[int ind] { get; }

    float Area { get; }

    IList<NFP> Children { get; }

    bool Fitted { get; }

    bool ForceRotations { get; set; }

    double HeightCalculated { get; }

    int Id { get; set; }

    bool IsPriority { get; set; }

    int Length { get; }

    string Name { get; set; }

    int PlacementOrder { get; set; }

    SvgPoint[] Points { get; }

    float Rotation { get; }

    float[] Rotations { get; }

    NFP Sheet { get; set; }

    int Source { get; }

    AnglesEnum StrictAngle { get; set; }

    double WidthCalculated { get; }

    double x { get; set; }

    double y { get; set; }

    void AddPoint(SvgPoint point);

    NFP Clone();

    NFP CloneExact();

    NFP CloneTree();

    NFP GetHull();

    SvgPoint[] ReplacePoints(IEnumerable<SvgPoint> points);

    void reverse();

    NFP Rotate(float degrees);

    NFP slice(int v);

    string stringify();

    string ToJson();

    string ToString();
  }
}