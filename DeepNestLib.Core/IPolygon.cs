namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;

  public interface IPolygon : IEquatable<IPolygon>
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
}