namespace DeepNestLib.Geometry
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using DeepNestLib;

  public class GeometryUtil
  {
    private static readonly double Tolerance = Math.Pow(10, -9); // floating point error is likely to be above 1 epsilon

    // returns true if points are within the given distance
    public static bool WithinDistance(SvgPoint p1, SvgPoint p2, double distance)
    {
      var dx = p1.X - p2.X;
      var dy = p1.Y - p2.Y;
      return (dx * dx) + (dy * dy) < distance * distance;
    }

    // returns an interior NFP for the special case where A is a rectangle
    public static NoFitPolygon[] NoFitPolygonRectangle(NoFitPolygon a, NoFitPolygon b)
    {
      var minAx = a[0].X;
      var minAy = a[0].Y;
      var maxAx = a[0].X;
      var maxAy = a[0].Y;

      for (var i = 1; i < a.Length; i++)
      {
        if (a[i].X < minAx)
        {
          minAx = a[i].X;
        }

        if (a[i].Y < minAy)
        {
          minAy = a[i].Y;
        }

        if (a[i].X > maxAx)
        {
          maxAx = a[i].X;
        }

        if (a[i].Y > maxAy)
        {
          maxAy = a[i].Y;
        }
      }

      var minBx = b[0].X;
      var minBy = b[0].Y;
      var maxBx = b[0].X;
      var maxBy = b[0].Y;
      for (var i = 1; i < b.Length; i++)
      {
        if (b[i].X < minBx)
        {
          minBx = b[i].X;
        }

        if (b[i].Y < minBy)
        {
          minBy = b[i].Y;
        }

        if (b[i].X > maxBx)
        {
          maxBx = b[i].X;
        }

        if (b[i].Y > maxBy)
        {
          maxBy = b[i].Y;
        }
      }

      if (maxBx - minBx > maxAx - minAx)
      {
        return null;
      }

      if (maxBy - minBy > maxAy - minAy)
      {
        return null;
      }

      var pnts = new NoFitPolygon[]
      {
                new NoFitPolygon(new SvgPoint[]
                {
                    new SvgPoint(minAx - minBx + b[0].X, minAy - minBy + b[0].Y),
                    new SvgPoint(maxAx - maxBx + b[0].X, minAy - minBy + b[0].Y),
                    new SvgPoint(maxAx - maxBx + b[0].X, maxAy - maxBy + b[0].Y),
                    new SvgPoint(minAx - minBx + b[0].X, maxAy - maxBy + b[0].Y),
                }),
      };
      return pnts;
    }

    // returns the rectangular bounding box of the given polygon
    public static PolygonBounds GetPolygonBounds(INfp polygon)
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

      // return new rectanglef(xmin, ymin, xmax - xmin, ymax - ymin);
      return new PolygonBounds(xmin, ymin, w, h);
    }

    public static bool IsRectangle(NoFitPolygon poly, double? tolerance = null)
    {
      var bb = GetPolygonBounds(poly);
      if (tolerance == null)
      {
        tolerance = Tolerance;
      }

      for (var i = 0; i < poly.Points.Length; i++)
      {
        if (!AlmostEqual(poly.Points[i].X, bb.X) && !AlmostEqual(poly.Points[i].X, bb.X + bb.Width))
        {
          return false;
        }

        if (!AlmostEqual(poly.Points[i].Y, bb.Y) && !AlmostEqual(poly.Points[i].Y, bb.Y + bb.Height))
        {
          return false;
        }
      }

      return true;
    }

    public static PolygonWithBounds RotatePolygon(NoFitPolygon polygon, double angle)
    {
      List<SvgPoint> rotated = new List<SvgPoint>();
      angle = (double)(angle * Math.PI / 180.0);
      for (var i = 0; i < polygon.Points.Length; i++)
      {
        var x = polygon.Points[i].X;
        var y = polygon.Points[i].Y;
        var x1 = (double)((x * Math.Cos(angle)) - (y * Math.Sin(angle)));
        var y1 = (double)((x * Math.Sin(angle)) + (y * Math.Cos(angle)));

        rotated.Add(new SvgPoint(x1, y1));
      }

      var ret = new PolygonWithBounds(rotated);
      var bounds = GetPolygonBounds(ret);
      ret.X = bounds.X;
      ret.Y = bounds.Y;
      ret.Width = bounds.Width;
      ret.Height = bounds.Height;
      return ret;
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

    // returns true if point already exists in the given nfp
    public static bool InNfp(SvgPoint p, NoFitPolygon[] nfp)
    {
      if (nfp == null || nfp.Length == 0)
      {
        return false;
      }

      for (var i = 0; i < nfp.Length; i++)
      {
        for (var j = 0; j < nfp[i].Length; j++)
        {
          if (AlmostEqual(p.X, nfp[i][j].X) && AlmostEqual(p.Y, nfp[i][j].Y))
          {
            return true;
          }
        }
      }

      return false;
    }

    // normalize vector into a unit vector
    public static SvgPoint NormalizeVector(SvgPoint v)
    {
      if (AlmostEqual((v.X * v.X) + (v.Y * v.Y), 1))
      {
        return v; // given vector was already a unit vector
      }

      var len = Math.Sqrt((v.X * v.X) + (v.Y * v.Y));
      var inverse = (float)(1 / len);

      return new SvgPoint(v.X * inverse, v.Y * inverse);
    }

    public static double? PointDistance(SvgPoint p, SvgPoint s1, SvgPoint s2, SvgPoint normal, bool infinite = false)
    {
      normal = NormalizeVector(normal);

      var dir = new SvgPoint(normal.Y, -normal.X);

      var pdot = (p.X * dir.X) + (p.Y * dir.Y);
      var s1dot = (s1.X * dir.X) + (s1.Y * dir.Y);
      var s2dot = (s2.X * dir.X) + (s2.Y * dir.Y);

      var pdotnorm = (p.X * normal.X) + (p.Y * normal.Y);
      var s1dotnorm = (s1.X * normal.X) + (s1.Y * normal.Y);
      var s2dotnorm = (s2.X * normal.X) + (s2.Y * normal.Y);

      if (!infinite)
      {
        if (((pdot < s1dot || AlmostEqual(pdot, s1dot)) && (pdot < s2dot || AlmostEqual(pdot, s2dot))) || ((pdot > s1dot || AlmostEqual(pdot, s1dot)) && (pdot > s2dot || AlmostEqual(pdot, s2dot))))
        {
          return null; // dot doesn't collide with segment, or lies directly on the vertex
        }

        if (AlmostEqual(pdot, s1dot) && AlmostEqual(pdot, s2dot) && pdotnorm > s1dotnorm && pdotnorm > s2dotnorm)
        {
          return Math.Min(pdotnorm - s1dotnorm, pdotnorm - s2dotnorm);
        }

        if (AlmostEqual(pdot, s1dot) && AlmostEqual(pdot, s2dot) && pdotnorm < s1dotnorm && pdotnorm < s2dotnorm)
        {
          return -Math.Min(s1dotnorm - pdotnorm, s2dotnorm - pdotnorm);
        }
      }

      return -(pdotnorm - s1dotnorm + ((s1dotnorm - s2dotnorm) * (s1dot - pdot) / (s1dot - s2dot)));
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

    // project each point of B onto A in the given direction, and return the
    public static double? PolygonProjectionDistance(INfp a, INfp b, SvgPoint direction)
    {
      var bOffsetX = b.OffsetX ?? 0;
      var bOffsetY = b.OffsetY ?? 0;

      var aOffsetX = a.OffsetX ?? 0;
      var aOffsetY = a.OffsetY ?? 0;

      a = a.Slice(0);
      b = b.Slice(0);

      // close the loop for polygons
      if (a[0] != a[a.Length - 1])
      {
        ((IHiddenNfp)a).Push(a[0]);
      }

      if (b[0] != b[b.Length - 1])
      {
        ((IHiddenNfp)b).Push(b[0]);
      }

      var edgeA = a;
      var edgeB = b;

      double? distance = null;
      SvgPoint p, s1, s2;
      double? d;

      for (var i = 0; i < edgeB.Length; i++)
      {
        // the shortest/most negative projection of B onto A
        double? minprojection = null;
        SvgPoint minp = null;
        for (var j = 0; j < edgeA.Length - 1; j++)
        {
          p = new SvgPoint(edgeB[i].X + bOffsetX, edgeB[i].Y + bOffsetY);
          s1 = new SvgPoint(edgeA[j].X + aOffsetX, edgeA[j].Y + aOffsetY);
          s2 = new SvgPoint(edgeA[j + 1].X + aOffsetX, edgeA[j + 1].Y + aOffsetY);

          if (Math.Abs(((s2.Y - s1.Y) * direction.X) - ((s2.X - s1.X) * direction.Y)) < Tolerance)
          {
            continue;
          }

          // project point, ignore edge boundaries
          d = PointDistance(p, s1, s2, direction);

          if (d != null && (minprojection == null || d < minprojection))
          {
            minprojection = d;
            minp = p;
          }
        }

        if (minprojection != null && (distance == null || minprojection > distance))
        {
          distance = minprojection;
        }
      }

      return distance;
    }

    public static double PolygonArea(INfp polygon)
    {
      double area = 0;
      int i, j;
      for (i = 0, j = polygon.Points.Length - 1; i < polygon.Points.Length; j = i++)
      {
        area += (polygon.Points[j].X + polygon.Points[i].X) * (polygon.Points[j].Y
            - polygon.Points[i].Y);
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

    // todo: swap this for a more efficient sweep-line implementation
    // returnEdges: if set, return all edges on A that have intersections
    public static bool Intersect(INfp a, INfp b)
    {
      var aOffsetx = a.OffsetX ?? 0;
      var aOffsety = a.OffsetY ?? 0;

      var bOffsetx = b.OffsetX ?? 0;
      var bOffsety = b.OffsetY ?? 0;

      a = a.Slice(0);
      b = b.Slice(0);

      for (var i = 0; i < a.Length - 1; i++)
      {
        for (var j = 0; j < b.Length - 1; j++)
        {
          var a1 = new SvgPoint(a[i].X + aOffsetx, a[i].Y + aOffsety);
          var a2 = new SvgPoint(a[i + 1].X + aOffsetx, a[i + 1].Y + aOffsety);
          var b1 = new SvgPoint(b[j].X + bOffsetx, b[j].Y + bOffsety);
          var b2 = new SvgPoint(b[j + 1].X + bOffsetx, b[j + 1].Y + bOffsety);

          var prevbindex = j == 0 ? b.Length - 1 : j - 1;
          var prevaindex = i == 0 ? a.Length - 1 : i - 1;
          var nextbindex = j + 1 == b.Length - 1 ? 0 : j + 2;
          var nextaindex = i + 1 == a.Length - 1 ? 0 : i + 2;

          // go even further back if we happen to hit on a loop end point
          if (b[prevbindex] == b[j] || (AlmostEqual(b[prevbindex].X, b[j].X) && AlmostEqual(b[prevbindex].Y, b[j].Y)))
          {
            prevbindex = prevbindex == 0 ? b.Length - 1 : prevbindex - 1;
          }

          if (a[prevaindex] == a[i] || (AlmostEqual(a[prevaindex].X, a[i].X) && AlmostEqual(a[prevaindex].Y, a[i].Y)))
          {
            prevaindex = prevaindex == 0 ? a.Length - 1 : prevaindex - 1;
          }

          // go even further forward if we happen to hit on a loop end point
          if (b[nextbindex] == b[j + 1] || (AlmostEqual(b[nextbindex].X, b[j + 1].X) && AlmostEqual(b[nextbindex].Y, b[j + 1].Y)))
          {
            nextbindex = nextbindex == b.Length - 1 ? 0 : nextbindex + 1;
          }

          if (a[nextaindex] == a[i + 1] || (AlmostEqual(a[nextaindex].X, a[i + 1].X) && AlmostEqual(a[nextaindex].Y, a[i + 1].Y)))
          {
            nextaindex = nextaindex == a.Length - 1 ? 0 : nextaindex + 1;
          }

          var a0 = new SvgPoint(a[prevaindex].X + aOffsetx, a[prevaindex].Y + aOffsety);
          var b0 = new SvgPoint(b[prevbindex].X + bOffsetx, b[prevbindex].Y + bOffsety);

          var a3 = new SvgPoint(a[nextaindex].X + aOffsetx, a[nextaindex].Y + aOffsety);
          var b3 = new SvgPoint(b[nextbindex].X + bOffsetx, b[nextbindex].Y + bOffsety);

          if (OnSegment(a1, a2, b1) || (AlmostEqual(a1.X, b1.X) && AlmostEqual(a1.Y, b1.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var b0in = PointInPolygon(b0, a);
            var b2in = PointInPolygon(b2, a);
            if ((b0in == true && b2in == false) || (b0in == false && b2in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          if (OnSegment(a1, a2, b2) || (AlmostEqual(a2.X, b2.X) && AlmostEqual(a2.Y, b2.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var b1in = PointInPolygon(b1, a);
            var b3in = PointInPolygon(b3, a);

            if ((b1in == true && b3in == false) || (b1in == false && b3in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          if (OnSegment(b1, b2, a1) || (AlmostEqual(a1.X, b2.X) && AlmostEqual(a1.Y, b2.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var a0in = PointInPolygon(a0, b);
            var a2in = PointInPolygon(a2, b);

            if ((a0in == true && a2in == false) || (a0in == false && a2in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          if (OnSegment(b1, b2, a2) || (AlmostEqual(a2.X, b1.X) && AlmostEqual(a2.Y, b1.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var a1in = PointInPolygon(a1, b);
            var a3in = PointInPolygon(a3, b);

            if ((a1in == true && a3in == false) || (a1in == false && a3in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          var p = LineIntersect(b1, b2, a1, a2);

          if (p != null)
          {
            return true;
          }
        }
      }

      return false;
    }

    private static bool IsFinite(object obj)
    {
      return true;
    }

    // returns the intersection of AB and EF
    // or null if there are no intersections or other numerical error
    // if the infinite flag is set, AE and EF describe infinite lines without endpoints, they are finite line segments otherwise
    private static SvgPoint LineIntersect(SvgPoint a, SvgPoint b, SvgPoint e, SvgPoint f, bool infinite = false)
    {
      double a1, a2, b1, b2, c1, c2, x, y;

      a1 = b.Y - a.Y;
      b1 = a.X - b.X;
      c1 = (b.X * a.Y) - (a.X * b.Y);
      a2 = f.Y - e.Y;
      b2 = e.X - f.X;
      c2 = (f.X * e.Y) - (e.X * f.Y);

      var denom = (a1 * b2) - (a2 * b1);

      x = ((b1 * c2) - (b2 * c1)) / denom;
      y = ((a2 * c1) - (a1 * c2)) / denom;

      if (!IsFinite(x) || !IsFinite(y))
      {
        return null;
      }

      // lines are colinear
      /*var crossABE = (E.y - A.y) * (B.x - A.x) - (E.x - A.x) * (B.y - A.y);
      var crossABF = (F.y - A.y) * (B.x - A.x) - (F.x - A.x) * (B.y - A.y);
      if(_almostEqual(crossABE,0) && _almostEqual(crossABF,0)){
          return null;
      }*/

      if (!infinite)
      {
        // coincident points do not count as intersecting
        if (Math.Abs(a.X - b.X) > Tolerance && (a.X < b.X ? x < a.X || x > b.X : x > a.X || x < b.X))
        {
          return null;
        }

        if (Math.Abs(a.Y - b.Y) > Tolerance && (a.Y < b.Y ? y < a.Y || y > b.Y : y > a.Y || y < b.Y))
        {
          return null;
        }

        if (Math.Abs(e.X - f.X) > Tolerance && (e.X < f.X ? x < e.X || x > f.X : x > e.X || x < f.X))
        {
          return null;
        }

        if (Math.Abs(e.Y - f.Y) > Tolerance && (e.Y < f.Y ? y < e.Y || y > f.Y : y > e.Y || y < f.Y))
        {
          return null;
        }
      }

      return new SvgPoint(x, y);
    }

    // searches for an arrangement of A and B such that they do not overlap
    // if an NFP is given, only search for startpoints that have not already been traversed in the given NFP
    private static SvgPoint SearchStartPoint(INfp a, INfp b, bool inside, NoFitPolygon[] nfp = null)
    {
      // clone arrays
      a = a.Slice(0);
      b = b.Slice(0);

      // close the loop for polygons
      if (a[0] != a[a.Length - 1])
      {
        ((IHiddenNfp)a).Push(a[0]);
      }

      if (b[0] != b[b.Length - 1])
      {
        ((IHiddenNfp)b).Push(b[0]);
      }

      for (var i = 0; i < a.Length - 1; i++)
      {
        if (!a[i].Marked)
        {
          a[i].Marked = true;
          for (var j = 0; j < b.Length; j++)
          {
            b.OffsetX = a[i].X - b[j].X;
            b.OffsetY = a[i].Y - b[j].Y;

            bool? bInside = null;
            for (var k = 0; k < b.Length; k++)
            {
              var inpoly = PointInPolygon(
                  new SvgPoint(
                  b[k].X + b.OffsetX.Value,
                  b[k].Y + b.OffsetY.Value), a);
              if (inpoly != null)
              {
                bInside = inpoly;
                break;
              }
            }

            if (bInside == null)
            { // A and B are the same
              return null;
            }

            var startPoint = new SvgPoint(b.OffsetX.Value, b.OffsetY.Value);
            if (((bInside.Value && inside) || (!bInside.Value && !inside)) &&
                !Intersect(a, b) && !InNfp(startPoint, nfp))
            {
              return startPoint;
            }

            // slide B along vector
            var vx = a[i + 1].X - a[i].X;
            var vy = a[i + 1].Y - a[i].Y;

            var d1 = PolygonProjectionDistance(a, b, new SvgPoint(vx, vy));
            var d2 = PolygonProjectionDistance(b, a, new SvgPoint(-vx, -vy));

            double? d = null;

            // todo: clean this up
            if (d1 == null && d2 == null)
            {
              // nothin
            }
            else if (d1 == null)
            {
              d = d2;
            }
            else if (d2 == null)
            {
              d = d1;
            }
            else
            {
              d = Math.Min(d1.Value, d2.Value);
            }

            // only slide until no longer negative
            // todo: clean this up
            if (d != null && !AlmostEqual(d, 0) && d > 0)
            {
            }
            else
            {
              continue;
            }

            var vd2 = (vx * vx) + (vy * vy);

            if (d * d < vd2 && !AlmostEqual(d * d, vd2))
            {
              var vd = Math.Sqrt((vx * vx) + (vy * vy));
              vx *= d.Value / vd;
              vy *= d.Value / vd;
            }

            b.OffsetX += vx;
            b.OffsetY += vy;

            for (var k = 0; k < b.Length; k++)
            {
              var inpoly = PointInPolygon(
                  new SvgPoint(
                   b[k].X + b.OffsetX.Value, b[k].Y + b.OffsetY.Value), a);
              if (inpoly != null)
              {
                bInside = inpoly;
                break;
              }
            }

            startPoint =
                                new SvgPoint(b.OffsetX.Value, b.OffsetY.Value);
            if (((bInside.Value && inside) || (!bInside.Value && !inside)) &&
                !Intersect(a, b) && !InNfp(startPoint, nfp))
            {
              return startPoint;
            }
          }
        }
      }

      return null;
    }

    public static double? SegmentDistance(SvgPoint a, SvgPoint b, SvgPoint e, SvgPoint f, SvgPoint direction)
    {
      var normal = new SvgPoint(
          direction.Y,
          -direction.X);

      var reverse = new SvgPoint(
              -direction.X,
              -direction.Y);

      var dotA = (a.X * normal.X) + (a.Y * normal.Y);
      var dotB = (b.X * normal.X) + (b.Y * normal.Y);
      var dotE = (e.X * normal.X) + (e.Y * normal.Y);
      var dotF = (f.X * normal.X) + (f.Y * normal.Y);

      var crossA = (a.X * direction.X) + (a.Y * direction.Y);
      var crossB = (b.X * direction.X) + (b.Y * direction.Y);
      var crossE = (e.X * direction.X) + (e.Y * direction.Y);
      var crossF = (f.X * direction.X) + (f.Y * direction.Y);

      var crossABmin = Math.Min(crossA, crossB);
      var crossABmax = Math.Max(crossA, crossB);

      var crossEFmax = Math.Max(crossE, crossF);
      var crossEFmin = Math.Min(crossE, crossF);

      var abMin = Math.Min(dotA, dotB);
      var abMax = Math.Max(dotA, dotB);

      var efMax = Math.Max(dotE, dotF);
      var efMin = Math.Min(dotE, dotF);

      // segments that will merely touch at one point
      if (AlmostEqual(abMax, efMin, Tolerance) || AlmostEqual(abMin, efMax, Tolerance))
      {
        return null;
      }

      // segments miss eachother completely
      if (abMax < efMin || abMin > efMax)
      {
        return null;
      }

      double overlap;

      if ((abMax > efMax && abMin < efMin) || (efMax > abMax && efMin < abMin))
      {
        overlap = 1;
      }
      else
      {
        var minMax = Math.Min(abMax, efMax);
        var maxMin = Math.Max(abMin, efMin);

        var maxMax = Math.Max(abMax, efMax);
        var minMin = Math.Min(abMin, efMin);

        overlap = (minMax - maxMin) / (maxMax - minMin);
      }

      var crossABE = ((e.Y - a.Y) * (b.X - a.X)) - ((e.X - a.X) * (b.Y - a.Y));
      var crossABF = ((f.Y - a.Y) * (b.X - a.X)) - ((f.X - a.X) * (b.Y - a.Y));

      // lines are colinear
      if (AlmostEqual(crossABE, 0) && AlmostEqual(crossABF, 0))
      {
        var abNorm = new SvgPoint(b.Y - a.Y, a.X - b.X);
        var efNorm = new SvgPoint(f.Y - e.Y, e.X - f.X);

        var abNormLength = Math.Sqrt((abNorm.X * abNorm.X) + (abNorm.Y * abNorm.Y));
        abNorm.X /= abNormLength;
        abNorm.Y /= abNormLength;

        var efNormLength = Math.Sqrt((efNorm.X * efNorm.X) + (efNorm.Y * efNorm.Y));
        efNorm.X /= efNormLength;
        efNorm.Y /= efNormLength;

        // segment normals must point in opposite directions
        if (Math.Abs((abNorm.Y * efNorm.X) - (abNorm.X * efNorm.Y)) < Tolerance && (abNorm.Y * efNorm.Y) + (abNorm.X * efNorm.X) < 0)
        {
          // normal of AB segment must point in same direction as given direction vector
          var normdot = (abNorm.Y * direction.Y) + (abNorm.X * direction.X);

          // the segments merely slide along eachother
          if (AlmostEqual(normdot, 0, Tolerance))
          {
            return null;
          }

          if (normdot < 0)
          {
            return 0;
          }
        }

        return null;
      }

      var distances = new List<double>();

      // coincident points
      if (AlmostEqual(dotA, dotE))
      {
        distances.Add(crossA - crossE);
      }
      else if (AlmostEqual(dotA, dotF))
      {
        distances.Add(crossA - crossF);
      }
      else if (dotA > efMin && dotA < efMax)
      {
        var d = PointDistance(a, e, f, reverse);
        if (d != null && AlmostEqual(d, 0))
        { // A currently touches EF, but AB is moving away from EF
          var dB = PointDistance(b, e, f, reverse, true);
          if (dB < 0 || AlmostEqual(dB * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (AlmostEqual(dotB, dotE))
      {
        distances.Add(crossB - crossE);
      }
      else if (AlmostEqual(dotB, dotF))
      {
        distances.Add(crossB - crossF);
      }
      else if (dotB > efMin && dotB < efMax)
      {
        var d = PointDistance(b, e, f, reverse);

        if (d != null && AlmostEqual(d, 0))
        { // crossA>crossB A currently touches EF, but AB is moving away from EF
          var dA = PointDistance(a, e, f, reverse, true);
          if (dA < 0 || AlmostEqual(dA * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (dotE > abMin && dotE < abMax)
      {
        var d = PointDistance(e, a, b, direction);
        if (d != null && AlmostEqual(d, 0))
        { // crossF<crossE A currently touches EF, but AB is moving away from EF
          var dF = PointDistance(f, a, b, direction, true);
          if (dF < 0 || AlmostEqual(dF * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (dotF > abMin && dotF < abMax)
      {
        var d = PointDistance(f, a, b, direction);
        if (d != null && AlmostEqual(d, 0))
        { // && crossE<crossF A currently touches EF, but AB is moving away from EF
          var dE = PointDistance(e, a, b, direction, true);
          if (dE < 0 || AlmostEqual(dE * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (distances.Count == 0)
      {
        return null;
      }

      // return Math.min.apply(Math, distances);
      return distances.Min();
    }

    public static double? PolygonSlideDistance(INfp a, INfp b, NVector direction, bool ignoreNegative)
    {
      a = a.Slice(0);
      b = b.Slice(0);

      // close the loop for polygons
      if (a[0] != a[a.Length - 1])
      {
        ((IHiddenNfp)a).Push(a[0]);
      }

      if (b[0] != b[b.Length - 1])
      {
        ((IHiddenNfp)b).Push(b[0]);
      }

      var edgeA = a;
      var edgeB = b;

      double? distance = null;

      // var p, s1, s2;
      double? d;

      var dir = NormalizeVector(new SvgPoint(direction.X, direction.Y));

      var normal = new SvgPoint(
          dir.Y,
          -dir.X);

      var reverse = new SvgPoint(-dir.X, -dir.Y);

      for (var i = 0; i < edgeB.Length - 1; i++)
      {
        // var mind = null;
        for (var j = 0; j < edgeA.Length - 1; j++)
        {
          var aCurrent = new SvgPoint(edgeA[j].X + a.OffsetX ?? 0, edgeA[j].Y + a.OffsetY ?? 0);
          var aNext = new SvgPoint(edgeA[j + 1].X + a.OffsetX ?? 0, edgeA[j + 1].Y + a.OffsetY ?? 0);
          var bCurrent = new SvgPoint(edgeB[i].X + b.OffsetX ?? 0, edgeB[i].Y + b.OffsetY ?? 0);
          var bNext = new SvgPoint(edgeB[i + 1].X + b.OffsetX ?? 0, edgeB[i + 1].Y + b.OffsetY ?? 0);
          if ((AlmostEqual(aCurrent.X, aNext.X) && AlmostEqual(aCurrent.Y, aNext.Y)) || (AlmostEqual(bCurrent.X, bNext.X) && AlmostEqual(bCurrent.Y, bNext.Y)))
          {
            continue; // ignore extremely small lines
          }

          d = SegmentDistance(aCurrent, aNext, bCurrent, bNext, dir);

          if (d != null && (distance == null || d < distance))
          {
            if (!ignoreNegative || d > 0 || AlmostEqual(d, 0))
            {
              distance = d;
            }
          }
        }
      }

      return distance;
    }

    // given a static polygon A and a movable polygon B, compute a no fit polygon by orbiting B about A
    // if the inside flag is set, B is orbited inside of A rather than outside
    // if the searchEdges flag is set, all edges of A are explored for NFPs - multiple
    public static NoFitPolygon[] NoFitPolygon(NoFitPolygon a, NoFitPolygon b, bool inside, bool searchEdges)
    {
      if (a == null || a.Length < 3 || b == null || b.Length < 3)
      {
        return null;
      }

      a.OffsetX = 0;
      a.OffsetY = 0;

      int i = 0, j = 0;

      var minA = a[0].Y;
      var minAindex = 0;

      var maxB = b[0].Y;
      var maxBindex = 0;

      for (i = 1; i < a.Length; i++)
      {
        a[i].Marked = false;
        if (a[i].Y < minA)
        {
          minA = a[i].Y;
          minAindex = i;
        }
      }

      for (i = 1; i < b.Length; i++)
      {
        b[i].Marked = false;
        if (b[i].Y > maxB)
        {
          maxB = b[i].Y;
          maxBindex = i;
        }
      }

      SvgPoint startpoint;
      if (!inside)
      {
        // shift B such that the bottom-most point of B is at the top-most point of A. This guarantees an initial placement with no intersections
        startpoint = new SvgPoint(
             a[minAindex].X - b[maxBindex].X,
             a[minAindex].Y - b[maxBindex].Y);
      }
      else
      {
        // no reliable heuristic for inside
        startpoint = SearchStartPoint(a, b, true);
      }

      List<NoFitPolygon> nfpList = new List<NoFitPolygon>();

      while (startpoint != null)
      {
        b.OffsetX = startpoint.X;
        b.OffsetY = startpoint.Y;

        // maintain a list of touching points/edges
        List<TouchingItem> touching = null;

        NVector prevvector = null; // keep track of previous vector
        NoFitPolygon nfp = new NoFitPolygon();
        /*var NFP = [{
            x: B[0].x + B.Offsetx,
            y: B[0].y + B.Offsety
        }];*/

        ((IHiddenNfp)nfp).Push(new SvgPoint(b[0].X + b.OffsetX.Value, b[0].Y + b.OffsetY.Value));

        var referencex = b[0].X + b.OffsetX.Value;
        var referencey = b[0].Y + b.OffsetY.Value;
        var startx = referencex;
        var starty = referencey;
        var counter = 0;

        while (counter < 10 * (a.Length + b.Length))
        { // sanity check, prevent infinite loop
          touching = new List<TouchingItem>();

          // find touching vertices/edges
          for (i = 0; i < a.Length; i++)
          {
            var nexti = i == a.Length - 1 ? 0 : i + 1;
            for (j = 0; j < b.Length; j++)
            {
              var nextj = j == b.Length - 1 ? 0 : j + 1;
              if (AlmostEqual(a[i].X, b[j].X + b.OffsetX) && AlmostEqual(a[i].Y, b[j].Y + b.OffsetY))
              {
                touching.Add(new TouchingItem(0, i, j));
              }
              else if (OnSegment(
                a[i],
                a[nexti],
                new SvgPoint(b[j].X + b.OffsetX.Value, b[j].Y + b.OffsetY.Value)))
              {
                touching.Add(new TouchingItem(1, nexti, j));
              }
              else if (OnSegment(
                  new SvgPoint(
                   b[j].X + b.OffsetX.Value, b[j].Y + b.OffsetY.Value),
                  new SvgPoint(
                   b[nextj].X + b.OffsetX.Value, b[nextj].Y + b.OffsetY.Value), a[i]))
              {
                touching.Add(new TouchingItem(2, i, nextj));
              }
            }
          }

          // generate translation vectors from touching vertices/edges
          var vectors = new List<NVector>();
          for (i = 0; i < touching.Count; i++)
          {
            var vertexA = a[touching[i].A];
            vertexA.Marked = true;

            // adjacent A vertices
            var prevAindex = touching[i].A - 1;
            var nextAindex = touching[i].A + 1;

            prevAindex = prevAindex < 0 ? a.Length - 1 : prevAindex; // loop
            nextAindex = nextAindex >= a.Length ? 0 : nextAindex; // loop

            var prevA = a[prevAindex];
            var nextA = a[nextAindex];

            // adjacent B vertices
            var vertexB = b[touching[i].B];

            var prevBindex = touching[i].B - 1;
            var nextBindex = touching[i].B + 1;

            prevBindex = prevBindex < 0 ? b.Length - 1 : prevBindex; // loop
            nextBindex = nextBindex >= b.Length ? 0 : nextBindex; // loop

            var prevB = b[prevBindex];
            var nextB = b[nextBindex];

            if (touching[i].Type == 0)
            {
              var vA1 = new NVector(
                   prevA.X - vertexA.X,
                   prevA.Y - vertexA.Y,
                   vertexA,
                   prevA);

              var vA2 = new NVector(
                       nextA.X - vertexA.X,
                       nextA.Y - vertexA.Y,
                       vertexA,
                       nextA);

              // B vectors need to be inverted
              var vB1 = new NVector(
                           vertexB.X - prevB.X,
                           vertexB.Y - prevB.Y,
                           prevB,
                           vertexB);

              var vB2 = new NVector(
                               vertexB.X - nextB.X,
                               vertexB.Y - nextB.Y,
                               nextB,
                               vertexB);

              vectors.Add(vA1);
              vectors.Add(vA2);
              vectors.Add(vB1);
              vectors.Add(vB2);
            }
            else if (touching[i].Type == 1)
            {
              vectors.Add(new NVector(
                   vertexA.X - (vertexB.X + b.OffsetX.Value),
                   vertexA.Y - (vertexB.Y + b.OffsetY.Value),
                   prevA,
                   vertexA));

              vectors.Add(new NVector(
                   prevA.X - (vertexB.X + b.OffsetX.Value),
                   prevA.Y - (vertexB.Y + b.OffsetY.Value),
                   vertexA,
                   prevA));
            }
            else if (touching[i].Type == 2)
            {
              vectors.Add(new NVector(
                   vertexA.X - (vertexB.X + b.OffsetX.Value),
                   vertexA.Y - (vertexB.Y + b.OffsetY.Value),
                   prevB,
                   vertexB));

              vectors.Add(new NVector(
                   vertexA.X - (prevB.X + b.OffsetX.Value),
                   vertexA.Y - (prevB.Y + b.OffsetY.Value),
                   vertexB,
                   prevB));
            }
          }

          // todo: there should be a faster way to reject vectors that will cause immediate intersection. For now just check them all
          NVector translate = null;
          double maxd = 0;

          for (i = 0; i < vectors.Count; i++)
          {
            if (vectors[i].X == 0 && vectors[i].Y == 0)
            {
              continue;
            }

            // if this vector points us back to where we came from, ignore it.
            // ie cross product = 0, dot product < 0
            if (prevvector != null &&
                (vectors[i].Y * prevvector.Y) + (vectors[i].X * prevvector.X) < 0)
            {
              // compare magnitude with unit vectors
              var vectorlength = (double)Math.Sqrt((vectors[i].X * vectors[i].X) + (vectors[i].Y * vectors[i].Y));
              var unitv = new SvgPoint(vectors[i].X / vectorlength, vectors[i].Y / vectorlength);

              var prevlength = (double)Math.Sqrt((prevvector.X * prevvector.X) + (prevvector.Y * prevvector.Y));
              var prevunit = new SvgPoint(prevvector.X / prevlength, prevvector.Y / prevlength);

              // we need to scale down to unit vectors to normalize vector length. Could also just do a tan here
              if (Math.Abs((unitv.Y * prevunit.X) - (unitv.X * prevunit.Y)) < 0.0001)
              {
                continue;
              }
            }

            var d = PolygonSlideDistance(a, b, vectors[i], true);
            var vecd2 = (vectors[i].X * vectors[i].X) + (vectors[i].Y * vectors[i].Y);

            if (d == null || d * d > vecd2)
            {
              var vecd = (double)Math.Sqrt((vectors[i].X * vectors[i].X) + (vectors[i].Y * vectors[i].Y));
              d = vecd;
            }

            if (d != null && d > maxd)
            {
              maxd = d.Value;
              translate = vectors[i];
            }
          }

          if (translate == null || AlmostEqual(maxd, 0))
          {
            // didn't close the loop, something went wrong here
            nfp = null;
            break;
          }

          translate.Start.Marked = true;
          translate.End.Marked = true;

          prevvector = translate;

          // trim
          var vlength2 = (translate.X * translate.X) + (translate.Y * translate.Y);
          if (maxd * maxd < vlength2 && !AlmostEqual(maxd * maxd, vlength2))
          {
            var scale = (double)Math.Sqrt(maxd * maxd / vlength2);
            translate.X *= scale;
            translate.Y *= scale;
          }

          referencex += translate.X;
          referencey += translate.Y;

          if (AlmostEqual(referencex, startx) && AlmostEqual(referencey, starty))
          {
            // we've made a full loop
            break;
          }

          // if A and B start on a touching horizontal line, the end point may not be the start point
          var looped = false;
          if (nfp.Length > 0)
          {
            for (i = 0; i < nfp.Length - 1; i++)
            {
              if (AlmostEqual(referencex, nfp[i].X) && AlmostEqual(referencey, nfp[i].Y))
              {
                looped = true;
              }
            }
          }

          if (looped)
          {
            // we've made a full loop
            break;
          }

          ((IHiddenNfp)nfp).Push(new SvgPoint(referencex, referencey));

          b.OffsetX += translate.X;
          b.OffsetY += translate.Y;

          counter++;
        }

        if (nfp != null && nfp.Length > 0)
        {
          nfpList.Add(nfp);
        }

        if (!searchEdges)
        {
          // only get outer NFP or first inner NFP
          break;
        }

        startpoint = SearchStartPoint(a, b, inside, nfpList.ToArray());
      }

      return nfpList.ToArray();
    }

    public class NVector
    {
      public NVector(double v1, double v2, SvgPoint start, SvgPoint end)
      {
        this.X = v1;
        this.Y = v2;
        this.Start = start;
        this.End = end;
      }

      public SvgPoint Start { get; set; }

      public SvgPoint End { get; set; }

      public double X { get; set; }

      public double Y { get; set; }
    }

    public class TouchingItem
    {
      public TouchingItem(int type, int a, int b)
      {
        this.A = a;
        this.B = b;
        this.Type = type;
      }

      public int A { get; set; }

      public int B { get; set; }

      public int Type { get; set; }
    }
  }
}
