namespace DeepNestLib.Geometry
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using DeepNestLib;

  public class GeometryUtil
  {
    internal static readonly double Tolerance = Math.Pow(10, -9); // floating point error is likely to be above 1 epsilon

    /// <summary>
    /// Converts from degrees to radians. Note that angles are measured counter-clockwise by convention. I didn't know that...
    /// </summary>
    /// <param name="rotationAngle">Degrees to convert.</param>
    /// <returns>Equivalent angle in radians.</returns>
    public static double ToRadians(double rotationAngle)
    {
      return (double)(rotationAngle * Math.PI / 180.0f);
    }

    // returns true if points are within the given distance
    public static bool WithinDistance(SvgPoint p1, SvgPoint p2, double distance)
    {
      var dx = p1.X - p2.X;
      var dy = p1.Y - p2.Y;
      return (dx * dx) + (dy * dy) < distance * distance;
    }

    // returns the rectangular bounding box of the given polygon
    public static PolygonBounds GetPolygonBounds(IPolygon polygon)
    {
      return GetPolygonBounds(polygon.Points);
    }

    public static PolygonBounds GetPolygonBounds(List<SvgPoint> polygon)
    {
      return GetPolygonBounds(polygon.ToArray());
    }

    public static PolygonBounds GetPolygonBounds(SvgPoint[] polygon)
    {
      if (polygon == null || polygon.Count() < 3)
      {
        throw new ArgumentException("null");
      }

      var xmin = polygon[0].X;
      var xmax = polygon[0].X;
      var ymin = polygon[0].Y;
      var ymax = polygon[0].Y;

      for (var i = 1; i < polygon.Length; i++)
      {
        if (polygon[i].X > xmax)
        {
          xmax = polygon[i].X;
        }
        else if (polygon[i].X < xmin)
        {
          xmin = polygon[i].X;
        }

        if (polygon[i].Y > ymax)
        {
          ymax = polygon[i].Y;
        }
        else if (polygon[i].Y < ymin)
        {
          ymin = polygon[i].Y;
        }
      }

      var w = xmax - xmin;
      var h = ymax - ymin;
      return new PolygonBounds(xmin, ymin, w, h);
    }

    public static bool AlmostEqual(double a, double b, double? tolerance = null)
    {
      if (tolerance == null)
      {
        tolerance = Tolerance;
      }

      return Math.Abs(a - b) < tolerance;
    }

    public static bool AlmostEqual(double? a, double? b, double? tolerance = null)
    {
      return AlmostEqual(a.Value, b.Value, tolerance);
    }

    // returns true if p lies on the line segment defined by AB, but not at any endpoints
    // may need work!
    public static bool OnSegment(SvgPoint a, SvgPoint b, SvgPoint p)
    {
      // vertical line
      if (AlmostEqual(a.X, b.X) && AlmostEqual(p.X, a.X))
      {
        if (!AlmostEqual(p.Y, b.Y) && !AlmostEqual(p.Y, a.Y) && p.Y < Math.Max(b.Y, a.Y) && p.Y > Math.Min(b.Y, a.Y))
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // horizontal line
      if (AlmostEqual(a.Y, b.Y) && AlmostEqual(p.Y, a.Y))
      {
        if (!AlmostEqual(p.X, b.X) && !AlmostEqual(p.X, a.X) && p.X < Math.Max(b.X, a.X) && p.X > Math.Min(b.X, a.X))
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // range check
      if ((p.X < a.X && p.X < b.X) || (p.X > a.X && p.X > b.X) || (p.Y < a.Y && p.Y < b.Y) || (p.Y > a.Y && p.Y > b.Y))
      {
        return false;
      }

      // exclude end points
      if ((AlmostEqual(p.X, a.X) && AlmostEqual(p.Y, a.Y)) || (AlmostEqual(p.X, b.X) && AlmostEqual(p.Y, b.Y)))
      {
        return false;
      }

      var cross = ((p.Y - a.Y) * (b.X - a.X)) - ((p.X - a.X) * (b.Y - a.Y));

      if (Math.Abs(cross) > Tolerance)
      {
        return false;
      }

      var dot = ((p.X - a.X) * (b.X - a.X)) + ((p.Y - a.Y) * (b.Y - a.Y));

      if (dot < 0 || AlmostEqual(dot, 0))
      {
        return false;
      }

      var len2 = ((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y));

      if (dot > len2 || AlmostEqual(dot, len2))
      {
        return false;
      }

      return true;
    }

    internal static double PolygonArea(IPolygon polygon)
    {
      return PolygonArea(polygon.Points);
    }

    internal static double PolygonArea(List<PointF> points)
    {
      return PolygonArea(points.Select(p => new SvgPoint(p.X, p.Y)).ToArray());
    }

    public static double PolygonArea(SvgPoint[] points)
    {
      double area = 0;
      if (points.Length < 3)
      {
        return 0;
      }

      int i, j;
      for (i = 0, j = points.Length - 1; i < points.Length; j = i++)
      {
        area += (points[j].X + points[i].X) * (points[j].Y
            - points[i].Y);
      }

      return 0.5f * area;
    }

    // return true if point is in the polygon, false if outside, and null if exactly on a point or edge
    public static bool? PointInPolygon(SvgPoint point, INfp polygon)
    {
      if (polygon == null || polygon.Points.Length < 3)
      {
        throw new ArgumentException();
      }

      var inside = false;

      // var offsetx = polygon.Offsetx || 0;
      // var offsety = polygon.Offsety || 0;
      var offsetx = polygon.OffsetX == null ? 0 : polygon.OffsetX.Value;
      var offsety = polygon.OffsetY == null ? 0 : polygon.OffsetY.Value;

      int i, j;
      for (i = 0, j = polygon.Points.Count() - 1; i < polygon.Points.Length; j = i++)
      {
        var xi = polygon.Points[i].X + offsetx;
        var yi = polygon.Points[i].Y + offsety;
        var xj = polygon.Points[j].X + offsetx;
        var yj = polygon.Points[j].Y + offsety;

        if (AlmostEqual(xi, point.X) && AlmostEqual(yi, point.Y))
        {
          return null; // no result
        }

        if (OnSegment(new SvgPoint(xi, yi), new SvgPoint(xj, yj), point))
        {
          return null; // exactly on the segment
        }

        if (AlmostEqual(xi, xj) && AlmostEqual(yi, yj))
        { // ignore very small lines
          continue;
        }

        var intersect = yi > point.Y != yj > point.Y && point.X < ((xj - xi) * (point.Y - yi) / (yj - yi)) + xi;
        if (intersect)
        {
          inside = !inside;
        }
      }

      return inside;
    }
  }
}
