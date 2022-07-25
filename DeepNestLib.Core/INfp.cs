namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.Placement;
  using IxMilia.Dxf;

  public interface INfp : IMinMaxXY, IPolygon, IPlacement
  {
    /// <summary>
    /// Gets the gross outer area, not discounting for any holes.
    /// </summary>
    double Area { get; }

    /// <summary>
    /// Gets the gross outer area, discounting for any holes.
    /// </summary>
    double NetArea { get; }

    /// <summary>
    /// Cleans the points of the parent and all children, maintaining IsClosed state.
    /// </summary>
    void Clean();

    bool Fitted { get; }

    /// <summary>
    /// Gets overall height.
    /// </summary>
    double HeightCalculated { get; }

    /// <summary>
    /// Gets a value indicating if the polygon is closed; i.e. rectangle has 5 vertices; first and last being the same.
    /// </summary>
    bool IsClosed { get; }

    /// <summary>
    /// Gets the length of the Points collection.
    /// </summary>
    int Length { get; }

    double WidthCalculated { get; }

    NoFitPolygon Clone();

    /// <summary>
    /// Clone but only copy exact points.
    /// </summary>
    /// <returns>A clone.</returns>
    NoFitPolygon CloneExact();

    INfp CloneTree();

    /// <summary>
    /// Clones but only the top level points; no children.
    /// </summary>
    /// <returns>A clone.</returns>
    INfp CloneTop();

    /// <summary>
    /// Shift the polygon to the origin.
    /// </summary>
    /// <returns>A partial clone of the polygon.</returns>
    INfp ShiftToOrigin();

    NoFitPolygon GetHull();

    /// <summary>
    /// Checks if the points collection is closed, and closes it by adding a point if needed.
    /// </summary>
    void EnsureIsClosed();

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
    /// <param name="degrees">The degrees to rotate, +ve clockwise, -ve anti-clockwise.</param>
    /// <returns>A clone of the original, rotated. The original <see cref="NoFitPolygon"/> is unaltered.</returns>
    INfp Rotate(double degrees, WithChildren withChildren = WithChildren.Included);

    /// <summary>
    /// Generates a chromosome which to separate Rotation from the Nfp. Rotation served a confused dual purpose of tracking state as well as instruction; store instruction the gene instead).
    /// </summary>
    /// <returns>A chromosome representing the part.</returns>
    Chromosome ToChromosome();

    bool Overlaps(INfp other);

    INfp Slice(int v);

    DxfFile ToDxfFile();

    string Stringify();

    string ToJson();

    string ToString();

    string ToShortString();

    string ToOpenScadPolygon();
  }
}