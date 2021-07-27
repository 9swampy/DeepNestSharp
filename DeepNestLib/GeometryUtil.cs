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
    public static NFP[] noFitPolygonRectangle(NFP A, NFP B)
    {
      var minAx = A[0].X;
      var minAy = A[0].Y;
      var maxAx = A[0].X;
      var maxAy = A[0].Y;

      for (var i = 1; i < A.Length; i++)
      {
        if (A[i].X < minAx)
        {
          minAx = A[i].X;
        }

        if (A[i].Y < minAy)
        {
          minAy = A[i].Y;
        }

        if (A[i].X > maxAx)
        {
          maxAx = A[i].X;
        }

        if (A[i].Y > maxAy)
        {
          maxAy = A[i].Y;
        }
      }

      var minBx = B[0].X;
      var minBy = B[0].Y;
      var maxBx = B[0].X;
      var maxBy = B[0].Y;
      for (int i = 1; i < B.Length; i++)
      {
        if (B[i].X < minBx)
        {
          minBx = B[i].X;
        }

        if (B[i].Y < minBy)
        {
          minBy = B[i].Y;
        }

        if (B[i].X > maxBx)
        {
          maxBx = B[i].X;
        }

        if (B[i].Y > maxBy)
        {
          maxBy = B[i].Y;
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
                    new SvgPoint(minAx - minBx + B[0].X, minAy - minBy + B[0].Y),
                    new SvgPoint(maxAx - maxBx + B[0].X, minAy - minBy + B[0].Y),
                    new SvgPoint(maxAx - maxBx + B[0].X, maxAy - maxBy + B[0].Y),
                    new SvgPoint(minAx - minBx + B[0].X, maxAy - maxBy + B[0].Y),
                }),
      };
      return pnts;
    }

    // returns the rectangular bounding box of the given polygon
    public static PolygonBounds getPolygonBounds(INfp _polygon)
    {
      return getPolygonBounds(_polygon.Points);
    }

    public static PolygonBounds getPolygonBounds(List<SvgPoint
        > polygon)
    {
      return getPolygonBounds(polygon.ToArray());
    }

    public static PolygonBounds getPolygonBounds(SvgPoint[] polygon)
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
      var bb = getPolygonBounds(poly);
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
      var bounds = GeometryUtil.getPolygonBounds(ret);
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
    public static bool _onSegment(SvgPoint A, SvgPoint B, SvgPoint p)
    {
      // vertical line
      if (_almostEqual(A.X, B.X) && _almostEqual(p.X, A.X))
      {
        if (!_almostEqual(p.Y, B.Y) && !_almostEqual(p.Y, A.Y) && p.Y < Math.Max(B.Y, A.Y) && p.Y > Math.Min(B.Y, A.Y))
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // horizontal line
      if (_almostEqual(A.Y, B.Y) && _almostEqual(p.Y, A.Y))
      {
        if (!_almostEqual(p.X, B.X) && !_almostEqual(p.X, A.X) && p.X < Math.Max(B.X, A.X) && p.X > Math.Min(B.X, A.X))
        {
          return true;
        }
        else
        {
          return false;
        }
      }

      // range check
      if ((p.X < A.X && p.X < B.X) || (p.X > A.X && p.X > B.X) || (p.Y < A.Y && p.Y < B.Y) || (p.Y > A.Y && p.Y > B.Y))
      {
        return false;
      }

      // exclude end points
      if ((_almostEqual(p.X, A.X) && _almostEqual(p.Y, A.Y)) || (_almostEqual(p.X, B.X) && _almostEqual(p.Y, B.Y)))
      {
        return false;
      }

      var cross = ((p.Y - A.Y) * (B.X - A.X)) - ((p.X - A.X) * (B.Y - A.Y));

      if (Math.Abs(cross) > TOL)
      {
        return false;
      }

      var dot = ((p.X - A.X) * (B.X - A.X)) + ((p.Y - A.Y) * (B.Y - A.Y));

      if (dot < 0 || _almostEqual(dot, 0))
      {
        return false;
      }

      var len2 = ((B.X - A.X) * (B.X - A.X)) + ((B.Y - A.Y) * (B.Y - A.Y));

      if (dot > len2 || _almostEqual(dot, len2))
      {
        return false;
      }

      return true;
    }

    // project each point of B onto A in the given direction, and return the
    public static double? polygonProjectionDistance(INfp A, INfp B, SvgPoint direction)
    {
      var Boffsetx = B.Offsetx ?? 0;
      var Boffsety = B.Offsety ?? 0;

      var Aoffsetx = A.Offsetx ?? 0;
      var Aoffsety = A.Offsety ?? 0;

      A = A.Slice(0);
      B = B.Slice(0);

      // close the loop for polygons
      if (A[0] != A[A.Length - 1])
      {
        ((IHiddenNfp)A).Push(A[0]);
      }

      if (B[0] != B[B.Length - 1])
      {
        ((IHiddenNfp)B).Push(B[0]);
      }

      var edgeA = A;
      var edgeB = B;

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
    public static bool intersect(INfp A, INfp B)
    {
      var aOffsetx = A.Offsetx ?? 0;
      var aOffsety = A.Offsety ?? 0;

      var bOffsetx = B.Offsetx ?? 0;
      var bOffsety = B.Offsety ?? 0;

      A = A.Slice(0);
      B = B.Slice(0);

      for (var i = 0; i < A.Length - 1; i++)
      {
        for (var j = 0; j < B.Length - 1; j++)
        {
          var a1 = new SvgPoint(A[i].X + aOffsetx, A[i].Y + aOffsety);
          var a2 = new SvgPoint(A[i + 1].X + aOffsetx, A[i + 1].Y + aOffsety);
          var b1 = new SvgPoint(B[j].X + bOffsetx, B[j].Y + bOffsety);
          var b2 = new SvgPoint(B[j + 1].X + bOffsetx, B[j + 1].Y + bOffsety);

          var prevbindex = (j == 0) ? B.Length - 1 : j - 1;
          var prevaindex = (i == 0) ? A.Length - 1 : i - 1;
          var nextbindex = (j + 1 == B.Length - 1) ? 0 : j + 2;
          var nextaindex = (i + 1 == A.Length - 1) ? 0 : i + 2;

          // go even further back if we happen to hit on a loop end point
          if (B[prevbindex] == B[j] || (_almostEqual(B[prevbindex].X, B[j].X) && _almostEqual(B[prevbindex].Y, B[j].Y)))
          {
            prevbindex = (prevbindex == 0) ? B.Length - 1 : prevbindex - 1;
          }

          if (A[prevaindex] == A[i] || (_almostEqual(A[prevaindex].X, A[i].X) && _almostEqual(A[prevaindex].Y, A[i].Y)))
          {
            prevaindex = (prevaindex == 0) ? A.Length - 1 : prevaindex - 1;
          }

          // go even further forward if we happen to hit on a loop end point
          if (B[nextbindex] == B[j + 1] || (_almostEqual(B[nextbindex].X, B[j + 1].X) && _almostEqual(B[nextbindex].Y, B[j + 1].Y)))
          {
            nextbindex = (nextbindex == B.Length - 1) ? 0 : nextbindex + 1;
          }

          if (A[nextaindex] == A[i + 1] || (_almostEqual(A[nextaindex].X, A[i + 1].X) && _almostEqual(A[nextaindex].Y, A[i + 1].Y)))
          {
            nextaindex = (nextaindex == A.Length - 1) ? 0 : nextaindex + 1;
          }

          var a0 = new SvgPoint(A[prevaindex].X + aOffsetx, A[prevaindex].Y + aOffsety);
          var b0 = new SvgPoint(B[prevbindex].X + bOffsetx, B[prevbindex].Y + bOffsety);

          var a3 = new SvgPoint(A[nextaindex].X + aOffsetx, A[nextaindex].Y + aOffsety);
          var b3 = new SvgPoint(B[nextbindex].X + bOffsetx, B[nextbindex].Y + bOffsety);

          if (_onSegment(a1, a2, b1) || (_almostEqual(a1.X, b1.X) && _almostEqual(a1.Y, b1.Y)))
          {
            // if a point is on a segment, it could intersect or it could not. Check via the neighboring points
            var b0in = pointInPolygon(b0, A);
            var b2in = pointInPolygon(b2, A);
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
            var b1in = pointInPolygon(b1, A);
            var b3in = pointInPolygon(b3, A);

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
            var a0in = pointInPolygon(a0, B);
            var a2in = pointInPolygon(a2, B);

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
            var a1in = pointInPolygon(a1, B);
            var a3in = pointInPolygon(a3, B);

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
    private static SvgPoint LineIntersect(SvgPoint A, SvgPoint B, SvgPoint E, SvgPoint F, bool infinite = false)
    {
      double a1, a2, b1, b2, c1, c2, x, y;

      a1 = B.Y - A.Y;
      b1 = A.X - B.X;
      c1 = (B.X * A.Y) - (A.X * B.Y);
      a2 = F.Y - E.Y;
      b2 = E.X - F.X;
      c2 = (F.X * E.Y) - (E.X * F.Y);

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
        if (Math.Abs(A.X - B.X) > TOL && ((A.X < B.X) ? x < A.X || x > B.X : x > A.X || x < B.X))
        {
          return null;
        }

        if (Math.Abs(A.Y - B.Y) > TOL && ((A.Y < B.Y) ? y < A.Y || y > B.Y : y > A.Y || y < B.Y))
        {
          return null;
        }

        if (Math.Abs(E.X - F.X) > TOL && ((E.X < F.X) ? x < E.X || x > F.X : x > E.X || x < F.X))
        {
          return null;
        }

        if (Math.Abs(E.Y - F.Y) > TOL && ((E.Y < F.Y) ? y < E.Y || y > F.Y : y > E.Y || y < F.Y))
        {
          return null;
        }
      }

      return new SvgPoint(x, y);
    }

    // searches for an arrangement of A and B such that they do not overlap
    // if an NFP is given, only search for startpoints that have not already been traversed in the given NFP
    private static SvgPoint SearchStartPoint(INfp A, INfp B, bool inside, NFP[] NFP = null)
    {
      // clone arrays
      A = A.Slice(0);
      B = B.Slice(0);

      // close the loop for polygons
      if (A[0] != A[A.Length - 1])
      {
        ((IHiddenNfp)A).Push(A[0]);
      }

      if (B[0] != B[B.Length - 1])
      {
        ((IHiddenNfp)B).Push(B[0]);
      }

      for (var i = 0; i < A.Length - 1; i++)
      {
        if (!A[i].Marked)
        {
          A[i].Marked = true;
          for (var j = 0; j < B.Length; j++)
          {
            B.Offsetx = A[i].X - B[j].X;
            B.Offsety = A[i].Y - B[j].Y;

            bool? bInside = null;
            for (var k = 0; k < B.Length; k++)
            {
              var inpoly = pointInPolygon(
                  new SvgPoint(
                  B[k].X + B.Offsetx.Value,
                  B[k].Y + B.Offsety.Value), A);
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

            var startPoint = new SvgPoint(B.Offsetx.Value, B.Offsety.Value);
            if (((bInside.Value && inside) || (!bInside.Value && !inside)) &&
                !intersect(A, B) && !inNfp(startPoint, NFP))
            {
              return startPoint;
            }

            // slide B along vector
            var vx = A[i + 1].X - A[i].X;
            var vy = A[i + 1].Y - A[i].Y;

            var d1 = polygonProjectionDistance(A, B, new SvgPoint(vx, vy));
            var d2 = polygonProjectionDistance(B, A, new SvgPoint(-vx, -vy));

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

            B.Offsetx += vx;
            B.Offsety += vy;

            for (var k = 0; k < B.Length; k++)
            {
              var inpoly = pointInPolygon(
                  new SvgPoint(
                   B[k].X + B.Offsetx.Value, B[k].Y + B.Offsety.Value), A);
              if (inpoly != null)
              {
                bInside = inpoly;
                break;
              }
            }

            startPoint =
                                new SvgPoint(B.Offsetx.Value, B.Offsety.Value);
            if (((bInside.Value && inside) || (!bInside.Value && !inside)) &&
                !intersect(A, B) && !inNfp(startPoint, NFP))
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
      public TouchingItem(int _type, int _a, int _b)
      {
        this.A = _a;
        this.B = _b;
        this.type = _type;
      }

      public int A;
      public int B;
      public int type;
    }

    public static double? segmentDistance(SvgPoint A, SvgPoint B, SvgPoint E, SvgPoint F, SvgPoint direction)
    {
      var normal = new SvgPoint(
          direction.Y,
          -direction.X);

      var reverse = new SvgPoint(
              -direction.X,
              -direction.Y);

      var dotA = (A.X * normal.X) + (A.Y * normal.Y);
      var dotB = (B.X * normal.X) + (B.Y * normal.Y);
      var dotE = (E.X * normal.X) + (E.Y * normal.Y);
      var dotF = (F.X * normal.X) + (F.Y * normal.Y);

      var crossA = (A.X * direction.X) + (A.Y * direction.Y);
      var crossB = (B.X * direction.X) + (B.Y * direction.Y);
      var crossE = (E.X * direction.X) + (E.Y * direction.Y);
      var crossF = (F.X * direction.X) + (F.Y * direction.Y);

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

      var crossABE = ((E.Y - A.Y) * (B.X - A.X)) - ((E.X - A.X) * (B.Y - A.Y));
      var crossABF = ((F.Y - A.Y) * (B.X - A.X)) - ((F.X - A.X) * (B.Y - A.Y));

      // lines are colinear
      if (_almostEqual(crossABE, 0) && _almostEqual(crossABF, 0))
      {
        var ABnorm = new SvgPoint(B.Y - A.Y, A.X - B.X);
        var EFnorm = new SvgPoint(F.Y - E.Y, E.X - F.X);

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
        var d = pointDistance(A, E, F, reverse);
        if (d != null && _almostEqual(d, 0))
        { // A currently touches EF, but AB is moving away from EF
          var dB = pointDistance(B, E, F, reverse, true);
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
        var d = pointDistance(B, E, F, reverse);

        if (d != null && _almostEqual(d, 0))
        { // crossA>crossB A currently touches EF, but AB is moving away from EF
          var dA = pointDistance(A, E, F, reverse, true);
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
        var d = pointDistance(E, A, B, direction);
        if (d != null && _almostEqual(d, 0))
        { // crossF<crossE A currently touches EF, but AB is moving away from EF
          var dF = pointDistance(F, A, B, direction, true);
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
        var d = pointDistance(F, A, B, direction);
        if (d != null && _almostEqual(d, 0))
        { // && crossE<crossF A currently touches EF, but AB is moving away from EF
          var dE = pointDistance(E, A, B, direction, true);
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

    public static double? polygonSlideDistance(INfp A, INfp B, nVector direction, bool ignoreNegative)
    {
      SvgPoint A1, A2, B1, B2;
      double Aoffsetx, Aoffsety, Boffsetx, Boffsety;

      Aoffsetx = A.Offsetx ?? 0;
      Aoffsety = A.Offsety ?? 0;

      Boffsetx = B.Offsetx ?? 0;
      Boffsety = B.Offsety ?? 0;

      A = A.Slice(0);
      B = B.Slice(0);

      // close the loop for polygons
      if (A[0] != A[A.Length - 1])
      {
        ((IHiddenNfp)A).Push(A[0]);
      }

      if (B[0] != B[B.Length - 1])
      {
        ((IHiddenNfp)B).Push(B[0]);
      }

      var edgeA = A;
      var edgeB = B;

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

      public nVector(double v1, double v2, SvgPoint _start, SvgPoint _end)
      {
        this.x = v1;
        this.y = v2;
        this.start = _start;
        this.end = _end;
      }
    }

    // given a static polygon A and a movable polygon B, compute a no fit polygon by orbiting B about A
    // if the inside flag is set, B is orbited inside of A rather than outside
    // if the searchEdges flag is set, all edges of A are explored for NFPs - multiple
    public static NFP[] noFitPolygon(NFP A, NFP B, bool inside, bool searchEdges)
    {
      if (A == null || A.Length < 3 || B == null || B.Length < 3)
      {
        return null;
      }

      A.Offsetx = 0;
      A.Offsety = 0;

      int i = 0, j = 0;

      var minA = A[0].Y;
      var minAindex = 0;

      var maxB = B[0].Y;
      var maxBindex = 0;

      for (i = 1; i < A.Length; i++)
      {
        A[i].Marked = false;
        if (A[i].Y < minA)
        {
          minA = A[i].Y;
          minAindex = i;
        }
      }

      for (i = 1; i < B.Length; i++)
      {
        B[i].Marked = false;
        if (B[i].Y > maxB)
        {
          maxB = B[i].Y;
          maxBindex = i;
        }
      }

      SvgPoint startpoint;
      if (!inside)
      {
        // shift B such that the bottom-most point of B is at the top-most point of A. This guarantees an initial placement with no intersections
        startpoint = new SvgPoint(
             A[minAindex].X - B[maxBindex].X,
             A[minAindex].Y - B[maxBindex].Y);
      }
      else
      {
        // no reliable heuristic for inside
        startpoint = SearchStartPoint(A, B, true);
      }

      List<NFP> nfpList = new List<NFP>();

      while (startpoint != null)
      {
        B.Offsetx = startpoint.X;
        B.Offsety = startpoint.Y;

        // maintain a list of touching points/edges
        List<TouchingItem> touching = null;

        nVector prevvector = null; // keep track of previous vector
        NFP nfp = new NFP();
        /*var NFP = [{
            x: B[0].x + B.Offsetx,
            y: B[0].y + B.Offsety
        }];*/

        ((IHiddenNfp)nfp).Push(new SvgPoint(B[0].X + B.Offsetx.Value, B[0].Y + B.Offsety.Value));

        double referencex = B[0].X + B.Offsetx.Value;
        double referencey = B[0].Y + B.Offsety.Value;
        var startx = referencex;
        var starty = referencey;
        var counter = 0;

        while (counter < 10 * (A.Length + B.Length))
        { // sanity check, prevent infinite loop
          touching = new List<GeometryUtil.TouchingItem>();

          // find touching vertices/edges
          for (i = 0; i < A.Length; i++)
          {
            var nexti = (i == A.Length - 1) ? 0 : i + 1;
            for (j = 0; j < B.Length; j++)
            {
              var nextj = (j == B.Length - 1) ? 0 : j + 1;
              if (_almostEqual(A[i].X, B[j].X + B.Offsetx) && _almostEqual(A[i].Y, B[j].Y + B.Offsety))
              {
                touching.Add(new TouchingItem(0, i, j));
              }
              else if (_onSegment(A[i], A[nexti],
                  new SvgPoint(B[j].X + B.Offsetx.Value, B[j].Y + B.Offsety.Value)))
              {
                touching.Add(new TouchingItem(1, nexti, j));
              }
              else if (_onSegment(
                  new SvgPoint(
                   B[j].X + B.Offsetx.Value, B[j].Y + B.Offsety.Value),
                  new SvgPoint(
                   B[nextj].X + B.Offsetx.Value, B[nextj].Y + B.Offsety.Value), A[i]))
              {
                touching.Add(new TouchingItem(2, i, nextj));
              }
            }
          }

          // generate translation vectors from touching vertices/edges
          var vectors = new List<nVector>();
          for (i = 0; i < touching.Count; i++)
          {
            var vertexA = A[touching[i].A];
            vertexA.Marked = true;

            // adjacent A vertices
            var prevAindex = touching[i].A - 1;
            var nextAindex = touching[i].A + 1;

            prevAindex = (prevAindex < 0) ? A.Length - 1 : prevAindex; // loop
            nextAindex = (nextAindex >= A.Length) ? 0 : nextAindex; // loop

            var prevA = A[prevAindex];
            var nextA = A[nextAindex];

            // adjacent B vertices
            var vertexB = B[touching[i].B];

            var prevBindex = touching[i].B - 1;
            var nextBindex = touching[i].B + 1;

            prevBindex = (prevBindex < 0) ? B.Length - 1 : prevBindex; // loop
            nextBindex = (nextBindex >= B.Length) ? 0 : nextBindex; // loop

            var prevB = B[prevBindex];
            var nextB = B[nextBindex];

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
                   vertexA.X - (vertexB.X + B.Offsetx.Value),
                   vertexA.Y - (vertexB.Y + B.Offsety.Value),
                   prevA,
                   vertexA));

              vectors.Add(new nVector(
                   prevA.X - (vertexB.X + B.Offsetx.Value),
                   prevA.Y - (vertexB.Y + B.Offsety.Value),
                   vertexA,
                   prevA));
            }
            else if (touching[i].type == 2)
            {
              vectors.Add(new nVector(
                   vertexA.X - (vertexB.X + B.Offsetx.Value),
                   vertexA.Y - (vertexB.Y + B.Offsety.Value),
                   prevB,
                   vertexB));

              vectors.Add(new nVector(
                   vertexA.X - (prevB.X + B.Offsetx.Value),
                   vertexA.Y - (prevB.Y + B.Offsety.Value),
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

            var d = polygonSlideDistance(A, B, vectors[i], true);
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

          B.Offsetx += translate.x;
          B.Offsety += translate.y;

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

        startpoint = SearchStartPoint(A, B, inside, nfpList.ToArray());
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
