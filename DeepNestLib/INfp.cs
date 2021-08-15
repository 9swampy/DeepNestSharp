namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;

  public interface IPolygon
  {
    IList<INfp> Children { get; set; }

    int Id { get; set; }

    /// <summary>
    /// Gets whether every point in the polygon is exact, true to the original import.
    /// </summary>
    bool IsExact { get; }

    string Name { get; set; }

    SvgPoint[] Points { get; }

    /// <summary>
    /// Gets or sets a value indicating the rotation of the part in degrees clockwise.
    /// If the number is over 360˚ then it will be reduced by 360˚ repeatedly until it is.
    /// Use the Rotate method to actually alter the part. Use of this property is inconsistent,
    /// sometimes it's used to store a desired rotation to be applied; sometimes to record the
    /// amount the part has been rotated? Needs clarification of purpose...
    /// </summary>
    double Rotation { get; set; }

    int Source { get; set; }

    SvgPoint this[int ind] { get; }

    void AddPoint(SvgPoint point);
  }

  public interface IPlacement
  {
    bool IsPriority { get; set; }

    double? Offsetx { get; set; }

    double? Offsety { get; set; }

    int PlacementOrder { get; set; }

    INfp Sheet { get; set; }

    AnglesEnum StrictAngle { get; set; }

    double X { get; set; }

    double Y { get; set; }
  }

  public interface INfp : IMinMaxXY, IPolygon, IPlacement
  {
    /// <summary>
    /// The gross outer area, not discounting for any holes.
    /// </summary>
    double Area { get; }

    bool Fitted { get; }

    /// <summary>
    /// Overall height.
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

    INfp Shift(IPartPlacement shift);

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

  public enum WithChildren
  {
    Included,
    Excluded,
  }
}