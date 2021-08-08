namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.NestProject;

  public interface INfp : IMinMaxXY
  {
    /// <summary>
    /// The gross outer area, not discounting for any holes.
    /// </summary>
    double Area { get; }

    IList<INfp> Children { get; set; }

    bool Fitted { get; }

    /// <summary>
    /// Overall height.
    /// </summary>
    double HeightCalculated { get; }

    int Id { get; set; }

    /// <summary>
    /// Gets whether every point in the polygon is exact, true to the original import.
    /// </summary>
    bool IsExact { get; }

    bool IsPriority { get; set; }

    /// <summary>
    /// Gets the length of the Points collection.
    /// </summary>
    int Length { get; }

    string Name { get; set; }

    double? Offsetx { get; set; }

    double? Offsety { get; set; }

    int PlacementOrder { get; set; }

    SvgPoint[] Points { get; }

    double Rotation { get; set; }

    INfp Sheet { get; set; }

    int Source { get; set; }

    AnglesEnum StrictAngle { get; set; }

    double WidthCalculated { get; }

    double X { get; set; }

    double Y { get; set; }

    SvgPoint this[int ind] { get; }

    void AddPoint(SvgPoint point);

    NFP Clone();

    /// <summary>
    /// Clone but only copy exact points.
    /// </summary>
    /// <returns>A clone.</returns>
    NFP CloneExact();

    NFP CloneTree();

    /// <summary>
    /// Clones but only the top level points; no children.
    /// </summary>
    /// <returns>A clone.</returns>
    INfp CloneTop();

    NFP GetHull();

    /// <summary>
    /// Replace the points of the current part with those passed in.
    /// </summary>
    /// <param name="points">Points to set.</param>
    void ReplacePoints(IEnumerable<SvgPoint> points);

    /// <summary>
    /// Replace the points of the current part and it's children with those passed in.
    /// </summary>
    /// <param name="replacementNfp">Nfp with points to set.</param>
    void ReplacePoints(INfp replacementNfp);

    /// <summary>
    /// Reverses the sequence of points.
    /// </summary>
    void Reverse();

    /// <summary>
    /// Rotate the part by the specified amount.
    /// </summary>
    /// <param name="degrees">The amount to rotate, +ve clockwise, -ve anti-clockwise.</param>
    /// <returns>A clone of the original, rotated.</returns>
    NFP Rotate(double degrees);

    INfp Slice(int v);

    string Stringify();

    string ToJson();

    string ToString();

    string ToShortString();

    string ToOpenScadPolygon();
  }
}