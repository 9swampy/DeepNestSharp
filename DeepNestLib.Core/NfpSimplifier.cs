namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
#if NCRUNCH
  using System.Diagnostics;
#endif
  using System.Linq;
  using ClipperLib;
  using DeepNestLib.Geometry;

  internal partial class NfpSimplifier
  {
    private const bool UsePolygonSimplificationCache = true;
    private const bool DoSimplifyRadialDist = false;
    private const bool DoSimplifyDouglasPeucker = true;

    private static volatile object cacheSyncLock = new object();

    private static readonly PolygonSimplificationDictionary PolygonSimplificationCache = new PolygonSimplificationDictionary();

    public static INfp SimplifyFunction(INfp polygon, bool inside, ISvgNestConfig config)
    {
      return SimplifyFunction(polygon, inside, config.CurveTolerance, config.Simplify);
    }

    /// <summary>
    /// Override specifically added for Tests so the curveTolerance can be passed in.
    /// </summary>
    internal static INfp SimplifyFunction(INfp polygon, bool inside, double curveTolerance, bool useHull)
    {
      var markExact = true;
      var straighten = true;
      var doSimplify = true;
      var selectiveReversalOfOffset = true;

      var tolerance = 4 * curveTolerance;

      // give special treatment to line segments above this length (squared)
      var fixedTolerance = 40 * curveTolerance * 40 * curveTolerance;
      int j;

      var hull = polygon.GetHull();
      if (useHull)
      {
        /*
        // use convex hull
        var hull = new ConvexHullGrahamScan();
        for(var i=0; i<polygon.length; i++){
            hull.addPoint(polygon[i].x, polygon[i].y);
        }

        return hull.getHull();*/

        if (hull != null)
        {
          return hull;
        }
        else
        {
          return polygon;
        }
      }

      var cleaned = SvgNest.CleanPolygon2(polygon);
      if (cleaned != null && cleaned.Length > 1)
      {
        polygon = cleaned;
      }
      else
      {
        return polygon;
      }

      SvgPoint[] resultSource;
      if (UsePolygonSimplificationCache && PolygonSimplificationCache.TryGetValue(new PolygonSimplificationKey(polygon.Points, curveTolerance, DoSimplifyRadialDist, DoSimplifyDouglasPeucker), out resultSource))
      {
        return new NoFitPolygon(resultSource);
      }

      INfp simple = null;
      if (doSimplify)
      {
        // polygon to polyline
        var copy = polygon.Slice(0);
        ((IHiddenNfp)copy).Push(copy[0]);

        // mark all segments greater than ~0.25 in to be kept
        // the PD simplification algo doesn't care about the accuracy of long lines, only the absolute distance of each point
        // we care a great deal
        for (int i = 0; i < copy.Length - 1; i++)
        {
          var p1 = copy[i];
          var p2 = copy[i + 1];
          var sqd = ((p2.X - p1.X) * (p2.X - p1.X)) + ((p2.Y - p1.Y) * (p2.Y - p1.Y));
          if (sqd > fixedTolerance)
          {
            p1.Marked = true;
            p2.Marked = true;
          }
        }

        simple = Simplify.SimplifyPolygon(copy.Points, tolerance, DoSimplifyRadialDist, DoSimplifyDouglasPeucker);

        // now a polygon again
        // simple.pop();
        simple.ReplacePoints(simple.Points.Take(simple.Points.Count() - 1));

        // could be dirty again (self intersections and/or coincident points)
        simple = SvgNest.CleanPolygon2(simple);
      }

      // simplification process reduced poly to a line or point; or it came back just as complex as the original
      if (simple == null || simple.Points.Count() > polygon.Points.Count() * 0.9)
      {
        simple = polygon;
      }

      var offsets = SvgNest.PolygonOffsetDeepNest(simple, inside ? -tolerance : tolerance);

      INfp offset = null;
      double offsetArea = 0;
      List<INfp> holes = new List<INfp>();
      for (int i = 0; i < offsets.Length; i++)
      {
        var area = GeometryUtil.PolygonArea(offsets[i]);
        if (offset == null || area < offsetArea)
        {
          offset = offsets[i];
          offsetArea = area;
        }

        if (area > 0)
        {
          holes.Add(offsets[i]);
        }
      }

      if (markExact)
      {
        MarkExact(polygon, simple, curveTolerance);
      }

      var numshells = 4;
      INfp[] shells = new INfp[numshells];

      for (j = 1; j < numshells; j++)
      {
        var delta = j * (tolerance / numshells);
        delta = inside ? -delta : delta;
        var shell = SvgNest.PolygonOffsetDeepNest(simple, delta);
        if (shell.Count() > 0)
        {
          shells[j] = shell.First();
        }
        else
        {
          // shells[j] = shell;
        }
      }

      if (offset == null)
      {
        return polygon;
      }

      if (selectiveReversalOfOffset)
      {
        // selective reversal of offset
        for (int i = 0; i < offset.Length; i++)
        {
          if (i % 10 == 0)
          {
            System.Diagnostics.Debug.Print($"{polygon.Name} selectiveReversalOfOffset {i} of {offset.Length}");
          }

          var o = offset[i];
          var target = GetTarget(o, simple, 2 * tolerance);

          // reverse point offset and try to find exterior points
          var test = offset.CloneTop();
          test.Points[i] = new SvgPoint(target.X, target.Y);

          if (inside ? Interior(test, polygon) : Exterior(test, polygon))
          {
            // a shell is an intermediate offset between simple and offset
            for (j = 1; j < numshells; j++)
            {
              if (shells[j] != null)
              {
                var shell = shells[j];
                var delta = j * (tolerance / numshells);
                target = GetTarget(o, shell, 2 * delta);
                test = offset.CloneTop();
                test.Points[i] = new SvgPoint(target.X, target.Y);
                if (inside ? !Interior(test, polygon) : !Exterior(test, polygon))
                {
                  o.X = target.X;
                  o.Y = target.Y;
                  break;
                }
              }
            }
          }
          else
          {
            o.X = target.X;
            o.Y = target.Y;
          }
        }
      }

      if (straighten)
      {
        // straighten long lines
        // a rounded rectangle would still have issues at this point, as the long sides won't line up straight
        var straightened = false;

        for (int i = 0; i < offset.Length; i++)
        {
          var p1 = offset[i];
          var p2 = offset[i + 1 == offset.Length ? 0 : i + 1];

          var sqd = ((p2.X - p1.X) * (p2.X - p1.X)) + ((p2.Y - p1.Y) * (p2.Y - p1.Y));

          if (sqd < fixedTolerance)
          {
            continue;
          }

          for (j = 0; j < simple.Length; j++)
          {
            var s1 = simple[j];
            var s2 = simple[j + 1 == simple.Length ? 0 : j + 1];

            var sqds = ((p2.X - p1.X) * (p2.X - p1.X)) + ((p2.Y - p1.Y) * (p2.Y - p1.Y));

            if (sqds < fixedTolerance)
            {
              continue;
            }

            if ((GeometryUtil.AlmostEqual(s1.X, s2.X) || GeometryUtil.AlmostEqual(s1.Y, s2.Y)) && // we only really care about vertical and horizontal lines
            GeometryUtil.WithinDistance(p1, s1, 2 * tolerance) &&
            GeometryUtil.WithinDistance(p2, s2, 2 * tolerance) &&
            (!GeometryUtil.WithinDistance(p1, s1, curveTolerance / 1000) ||
            !GeometryUtil.WithinDistance(p2, s2, curveTolerance / 1000)))
            {
              p1.X = s1.X;
              p1.Y = s1.Y;
              p2.X = s2.X;
              p2.Y = s2.Y;
              straightened = true;
            }
          }
        }

        if (straightened)
        {
          var Ac = DeepNestClipper.ScaleUpPath(offset.Points, 10000000);
          var Bc = DeepNestClipper.ScaleUpPath(polygon.Points, 10000000);

          var combined = new List<List<IntPoint>>();
          var clipper = new ClipperLib.Clipper();

          clipper.AddPath(Ac, ClipperLib.PolyType.ptSubject, true);
          clipper.AddPath(Bc, ClipperLib.PolyType.ptSubject, true);

          // the line straightening may have made the offset smaller than the simplified
          if (clipper.Execute(ClipperLib.ClipType.ctUnion, combined, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
          {
            double? largestArea = null;
            for (int i = 0; i < combined.Count; i++)
            {
              var n = combined[i].ToArray().ToNestCoordinates(10000000);
              var sarea = -GeometryUtil.PolygonArea(n);
              if (largestArea == null || largestArea < sarea)
              {
                offset = n;
                largestArea = sarea;
              }
            }
          }
        }
      }

      cleaned = SvgNest.CleanPolygon2(offset);
      if (cleaned != null && cleaned.Length > 1)
      {
        offset = cleaned;
      }

      if (SvgNest.Config.ClipByHull)
      {
        offset = ClipSubject(offset, hull, SvgNest.Config.ClipperScale);
      }

      if (markExact)
      {
        MarkExactSvg(polygon, offset, curveTolerance);
      }

      if (!inside && holes != null && holes.Count > 0)
      {
        offset.Children = holes;
      }

      if (UsePolygonSimplificationCache)
      {
        lock (cacheSyncLock)
        {
          if (!PolygonSimplificationCache.ContainsKey(new PolygonSimplificationKey(polygon.Points, curveTolerance, DoSimplifyRadialDist, DoSimplifyDouglasPeucker)))
          {
            PolygonSimplificationCache.Add(new PolygonSimplificationKey(polygon.Points, curveTolerance, DoSimplifyRadialDist, DoSimplifyDouglasPeucker), offset.Points);
          }
        }
      }

      return offset;
    }

    /// <summary>
    /// Clip the subject so it stays inside the clipBounds.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="clipBounds"></param>
    /// <param name="clipperScale"></param>
    /// <returns></returns>
    internal static INfp ClipSubject(INfp subject, INfp clipBounds, double clipperScale)
    {
      List<List<IntPoint>> finalNfp;
      if (TryClipSubject(subject, clipBounds, clipperScale, out finalNfp) && finalNfp != null && finalNfp.Count > 0)
      {
        return finalNfp[0].ToArray().ToNestCoordinates(clipperScale);
      }

      return subject;
    }

    internal static bool IsIntersect(INfp subject, INfp clipBounds, double clipperScale)
    {
      List<List<IntPoint>> intersect;
      if (TryClipSubject(subject, clipBounds, clipperScale, out intersect) && intersect != null && intersect.Count > 0)
      {
        return true;
      }

      return false;
    }

    internal static bool TryClipSubject(INfp subject, INfp clipBounds, double clipperScale, out List<List<IntPoint>> intersect)
    {
      Clipper clipper;
      var clipperSubject = NfpHelper.InnerNfpToClipperCoordinates(new INfp[] { subject }, clipperScale);
      var clipperClip = NfpHelper.InnerNfpToClipperCoordinates(new INfp[] { clipBounds }, clipperScale);

      clipper = new Clipper();
      clipper.AddPaths(clipperClip, PolyType.ptClip, true);
      clipper.AddPaths(clipperSubject, PolyType.ptSubject, true);

      intersect = new List<List<IntPoint>>();
      return clipper.Execute(ClipType.ctIntersection, intersect, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
    }

    /// <summary>
    /// Mark any points that are exact (for line merge detection).
    /// </summary>
    /// <param name="polygon">This is the one that's checked against.</param>
    /// <param name="offset">This is iterated and elements are marked exact when matched to a point in polygon.</param>
    private static void MarkExactSvg(INfp polygon, INfp offset, double curveTolerance)
    {
      int i;
      for (i = 0; i < offset.Length; i++)
      {
        var seg = new SvgPoint[] { offset[i], offset[i + 1 == offset.Length ? 0 : i + 1] };
        var index1 = Find(seg[0], polygon, curveTolerance);
        var index2 = Find(seg[1], polygon, curveTolerance);
        if (index1 == null)
        {
          index1 = 0;
        }

        if (index2 == null)
        {
          index2 = 0;
        }

        if (IsExactMatch(polygon, index1, index2))
        {
          seg[0].Exact = true;
          seg[1].Exact = true;
        }
      }
    }

    /// <summary>
    /// Mark any points that are exact.
    /// </summary>
    /// <param name="polygon">This is the one that's checked against.</param>
    /// <param name="simple">This is iterated and elements are marked exact when matched to a point in polygon.</param>
    private static void MarkExact(INfp polygon, INfp simple, double curveTolerance)
    {
      for (int i = 0; i < simple.Length; i++)
      {
        var seg = new NoFitPolygon();
        seg.AddPoint(simple[i]);
        seg.AddPoint(simple[i + 1 == simple.Length ? 0 : i + 1]);

        var index1 = Find(seg[0], polygon, curveTolerance);
        var index2 = Find(seg[1], polygon, curveTolerance);

        if (IsExactMatch(polygon, index1, index2))
        {
          seg[0].Exact = true;
          seg[1].Exact = true;
        }
      }
    }

    private static bool IsExactMatch(INfp polygon, int? index1, int? index2)
    {
      return index1 + 1 == index2 || index2 + 1 == index1 || (index1 == 0 && index2 == polygon.Length - 1) || (index2 == 0 && index1 == polygon.Length - 1);
    }

    /// <summary>
    /// Tests <see cref="complex"/> to find if all of it's vertices fall inside <see cref="simple"/>.
    /// </summary>
    /// <param name="simple"></param>
    /// <param name="complex"></param>
    /// <returns>.t if any complex vertices fall outside the simple polygon.</returns>
    internal static bool Interior(INfp simple, INfp complex)
    {
      return TestBRelativeToA(simple, complex, p => p == Found.Inside || p == Found.OnPolygon);
    }

    /// <summary>
    /// Tests <see cref="complex"/> to find if any of it's vertices fall outside <see cref="simple"/>.
    /// </summary>
    /// <param name="simple"></param>
    /// <param name="complex"></param>
    /// <param name="inside">Bool that flips the logic to test Interior.</param>
    /// <returns>.t if any complex vertices fall outside the simple polygon.</returns>
    internal static bool Exterior(INfp simple, INfp complex)
    {
      return TestBRelativeToA(simple, complex, p => p == Found.Outside);
    }

    /// <summary>
    /// Tests points in B to find if any of it's vertices Test positive relative to A/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="test">Function to apply as test</param>
    /// <returns>.t if any complex vertices meet the test condition.</returns>
    private static bool TestBRelativeToA(INfp a, INfp b, Func<Found, bool> test)
    {
      for (var i = 0; i < b.Length; i++)
      {
        var vertex = b[i];
        var pointInPolygon = PointInPolygon(vertex, a);
        if (test(pointInPolygon))
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Search <see cref="p"/> for a matching point within curveTolerance/1000.
    /// </summary>
    /// <param name="vertex">Vertex to search for a match.</param>
    /// <param name="polygon">Polygon to search for a matching (within tolerance) vertex.</param>
    /// <param name="curveTolerance">Tolerance within distance to find a matching point (/1000).</param>
    /// <returns>Index of matching point if found, otherwise null.</returns>
    private static int? Find(SvgPoint vertex, INfp polygon, double curveTolerance)
    {
      for (var i = 0; i < polygon.Length; i++)
      {
        if (GeometryUtil.WithinDistance(vertex, polygon[i], curveTolerance / 1000))
        {
          return i;
        }
      }

      return null;
    }

    /// <summary>
    /// Tests to determine if Point falls within (or on) the Polygon.
    /// </summary>
    /// <param name="point">Point to test.</param>
    /// <param name="polygon">Polygon within which to determine the lie of the points.</param>
    /// <returns>Whether the point falls within, without or on the polygon.</returns>
    internal static Found PointInPolygon(SvgPoint point, INfp polygon)
    {
      // scaling is deliberately coarse to distinguish points that lie *on* the polygon
      var p = SvgToClipper2(polygon, 1000);
      var pt = new IntPoint(1000 * point.X, 1000 * point.Y);

      return (Found)Clipper.PointInPolygon(pt, p);
    }

    internal static bool IsInnerContainedByOuter(INfp inner, INfp outer)
    {
      // scaling is deliberately coarse to filter out points that lie *on* the polygon
      var innerClipper = ToOutPt(SvgToClipper2(inner));
      var outerClipper = ToOutPt(SvgToClipper2(outer));

      return Clipper.Poly2ContainsPoly1(innerClipper, outerClipper);
    }

    internal static OutPt ToOutPt(INfp nfp)
    {
      return ToOutPt(SvgToClipper2(nfp));
    }

    internal static OutPt ToOutPt(List<IntPoint> intPoints)
    {
      int idx = 0;
      IntPoint origin;
      OutPt result = null;
      OutPt current;
      OutPt prior = null;
      foreach (var point in intPoints)
      {
        if (idx == 0)
        {
          origin = point;
          result = new OutPt()
          {
            Idx = idx,
            Pt = point,
          };

          current = result;
        }
        else
        {
          current = new OutPt()
          {
            Idx = idx,
            Pt = point,
            Prev = prior,
          };

          prior.Next = current;
        }

        prior = current;
        idx++;
      }

      result.Prev = prior;
      prior.Next = result;

      return result;
    }

    // returns a less complex polygon that satisfies the curve tolerance
    private static INfp CleanPolygon(INfp polygon)
    {
      var p = SvgToClipper2(polygon);

      // remove self-intersections and find the biggest polygon that's left
      var simple = ClipperLib.Clipper.SimplifyPolygon(p, ClipperLib.PolyFillType.pftNonZero);

      if (simple == null || simple.Count == 0)
      {
        return null;
      }

      var biggest = simple[0];
      var biggestarea = Math.Abs(ClipperLib.Clipper.Area(biggest));
      for (var i = 1; i < simple.Count; i++)
      {
        var area = Math.Abs(ClipperLib.Clipper.Area(simple[i]));
        if (area > biggestarea)
        {
          biggest = simple[i];
          biggestarea = area;
        }
      }

      // clean up singularities, coincident points and edges
      var clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 * SvgNest.Config.CurveTolerance * SvgNest.Config.ClipperScale);

      if (clean == null || clean.Count == 0)
      {
        return null;
      }

      return SvgNest.ClipperToSvg(clean);
    }

    // converts a polygon from normal double coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    private static List<IntPoint> SvgToClipper2(INfp polygon, double? scale = null)
    {
      var d = DeepNestClipper.ScaleUpPath(polygon.Points, scale == null ? SvgNest.Config.ClipperScale : scale.Value);
      return d;
    }

    private static SvgPoint GetTarget(SvgPoint o, INfp simple, double tol)
    {
      List<InrangeItem> inrange = new List<InrangeItem>();

      // find closest points within 2 offset deltas
      for (var j = 0; j < simple.Length; j++)
      {
        var s = simple[j];
        var d2 = ((o.X - s.X) * (o.X - s.X)) + ((o.Y - s.Y) * (o.Y - s.Y));
        if (d2 < tol * tol)
        {
          inrange.Add(new InrangeItem() { point = s, distance = d2 });
        }
      }

      SvgPoint target = null;
      if (inrange.Count > 0)
      {
        var filtered = inrange.Where((p) =>
        {
          return p.point.Exact;
        }).ToList();

        // use exact points when available, normal points when not
        inrange = filtered.Count > 0 ? filtered : inrange;

        inrange = inrange.OrderBy((b) =>
        {
          return b.distance;
        }).ToList();

        target = inrange[0].point;
      }
      else
      {
        double? mind = null;
        for (int j = 0; j < simple.Length; j++)
        {
          var s = simple[j];
          var d2 = ((o.X - s.X) * (o.X - s.X)) + ((o.Y - s.Y) * (o.Y - s.Y));
          if (mind == null || d2 < mind)
          {
            target = s;
            mind = d2;
          }
        }
      }

      return target;
    }
  }
}