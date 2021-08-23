namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.Placement;

  public interface INfp : IMinMaxXY, IPolygon, IPlacement
  {
    /// <summary>
    /// Gets the gross outer area, not discounting for any holes.
    /// </summary>
    double Area { get; }

    bool Fitted { get; }

    /// <summary>
    /// Gets overall height.
    /// </summary>
    double HeightCalculated { get; }

    /// <summary>
    /// Gets the length of the Points collection.
    /// </summary>
    int Length { get; }

    double WidthCalculated { get; }

    NFP Clone();

    /// <summary>
    /// Clone but only copy exact points.
    /// </summary>
    /// <returns>A clone.</returns>
    NFP CloneExact();

    INfp CloneTree();

    /// <summary>
    /// Clones but only the top level points; no children.
    /// </summary>
    /// <returns>A clone.</returns>
    INfp CloneTop();

    INfp ShiftToOrigin();

    NFP GetHull();

    /// <summary>
    /// Shifts the polygon and all it's children iteratively by the specified PartPlacement(X,Y) offset.
    /// </summary>
    /// <param name="shift">The amount to shift.</param>
    /// <returns>A partial clone of the polygon.</returns>
    INfp Shift(IPartPlacement shift);

    /// <summary>
    /// Shifts the polygon and all it's children iteratively by the specified X,Y offset.
    /// </summary>
    /// <param name="x">Distance to shift on X axis.</param>
    /// <param name="y">Distance to shift on Y axis.</param>
    /// <returns>A partial clone of the polygon.</returns>
    INfp Shift(double x, double y);

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
    INfp Rotate(double degrees, WithChildren withChildren = WithChildren.Included);

    INfp Slice(int v);

    string Stringify();

    string ToJson();

    string ToString();

    string ToShortString();

    string ToOpenScadPolygon();
  }
}