namespace DeepNestLib.Placement
{
  public interface IPartPlacement : IMinMaxXY
  {
    ///// <summary>
    ///// Gets or sets a hull of the part captured only when not Gravity or BoundingBox (ie. Squeeze).
    ///// </summary>
    //INfp Hull { get; set; }

    ///// <summary>
    ///// Gets or sets a hull of the sheet captured only when not Gravity or BoundingBox (ie. Squeeze).
    ///// </summary>
    //INfp HullSheet { get; set; }

    int Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the part is currently being dragged in the UI.
    /// </summary>
    bool IsDragging { get; set; }

    /// <summary>
    /// Gets a value indicating whether every point in the polygon is exact, true to the original import.
    /// </summary>
    bool IsExact { get; }

    double? MergedLength { get; }

    object MergedSegments { get; set; }

    /// <summary>
    /// Gets a clone of the part that was placed. The clone will have been rotated (potentially by 0') but NOT shifted.
    /// </summary>
    INfp Part { get; }

    /// <summary>
    /// Gets a clone of the part that was placed. The clone will have been rotated (potentially by 0') AND shifted relative to it's sheet.
    /// </summary>
    INfp PlacedPart { get; }

    /// <summary>
    /// Gets or sets the Rotation of the part (sheets I don't think ever get rotated, so this would be absolute).
    /// </summary>
    double Rotation { get; set; }

    /// <summary>
    /// Gets or sets the Source of the part placed.
    /// </summary>
    int Source { get; set; }

    /// <summary>
    /// Gets or sets the X offset of the part relative to the sheet.
    /// </summary>
    double X { get; set; }

    /// <summary>
    /// Gets or sets the Y offset of the part relative to the sheet.
    /// </summary>
    double Y { get; set; }
  }
}