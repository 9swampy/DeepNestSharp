namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;

  public interface IPolygon : IEquatable<IPolygon>
  {
    IList<INfp> Children { get; set; }

    int Id { get; set; }

    /// <summary>
    /// Gets a value indicating whether every point in the polygon is exact, true to the original import.
    /// </summary>
    bool IsExact { get; }

    string Name { get; set; }

    SvgPoint[] Points { get; }

    /// <summary>
    /// Gets a value indicating the rotation of the part in degrees anticlockwise from the original.
    /// If the number is over 360˚ then it will be reduced by 360˚ repeatedly until it is under again.
    /// Use the Rotate method to actually alter the part. This property is purely intended to stored
    /// actual state. It also used to pass instruction to the nest; but that use is deprecated.
    /// </summary>
    double Rotation { get; }

    int Source { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the part should be differentiated on export.
    /// </summary>
    bool IsDifferentiated { get; set; }

    SvgPoint this[int ind] { get; }

    void AddPoint(SvgPoint point);
  }
}