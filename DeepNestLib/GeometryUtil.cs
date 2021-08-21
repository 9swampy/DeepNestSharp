namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;

  public partial class GeometryUtil
  {
    // returns true if points are within the given distance
    public static bool _withinDistance(SvgPoint p1, SvgPoint p2, double distance)
    {
      var dx = p1.X - p2.X;
      var dy = p1.Y - p2.Y;
      return ((dx * dx) + (dy * dy)) < distance * distance;
    }

    // returns an interior NFP for the special case where A is a rectangle
    public static NFP[] noFitPolygonRectangle(NFP a, NFP b)
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
      for (int i = 1; i < b.Length; i++)
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

      var pnts = new NFP[]
      {
                new NFP(new SvgPoint[]
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

    public static bool isRectangle(NFP poly, double? tolerance = null)
    {
      var bb = GetPolygonBounds(poly);
      if (tolerance == null)
      {
        tolerance = TOL;
      }

      for (var i = 0; i < poly.Points.Length; i++)
      {
        if (!_almostEqual(poly.Points[i].X, bb.X) && !_almostEqual(poly.Points[i].X, bb.X + bb.Width))
        {
          return false;
        }

        if (!_almostEqual(poly.Points[i].Y, bb.Y) && !_almostEqual(poly.Points[i].Y, bb.Y + bb.Height))
        {
          return false;
        }
      }

      return true;
    }

    public static PolygonWithBounds rotatePolygon(NFP polygon, double angle)
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

      // reset bounding box
      RectangleF rr = default(RectangleF);

      var ret = new PolygonWithBounds(rotated);
      var bounds = GeometryUtil.GetPolygonBounds(ret);
      ret.X = bounds.X;
      ret.Y = bounds.Y;
      ret.Width = bounds.Width;
      ret.Height = bounds.Height;
      return ret;
    }

    public static bool _almostEqual(double a, double b, double? tolerance = null)
    {
      if (tolerance == null)
      {
        tolerance = TOL;
      }

      return Math.Abs(a - b) < tolerance;
    }

    public static bool _almostEqual(double? a, double? b, double? tolerance = null)
    {
      return _almostEqual(a.Value, b.Value, tolerance);
    }

    // returns true if point already exists in the given nfp
    public static bool inNfp(SvgPoint p, NFP[] nfp)
    {
      if (nfp == null || nfp.Length == 0)
      {
        return false;
      }

      for (var i = 0; i < nfp.Length; i++)
      {
        for (var j = 0; j < nfp[i].Length; j++)
        {
          if (_almostEqual(p.X, nfp[i][j].X) && _almostEqual(p.Y, nfp[i][j].Y))
          {
            return true;
          }
        }
      }

      return false;
    }

    // normalize vector into a unit vector
    public static SvgPoint _normalizeVector(SvgPoint v)
    {
      if (_almostEqual((v.X * v.X) + (v.Y * v.Y), 1))
      {
        return v; // given vector was already a unit vector
      }

      var len = Math.Sqrt((v.X * v.X) + (v.Y * v.Y));
      var inverse = (float)(1 / len);

      return new SvgPoint(v.X * inverse, v.Y * inverse);
    }

    public static double? pointDistance(SvgPoint p, SvgPoint s1, SvgPoint s2, SvgPoint normal, bool infinite = false)
    {
      normal = _normalizeVector(normal);

      var dir = new SvgPoint(normal.Y, -normal.X);

      var pdot = (p.X * dir.X) + (p.Y * dir.Y);
      var s1dot = (s1.X * dir.X) + (s1.Y * dir.Y);
      var s2dot = (s2.X * dir.X) + (s2.Y * dir.Y);

      var pdotnorm = (p.X * normal.X) + (p.Y * normal.Y);
      var s1dotnorm = (s1.X * normal.X) + (s1.Y * normal.Y);
      var s2dotnorm = (s2.X * normal.X) + (s2.Y * normal.Y);

      if (!infinite)
      {
        if (((pdot < s1dot || _almostEqual(pdot, s1dot)) && (pdot < s2dot || _almostEqual(pdot, s2dot))) || ((pdot > s1dot || _almostEqual(pdot, s1dot)) && (pdot > s2dot || _almostEqual(pdot, s2dot))))
        {
          return null; // dot doesn't collide with segment, or lies directly on the vertex
        }

        if ((_almostEqual(pdot, s1dot) && _almostEqual(pdot, s2dot)) && (pdotnorm > s1dotnorm && pdotnorm > s2dotnorm))
        {
          return Math.Min(pdotnorm - s1dotnorm, pdotnorm - s2dotnorm);
        }

        if ((_almostEqual(pdot, s1dot) && _almostEqual(pdot, s2dot)) && (pdotnorm < s1dotnorm && pdotnorm < s2dotnorm))
        {
          return -Math.Min(s1dotnorm - pdotnorm, s2dotnorm - pdotnorm);
        }
      }

      return -(pdotnorm - s1dotnorm + ((s1dotnorm - s2dotnorm) * (s1dot - pdot) / (s1dot - s2dot)));
    }

    private static double TOL = (double)Math.Pow(10, -9); // floating point error is likely to be above 1 epsilon

    // returns true if p lies on the line segment defined by AB, but not at any endpoints
    // may need work!
    public static bool _onSegment(SvgPoint a, SvgPoint b, SvgPoint p)
    {
      // vertical line
      if (_almostEqual(a.X, b.X) && _almostEqual(p.X, a.X))
      {
        if (!_almostEqual(p.Y, b.Y) && !_almostEqual(p.Y, a.Y) && p.Y < Math.Max(b.Y, a.Y) && p.Y > Math.Min(b.Y, a.Y))
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // horizontal line
      if (_almostEqual(a.Y, b.Y) && _almostEqual(p.Y, a.Y))
      {
        if (!_almostEqual(p.X, b.X) && !_almostEqual(p.X, a.X) && p.X < Math.Max(b.X, a.X) && p.X > Math.Min(b.X, a.X))
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
      if ((_almostEqual(p.X, a.X) && _almostEqual(p.Y, a.Y)) || (_almostEqual(p.X, b.X) && _almostEqual(p.Y, b.Y)))
      {
        return false;
      }

      var cross = ((p.Y - a.Y) * (b.X - a.X)) - ((p.X - a.X) * (b.Y - a.Y));

      if (Math.Abs(cross) > TOL)
      {
        return false;
      }

      var dot = ((p.X - a.X) * (b.X - a.X)) + ((p.Y - a.Y) * (b.Y - a.Y));

      if (dot < 0 || _almostEqual(dot, 0))
      {
        return false;
      }

      var len2 = ((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y));

      if (dot > len2 || _almostEqual(dot, len2))
      {
        return false;
      }

      return true;
    }

    // project each point of B onto A in the given direction, and return the
    public static double? polygonProjectionDistance(INfp a, INfp b, SvgPoint direction)
    {
      var Boffsetx = b.Offsetx ?? 0;
      var Boffsety = b.Offsety ?? 0;

      var Aoffsetx = a.Offsetx ?? 0;
      var Aoffsety = a.Offsety ?? 0;

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
          p = new SvgPoint(edgeB[i].X + Boffsetx, edgeB[i].Y + Boffsety);
          s1 = new SvgPoint(edgeA[j].X + Aoffsetx, edgeA[j].Y + Aoffsety);
          s2 = new SvgPoint(edgeA[j + 1].X + Aoffsetx, edgeA[j + 1].Y + Aoffsety);

          if (Math.Abs(((s2.Y - s1.Y) * direction.X) - ((s2.X - s1.X) * direction.Y)) < TOL)
          {
            continue;
          }

          // project point, ignore edge boundaries
          d = pointDistance(p, s1, s2, direction);

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

    public static double polygonArea(INfp polygon)
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
    public static bool? pointInPolygon(SvgPoint point, INfp polygon)
    {
      if (polygon == null || polygon.Points.Length < 3)
      {
        throw new ArgumentException();
      }

      var inside = false;

      // var offsetx = polygon.Offsetx || 0;
      // var offsety = polygon.Offsety || 0;
      var offsetx = polygon.Offsetx == null ? 0 : polygon.Offsetx.Value;
      var offsety = polygon.Offsety == null ? 0 : polygon.Offsety.Value;

      int i, j;
      for (i = 0, j = polygon.Points.Count() - 1; i < polygon.Points.Length; j = i++)
      {
        var xi = polygon.Points[i].X + offsetx;
        var yi = polygon.Points[i].Y + offsety;
        var xj = polygon.Points[j].X + offsetx;
        var yj = polygon.Points[j].Y + offsety;

        if (_almostEqual(xi, point.X) && _almostEqual(yi, point.Y))
        {
          return null; // no result
        }

        if (_onSegment(new SvgPoint(xi, yi), new SvgPoint(xj, yj), point))
        {
          return null; // exactly on the segment
        }

        if (_almostEqual(xi, xj) && _almostEqual(yi, yj))
        { // ignore very small lines
          continue;
        }

        var intersect = ((yi > point.Y) != (yj > point.Y)) && (point.X < ((xj - xi) * (point.Y - yi) / (yj - yi)) + xi);
        if (intersect)
        {
          inside = !inside;
        }
      }

      return inside;
    }

    // todo: swap this for a more efficient sweep-line implementation
    // returnEdges: if set, return all edges on A that have intersections
    public static bool intersect(INfp a, INfp b)
    {
      var aOffsetx = a.Offsetx ?? 0;
      var aOffsety = a.Offsety ?? 0;

      var bOffsetx = b.Offsetx ?? 0;
      var bOffsety = b.Offsety ?? 0;

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

          var prevbindex = (j == 0) ? b.Length - 1 : j - 1;
          var prevaindex = (i == 0) ? a.Length - 1 : i - 1;
          var nextbindex = (j + 1 == b.Length - 1) ? 0 : j + 2;
          var nextaindex = (i + 1 == a.Length - 1) ? 0 : i + 2;

          // go even further back if we happen to hit on a loop end point
          if (b[prevbindex] == b[j] || (_almostEqual(b[prevbindex].X, b[j].X) && _almostEqual(b[prevbindex].Y, b[j].Y)))
          {
            prevbindex = (prevbindex == 0) ? b.Length - 1 : prevbindex - 1;
          }

          if (a[prevaindex] == a[i] || (_almostEqual(a[prevaindex].X, a[i].X) && _almostEqual(a[prevaindex].Y, a[i].Y)))
          {
            prevaindex = (prevaindex == 0) ? a.Length - 1 : prevaindex - 1;
          }

          // go even further forward if we happen to hit on a loop end point
          if (b[nextbindex] == b[j + 1] || (_almostEqual(b[nextbindex].X, b[j + 1].X) && _almostEqual(b[nextbindex].Y, b[j + 1].Y)))
          {
            nextbindex = (nextbindex == b.Length - 1) ? 0 : nextbindex + 1;
          }

          if (a[nextaindex] == a[i + 1] || (_almostEqual(a[nextaindex].X, a[i + 1].X) && _almostEqual(a[nextaindex].Y, a[i + 1].Y)))
          {
            nextaindex = (nextaindex == a.Length - 1) ? 0 : nextaindex + 1;
          }

          var a0 = new SvgPoint(a[prevaindex].X + aOffsetx, a[prevaindex].Y + aOffsety);
          var b0 = new SvgPoint(b[prevbindex].X + bOffsetx, b[prevbindex].Y + bOffsety);

          var a3 = new SvgPoint(a[nextaindex].X + aOffsetx, a[nextaindex].Y + aOffsety);
          var b3 = new SvgPoint(b[nextbindex].X + bOffsetx, b[nextbindex].Y + bOffsety);

          if (_onSegment(a1, a2, b1) || (_almostEqual(a1.X, b1.X) && _almostEqual(a1.Y, b1.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var b0in = pointInPolygon(b0, a);
            var b2in = pointInPolygon(b2, a);
            if ((b0in == true && b2in == false) || (b0in == false && b2in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          if (_onSegment(a1, a2, b2) || (_almostEqual(a2.X, b2.X) && _almostEqual(a2.Y, b2.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var b1in = pointInPolygon(b1, a);
            var b3in = pointInPolygon(b3, a);

            if ((b1in == true && b3in == false) || (b1in == false && b3in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          if (_onSegment(b1, b2, a1) || (_almostEqual(a1.X, b2.X) && _almostEqual(a1.Y, b2.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var a0in = pointInPolygon(a0, b);
            var a2in = pointInPolygon(a2, b);

            if ((a0in == true && a2in == false) || (a0in == false && a2in == true))
            {
              return true;
            }
            else
            {
              continue;
            }
          }

          if (_onSegment(b1, b2, a2) || (_almostEqual(a2.X, b1.X) && _almostEqual(a2.Y, b1.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var a1in = pointInPolygon(a1, b);
            var a3in = pointInPolygon(a3, b);

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
        if (Math.Abs(a.X - b.X) > TOL && ((a.X < b.X) ? x < a.X || x > b.X : x > a.X || x < b.X))
        {
          return null;
        }

        if (Math.Abs(a.Y - b.Y) > TOL && ((a.Y < b.Y) ? y < a.Y || y > b.Y : y > a.Y || y < b.Y))
        {
          return null;
        }

        if (Math.Abs(e.X - f.X) > TOL && ((e.X < f.X) ? x < e.X || x > f.X : x > e.X || x < f.X))
        {
          return null;
        }

        if (Math.Abs(e.Y - f.Y) > TOL && ((e.Y < f.Y) ? y < e.Y || y > f.Y : y > e.Y || y < f.Y))
        {
          return null;
        }
      }

      return new SvgPoint(x, y);
    }

    // searches for an arrangement of A and B such that they do not overlap
    // if an NFP is given, only search for startpoints that have not already been traversed in the given NFP
    private static SvgPoint SearchStartPoint(INfp a, INfp b, bool inside, NFP[] nfp = null)
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
            b.Offsetx = a[i].X - b[j].X;
            b.Offsety = a[i].Y - b[j].Y;

            bool? bInside = null;
            for (var k = 0; k < b.Length; k++)
            {
              var inpoly = pointInPolygon(
                  new SvgPoint(
                  b[k].X + b.Offsetx.Value,
                  b[k].Y + b.Offsety.Value), a);
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

            var startPoint = new SvgPoint(b.Offsetx.Value, b.Offsety.Value);
            if (((bInside.Value && inside) || (!bInside.Value && !inside)) &&
                !intersect(a, b) && !inNfp(startPoint, nfp))
            {
              return startPoint;
            }

            // slide B along vector
            var vx = a[i + 1].X - a[i].X;
            var vy = a[i + 1].Y - a[i].Y;

            var d1 = polygonProjectionDistance(a, b, new SvgPoint(vx, vy));
            var d2 = polygonProjectionDistance(b, a, new SvgPoint(-vx, -vy));

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
            if (d != null && !_almostEqual(d, 0) && d > 0)
            {
            }
            else
            {
              continue;
            }

            var vd2 = (vx * vx) + (vy * vy);

            if (d * d < vd2 && !_almostEqual(d * d, vd2))
            {
              var vd = Math.Sqrt((vx * vx) + (vy * vy));
              vx *= d.Value / vd;
              vy *= d.Value / vd;
            }

            b.Offsetx += vx;
            b.Offsety += vy;

            for (var k = 0; k < b.Length; k++)
            {
              var inpoly = pointInPolygon(
                  new SvgPoint(
                   b[k].X + b.Offsetx.Value, b[k].Y + b.Offsety.Value), a);
              if (inpoly != null)
              {
                bInside = inpoly;
                break;
              }
            }

            startPoint =
                                new SvgPoint(b.Offsetx.Value, b.Offsety.Value);
            if (((bInside.Value && inside) || (!bInside.Value && !inside)) &&
                !intersect(a, b) && !inNfp(startPoint, nfp))
            {
              return startPoint;
            }
          }
        }
      }

      return null;
    }

    public class TouchingItem
    {
      public TouchingItem(int type, int a, int b)
      {
        this.A = a;
        this.B = b;
        this.type = type;
      }

      public int A;
      public int B;
      public int type;
    }

    public static double? segmentDistance(SvgPoint a, SvgPoint b, SvgPoint e, SvgPoint f, SvgPoint direction)
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

      var ABmin = Math.Min(dotA, dotB);
      var ABmax = Math.Max(dotA, dotB);

      var EFmax = Math.Max(dotE, dotF);
      var EFmin = Math.Min(dotE, dotF);

      // segments that will merely touch at one point
      if (_almostEqual(ABmax, EFmin, TOL) || _almostEqual(ABmin, EFmax, TOL))
      {
        return null;
      }

      // segments miss eachother completely
      if (ABmax < EFmin || ABmin > EFmax)
      {
        return null;
      }

      double overlap;

      if ((ABmax > EFmax && ABmin < EFmin) || (EFmax > ABmax && EFmin < ABmin))
      {
        overlap = 1;
      }
      else
      {
        var minMax = Math.Min(ABmax, EFmax);
        var maxMin = Math.Max(ABmin, EFmin);

        var maxMax = Math.Max(ABmax, EFmax);
        var minMin = Math.Min(ABmin, EFmin);

        overlap = (minMax - maxMin) / (maxMax - minMin);
      }

      var crossABE = ((e.Y - a.Y) * (b.X - a.X)) - ((e.X - a.X) * (b.Y - a.Y));
      var crossABF = ((f.Y - a.Y) * (b.X - a.X)) - ((f.X - a.X) * (b.Y - a.Y));

      // lines are colinear
      if (_almostEqual(crossABE, 0) && _almostEqual(crossABF, 0))
      {
        var ABnorm = new SvgPoint(b.Y - a.Y, a.X - b.X);
        var EFnorm = new SvgPoint(f.Y - e.Y, e.X - f.X);

        var ABnormlength = Math.Sqrt((ABnorm.X * ABnorm.X) + (ABnorm.Y * ABnorm.Y));
        ABnorm.X /= ABnormlength;
        ABnorm.Y /= ABnormlength;

        var EFnormlength = Math.Sqrt((EFnorm.X * EFnorm.X) + (EFnorm.Y * EFnorm.Y));
        EFnorm.X /= EFnormlength;
        EFnorm.Y /= EFnormlength;

        // segment normals must point in opposite directions
        if (Math.Abs((ABnorm.Y * EFnorm.X) - (ABnorm.X * EFnorm.Y)) < TOL && (ABnorm.Y * EFnorm.Y) + (ABnorm.X * EFnorm.X) < 0)
        {
          // normal of AB segment must point in same direction as given direction vector
          var normdot = (ABnorm.Y * direction.Y) + (ABnorm.X * direction.X);

          // the segments merely slide along eachother
          if (_almostEqual(normdot, 0, TOL))
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
      if (_almostEqual(dotA, dotE))
      {
        distances.Add(crossA - crossE);
      }
      else if (_almostEqual(dotA, dotF))
      {
        distances.Add(crossA - crossF);
      }
      else if (dotA > EFmin && dotA < EFmax)
      {
        var d = pointDistance(a, e, f, reverse);
        if (d != null && _almostEqual(d, 0))
        { // A currently touches EF, but AB is moving away from EF
          var dB = pointDistance(b, e, f, reverse, true);
          if (dB < 0 || _almostEqual(dB * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (_almostEqual(dotB, dotE))
      {
        distances.Add(crossB - crossE);
      }
      else if (_almostEqual(dotB, dotF))
      {
        distances.Add(crossB - crossF);
      }
      else if (dotB > EFmin && dotB < EFmax)
      {
        var d = pointDistance(b, e, f, reverse);

        if (d != null && _almostEqual(d, 0))
        { // crossA>crossB A currently touches EF, but AB is moving away from EF
          var dA = pointDistance(a, e, f, reverse, true);
          if (dA < 0 || _almostEqual(dA * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (dotE > ABmin && dotE < ABmax)
      {
        var d = pointDistance(e, a, b, direction);
        if (d != null && _almostEqual(d, 0))
        { // crossF<crossE A currently touches EF, but AB is moving away from EF
          var dF = pointDistance(f, a, b, direction, true);
          if (dF < 0 || _almostEqual(dF * overlap, 0))
          {
            d = null;
          }
        }

        if (d != null)
        {
          distances.Add(d.Value);
        }
      }

      if (dotF > ABmin && dotF < ABmax)
      {
        var d = pointDistance(f, a, b, direction);
        if (d != null && _almostEqual(d, 0))
        { // && crossE<crossF A currently touches EF, but AB is moving away from EF
          var dE = pointDistance(e, a, b, direction, true);
          if (dE < 0 || _almostEqual(dE * overlap, 0))
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

    public static double? polygonSlideDistance(INfp a, INfp b, nVector direction, bool ignoreNegative)
    {
      SvgPoint A1, A2, B1, B2;
      double Aoffsetx, Aoffsety, Boffsetx, Boffsety;

      Aoffsetx = a.Offsetx ?? 0;
      Aoffsety = a.Offsety ?? 0;

      Boffsetx = b.Offsetx ?? 0;
      Boffsety = b.Offsety ?? 0;

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

      var dir = _normalizeVector(new SvgPoint(direction.x, direction.y));

      var normal = new SvgPoint(
          dir.Y,
          -dir.X);

      var reverse = new SvgPoint(-dir.X, -dir.Y);

      for (var i = 0; i < edgeB.Length - 1; i++)
      {
        // var mind = null;
        for (var j = 0; j < edgeA.Length - 1; j++)
        {
          A1 = new SvgPoint(
               edgeA[j].X + Aoffsetx, edgeA[j].Y + Aoffsety);
          A2 = new SvgPoint(
              edgeA[j + 1].X + Aoffsetx, edgeA[j + 1].Y + Aoffsety);
          B1 = new SvgPoint(edgeB[i].X + Boffsetx, edgeB[i].Y + Boffsety);
          B2 = new SvgPoint(edgeB[i + 1].X + Boffsetx, edgeB[i + 1].Y + Boffsety);

          if ((_almostEqual(A1.X, A2.X) && _almostEqual(A1.Y, A2.Y)) || (_almostEqual(B1.X, B2.X) && _almostEqual(B1.Y, B2.Y)))
          {
            continue; // ignore extremely small lines
          }

          d = segmentDistance(A1, A2, B1, B2, dir);

          if (d != null && (distance == null || d < distance))
          {
            if (!ignoreNegative || d > 0 || _almostEqual(d, 0))
            {
              distance = d;
            }
          }
        }
      }

      return distance;
    }

    public class nVector
    {
      public SvgPoint start;
      public SvgPoint end;
      public double x;
      public double y;

      public nVector(double v1, double v2, SvgPoint start, SvgPoint end)
      {
        this.x = v1;
        this.y = v2;
        this.start = start;
        this.end = end;
      }
    }

    // given a static polygon A and a movable polygon B, compute a no fit polygon by orbiting B about A
    // if the inside flag is set, B is orbited inside of A rather than outside
    // if the searchEdges flag is set, all edges of A are explored for NFPs - multiple
    public static NFP[] noFitPolygon(NFP a, NFP b, bool inside, bool searchEdges)
    {
      if (a == null || a.Length < 3 || b == null || b.Length < 3)
      {
        return null;
      }

      a.Offsetx = 0;
      a.Offsety = 0;

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

      List<NFP> nfpList = new List<NFP>();

      while (startpoint != null)
      {
        b.Offsetx = startpoint.X;
        b.Offsety = startpoint.Y;

        // maintain a list of touching points/edges
        List<TouchingItem> touching = null;

        nVector prevvector = null; // keep track of previous vector
        NFP nfp = new NFP();
        /*var NFP = [{
            x: B[0].x + B.Offsetx,
            y: B[0].y + B.Offsety
        }];*/

        ((IHiddenNfp)nfp).Push(new SvgPoint(b[0].X + b.Offsetx.Value, b[0].Y + b.Offsety.Value));

        double referencex = b[0].X + b.Offsetx.Value;
        double referencey = b[0].Y + b.Offsety.Value;
        var startx = referencex;
        var starty = referencey;
        var counter = 0;

        while (counter < 10 * (a.Length + b.Length))
        { // sanity check, prevent infinite loop
          touching = new List<GeometryUtil.TouchingItem>();

          // find touching vertices/edges
          for (i = 0; i < a.Length; i++)
          {
            var nexti = (i == a.Length - 1) ? 0 : i + 1;
            for (j = 0; j < b.Length; j++)
            {
              var nextj = (j == b.Length - 1) ? 0 : j + 1;
              if (_almostEqual(a[i].X, b[j].X + b.Offsetx) && _almostEqual(a[i].Y, b[j].Y + b.Offsety))
              {
                touching.Add(new TouchingItem(0, i, j));
              }
              else if (_onSegment(a[i], a[nexti],
                  new SvgPoint(b[j].X + b.Offsetx.Value, b[j].Y + b.Offsety.Value)))
              {
                touching.Add(new TouchingItem(1, nexti, j));
              }
              else if (_onSegment(
                  new SvgPoint(
                   b[j].X + b.Offsetx.Value, b[j].Y + b.Offsety.Value),
                  new SvgPoint(
                   b[nextj].X + b.Offsetx.Value, b[nextj].Y + b.Offsety.Value), a[i]))
              {
                touching.Add(new TouchingItem(2, i, nextj));
              }
            }
          }

          // generate translation vectors from touching vertices/edges
          var vectors = new List<nVector>();
          for (i = 0; i < touching.Count; i++)
          {
            var vertexA = a[touching[i].A];
            vertexA.Marked = true;

            // adjacent A vertices
            var prevAindex = touching[i].A - 1;
            var nextAindex = touching[i].A + 1;

            prevAindex = (prevAindex < 0) ? a.Length - 1 : prevAindex; // loop
            nextAindex = (nextAindex >= a.Length) ? 0 : nextAindex; // loop

            var prevA = a[prevAindex];
            var nextA = a[nextAindex];

            // adjacent B vertices
            var vertexB = b[touching[i].B];

            var prevBindex = touching[i].B - 1;
            var nextBindex = touching[i].B + 1;

            prevBindex = (prevBindex < 0) ? b.Length - 1 : prevBindex; // loop
            nextBindex = (nextBindex >= b.Length) ? 0 : nextBindex; // loop

            var prevB = b[prevBindex];
            var nextB = b[nextBindex];

            if (touching[i].type == 0)
            {
              var vA1 = new nVector(
                   prevA.X - vertexA.X,
                   prevA.Y - vertexA.Y,
                   vertexA,
                   prevA);

              var vA2 = new nVector(
                       nextA.X - vertexA.X,
                       nextA.Y - vertexA.Y,
                       vertexA,
                       nextA);

              // B vectors need to be inverted
              var vB1 = new nVector(
                           vertexB.X - prevB.X,
                           vertexB.Y - prevB.Y,
                           prevB,
                           vertexB);

              var vB2 = new nVector(
                               vertexB.X - nextB.X,
                               vertexB.Y - nextB.Y,
                               nextB,
                               vertexB);

              vectors.Add(vA1);
              vectors.Add(vA2);
              vectors.Add(vB1);
              vectors.Add(vB2);
            }
            else if (touching[i].type == 1)
            {
              vectors.Add(new nVector(
                   vertexA.X - (vertexB.X + b.Offsetx.Value),
                   vertexA.Y - (vertexB.Y + b.Offsety.Value),
                   prevA,
                   vertexA));

              vectors.Add(new nVector(
                   prevA.X - (vertexB.X + b.Offsetx.Value),
                   prevA.Y - (vertexB.Y + b.Offsety.Value),
                   vertexA,
                   prevA));
            }
            else if (touching[i].type == 2)
            {
              vectors.Add(new nVector(
                   vertexA.X - (vertexB.X + b.Offsetx.Value),
                   vertexA.Y - (vertexB.Y + b.Offsety.Value),
                   prevB,
                   vertexB));

              vectors.Add(new nVector(
                   vertexA.X - (prevB.X + b.Offsetx.Value),
                   vertexA.Y - (prevB.Y + b.Offsety.Value),
                   vertexB,
                   prevB));
            }
          }

          // todo: there should be a faster way to reject vectors that will cause immediate intersection. For now just check them all
          nVector translate = null;
          double maxd = 0;

          for (i = 0; i < vectors.Count; i++)
          {
            if (vectors[i].x == 0 && vectors[i].y == 0)
            {
              continue;
            }

            // if this vector points us back to where we came from, ignore it.
            // ie cross product = 0, dot product < 0
            if (prevvector != null &&
                (vectors[i].y * prevvector.y) + (vectors[i].x * prevvector.x) < 0)
            {
              // compare magnitude with unit vectors
              var vectorlength = (double)Math.Sqrt((vectors[i].x * vectors[i].x) + (vectors[i].y * vectors[i].y));
              var unitv = new SvgPoint(vectors[i].x / vectorlength, vectors[i].y / vectorlength);

              var prevlength = (double)Math.Sqrt((prevvector.x * prevvector.x) + (prevvector.y * prevvector.y));
              var prevunit = new SvgPoint(prevvector.x / prevlength, prevvector.y / prevlength);

              // we need to scale down to unit vectors to normalize vector length. Could also just do a tan here
              if (Math.Abs((unitv.Y * prevunit.X) - (unitv.X * prevunit.Y)) < 0.0001)
              {
                continue;
              }
            }

            var d = polygonSlideDistance(a, b, vectors[i], true);
            var vecd2 = (vectors[i].x * vectors[i].x) + (vectors[i].y * vectors[i].y);

            if (d == null || d * d > vecd2)
            {
              var vecd = (double)Math.Sqrt((vectors[i].x * vectors[i].x) + (vectors[i].y * vectors[i].y));
              d = vecd;
            }

            if (d != null && d > maxd)
            {
              maxd = d.Value;
              translate = vectors[i];
            }
          }

          if (translate == null || _almostEqual(maxd, 0))
          {
            // didn't close the loop, something went wrong here
            nfp = null;
            break;
          }

          translate.start.Marked = true;
          translate.end.Marked = true;

          prevvector = translate;

          // trim
          var vlength2 = (translate.x * translate.x) + (translate.y * translate.y);
          if (maxd * maxd < vlength2 && !_almostEqual(maxd * maxd, vlength2))
          {
            var scale = (double)Math.Sqrt((maxd * maxd) / vlength2);
            translate.x *= scale;
            translate.y *= scale;
          }

          referencex += translate.x;
          referencey += translate.y;

          if (_almostEqual(referencex, startx) && _almostEqual(referencey, starty))
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
              if (_almostEqual(referencex, nfp[i].X) && _almostEqual(referencey, nfp[i].Y))
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

          b.Offsetx += translate.x;
          b.Offsety += translate.y;

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
  }

  public class PolygonBounds
  {
    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public PolygonBounds(double x, double y, double w, double h)
    {
      X = x;
      Y = y;
      Width = w;
      Height = h;
    }
  }
}
