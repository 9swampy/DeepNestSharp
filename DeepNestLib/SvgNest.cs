namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using ClipperLib;

  public class SvgNest
  {
    private readonly IMessageService messageService;
    private readonly Action setIsErrored;

    public SvgNest(IMessageService messageService, Action setIsErrored)
    {
      this.messageService = messageService;
      this.setIsErrored = setIsErrored;
    }

    public class InrangeItem
    {
      public SvgPoint point;
      public double distance;
    }

    public static SvgPoint getTarget(SvgPoint o, NFP simple, double tol)
    {
      List<InrangeItem> inrange = new List<InrangeItem>();

      // find closest points within 2 offset deltas
      for (var j = 0; j < simple.Length; j++)
      {
        var s = simple[j];
        var d2 = ((o.x - s.x) * (o.x - s.x)) + ((o.y - s.y) * (o.y - s.y));
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
          return p.point.exact;
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
          var d2 = ((o.x - s.x) * (o.x - s.x)) + ((o.y - s.y) * (o.y - s.y));
          if (mind == null || d2 < mind)
          {
            target = s;
            mind = d2;
          }
        }
      }

      return target;
    }

    public static ISvgNestConfig Config = new SvgNestConfig();

    public static NFP clone(NFP p)
    {
      var newp = new NFP();
      for (var i = 0; i < p.Length; i++)
      {
        newp.AddPoint(new SvgPoint(

             p[i].x,
             p[i].y));
      }

      return newp;
    }

    public static bool pointInPolygon(SvgPoint point, NFP polygon)
    {
      // scaling is deliberately coarse to filter out points that lie *on* the polygon
      var p = svgToClipper2(polygon, 1000);
      var pt = new ClipperLib.IntPoint(1000 * point.x, 1000 * point.y);

      return ClipperLib.Clipper.PointInPolygon(pt, p.ToList()) > 0;
    }

    // returns true if any complex vertices fall outside the simple polygon
    private static bool exterior(NFP simple, NFP complex, bool inside, ISvgNestConfig config)
    {
      // find all protruding vertices
      for (var i = 0; i < complex.Length; i++)
      {
        var v = complex[i];
        if (!inside && !pointInPolygon(v, simple) && find(v, simple, config) == null)
        {
          return true;
        }

        if (inside && pointInPolygon(v, simple) && find(v, simple, config) != null)
        {
          return true;
        }
      }

      return false;
    }

    private static readonly PolygonSimplificationDictionary cache = new PolygonSimplificationDictionary();

    private static volatile object cacheSyncLock = new object();

    public static NFP simplifyFunction(NFP polygon, bool inside)
    {
      return simplifyFunction(polygon, inside, Config);
    }

    /// <summary>
    /// Override specifically added for Tests so the curveTolerance can be passed in.
    /// </summary>
    /// <param name="polygon"></param>
    /// <param name="inside"></param>
    /// <param name="curveTolerance"></param>
    /// <returns></returns>
    internal static NFP simplifyFunction(NFP polygon, bool inside, ISvgNestConfig config)
    {
      var markExact = true;
      var straighten = true;
      var doSimplify = true;
      var selectiveReversalOfOffset = true;

      var tolerance = 4 * config.CurveTolerance;

      // give special treatment to line segments above this length (squared)
      var fixedTolerance = 40 * config.CurveTolerance * 40 * config.CurveTolerance;
      int i, j, k;

      var hull = Background.GetHull(polygon.Points);
      if (Config.Simplify)
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

      var cleaned = cleanPolygon2(polygon);
      if (cleaned != null && cleaned.Length > 1)
      {
        polygon = cleaned;
      }
      else
      {
        return polygon;
      }

      SvgPoint[] resultSource;
      bool doSimplifyRadialDist = false;
      bool doSimplifyDouglasPeucker = true;
      if (cache.TryGetValue(new System.Tuple<SvgPoint[], double?, bool, bool>(polygon.Points, config.CurveTolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker), out resultSource))
      {
        return new NFP(resultSource);
      }

      NFP simple = null;
      if (doSimplify)
      {
        // polygon to polyline
        var copy = polygon.slice(0);
        copy.Push(copy[0]);

        // mark all segments greater than ~0.25 in to be kept
        // the PD simplification algo doesn't care about the accuracy of long lines, only the absolute distance of each point
        // we care a great deal
        for (i = 0; i < copy.Length - 1; i++)
        {
          var p1 = copy[i];
          var p2 = copy[i + 1];
          var sqd = ((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y));
          if (sqd > fixedTolerance)
          {
            p1.marked = true;
            p2.marked = true;
          }
        }

        simple = Simplify.simplify(copy.Points, tolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker);

        // now a polygon again
        // simple.pop();
        simple.ReplacePoints(simple.Points.Take(simple.Points.Count() - 1));

        // could be dirty again (self intersections and/or coincident points)
        simple = cleanPolygon2(simple);
      }

      // simplification process reduced poly to a line or point; or it came back just as complex as the original
      if (simple == null || simple.Points.Count() > polygon.Points.Count() * 0.9)
      {
        simple = polygon;
      }

      var offsets = polygonOffsetDeepNest(simple, inside ? -tolerance : tolerance);

      NFP offset = null;
      double offsetArea = 0;
      List<NFP> holes = new List<NFP>();
      for (i = 0; i < offsets.Length; i++)
      {
        var area = GeometryUtil.polygonArea(offsets[i]);
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
        MarkExact(polygon, simple, config);
      }

      var numshells = 4;
      NFP[] shells = new NFP[numshells];

      for (j = 1; j < numshells; j++)
      {
        var delta = j * (tolerance / numshells);
        delta = inside ? -delta : delta;
        var shell = polygonOffsetDeepNest(simple, delta);
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
        for (i = 0; i < offset.Length; i++)
        {
          if (i % 10 == 0)
          {
            System.Diagnostics.Debug.Print($"{polygon.Name} selectiveReversalOfOffset {i} of {offset.Length}");
          }

          var o = offset[i];
          var target = getTarget(o, simple, 2 * tolerance);

          // reverse point offset and try to find exterior points
          var test = clone(offset);
          test.Points[i] = new SvgPoint(target.x, target.y);

          if (!exterior(test, polygon, inside, config))
          {
            o.x = target.x;
            o.y = target.y;
          }
          else
          {
            // a shell is an intermediate offset between simple and offset
            for (j = 1; j < numshells; j++)
            {
              if (shells[j] != null)
              {
                var shell = shells[j];
                var delta = j * (tolerance / numshells);
                target = getTarget(o, shell, 2 * delta);
                test = clone(offset);
                test.Points[i] = new SvgPoint(target.x, target.y);
                if (!exterior(test, polygon, inside, config))
                {
                  o.x = target.x;
                  o.y = target.y;
                  break;
                }
              }
            }
          }
        }
      }

      if (straighten)
      {
        // straighten long lines
        // a rounded rectangle would still have issues at this point, as the long sides won't line up straight
        var straightened = false;

        for (i = 0; i < offset.Length; i++)
        {
          var p1 = offset[i];
          var p2 = offset[i + 1 == offset.Length ? 0 : i + 1];

          var sqd = ((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y));

          if (sqd < fixedTolerance)
          {
            continue;
          }

          for (j = 0; j < simple.Length; j++)
          {
            var s1 = simple[j];
            var s2 = simple[j + 1 == simple.Length ? 0 : j + 1];

            var sqds = ((p2.x - p1.x) * (p2.x - p1.x)) + ((p2.y - p1.y) * (p2.y - p1.y));

            if (sqds < fixedTolerance)
            {
              continue;
            }

            if ((GeometryUtil._almostEqual(s1.x, s2.x) || GeometryUtil._almostEqual(s1.y, s2.y)) && // we only really care about vertical and horizontal lines
            GeometryUtil._withinDistance(p1, s1, 2 * tolerance) &&
            GeometryUtil._withinDistance(p2, s2, 2 * tolerance) &&
            (!GeometryUtil._withinDistance(p1, s1, config.CurveTolerance / 1000) ||
            !GeometryUtil._withinDistance(p2, s2, config.CurveTolerance / 1000)))
            {
              p1.x = s1.x;
              p1.y = s1.y;
              p2.x = s2.x;
              p2.y = s2.y;
              straightened = true;
            }
          }
        }

        if (straightened)
        {
          var Ac = _Clipper.ScaleUpPaths(offset.Points, 10000000);
          var Bc = _Clipper.ScaleUpPaths(polygon.Points, 10000000);

          var combined = new List<List<IntPoint>>();
          var clipper = new ClipperLib.Clipper();

          clipper.AddPath(Ac.ToList(), ClipperLib.PolyType.ptSubject, true);
          clipper.AddPath(Bc.ToList(), ClipperLib.PolyType.ptSubject, true);

          // the line straightening may have made the offset smaller than the simplified
          if (clipper.Execute(ClipperLib.ClipType.ctUnion, combined, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
          {
            double? largestArea = null;
            for (i = 0; i < combined.Count; i++)
            {
              var n = Background.ToNestCoordinates(combined[i].ToArray(), 10000000);
              var sarea = -GeometryUtil.polygonArea(n);
              if (largestArea == null || largestArea < sarea)
              {
                offset = n;
                largestArea = sarea;
              }
            }
          }
        }
      }

      cleaned = cleanPolygon2(offset);
      if (cleaned != null && cleaned.Length > 1)
      {
        offset = cleaned;
      }

      if (Config.ClipByHull)
      {
        offset = ClipSubject(offset, hull, Config.ClipperScale);
      }

      if (markExact)
      {
        MarkExactSvg(polygon, offset, config);
      }

      if (!inside && holes != null && holes.Count > 0)
      {
        offset.Children = holes;
      }

      lock (cacheSyncLock)
      {
        if (!cache.ContainsKey(new System.Tuple<SvgPoint[], double?, bool, bool>(polygon.Points, config.CurveTolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker)))
        {
          cache.Add(new System.Tuple<SvgPoint[], double?, bool, bool>(polygon.Points, config.CurveTolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker), offset.Points);
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
    internal static NFP ClipSubject(NFP subject, NFP clipBounds, double clipperScale)
    {
      var clipperSubject = Background.InnerNfpToClipperCoordinates(new NFP[] { subject }, clipperScale);
      var clipperClip = Background.InnerNfpToClipperCoordinates(new NFP[] { clipBounds }, clipperScale);

      var clipper = new Clipper();
      clipper.AddPaths(clipperClip.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
      clipper.AddPaths(clipperSubject.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

      List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
      if (clipper.Execute(ClipType.ctIntersection, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero) && finalNfp != null && finalNfp.Count > 0)
      {
        return Background.ToNestCoordinates(finalNfp[0].ToArray(), clipperScale);
      }

      return subject;
    }

    /// <summary>
    /// Mark any points that are exact (for line merge detection).
    /// </summary>
    /// <param name="polygon">This is the one that's checked against.</param>
    /// <param name="offset">This is iterated and elements are marked exact when matched to a point in polygon.</param>
    private static void MarkExactSvg(NFP polygon, NFP offset, ISvgNestConfig config)
    {
      int i;
      for (i = 0; i < offset.Length; i++)
      {
        var seg = new SvgPoint[] { offset[i], offset[i + 1 == offset.Length ? 0 : i + 1] };
        var index1 = find(seg[0], polygon, config);
        var index2 = find(seg[1], polygon, config);
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
          seg[0].exact = true;
          seg[1].exact = true;
        }
      }
    }

    /// <summary>
    /// Mark any points that are exact.
    /// </summary>
    /// <param name="polygon">This is the one that's checked against.</param>
    /// <param name="simple">This is iterated and elements are marked exact when matched to a point in polygon.</param>
    private static void MarkExact(NFP polygon, NFP simple, ISvgNestConfig config)
    {
      for (int i = 0; i < simple.Length; i++)
      {
        var seg = new NFP();
        seg.AddPoint(simple[i]);
        seg.AddPoint(simple[i + 1 == simple.Length ? 0 : i + 1]);

        var index1 = find(seg[0], polygon, config);
        var index2 = find(seg[1], polygon, config);

        if (IsExactMatch(polygon, index1, index2))
        {
          seg[0].exact = true;
          seg[1].exact = true;
        }
      }
    }

    private static bool IsExactMatch(NFP polygon, int? index1, int? index2)
    {
      return index1 + 1 == index2 || index2 + 1 == index1 || (index1 == 0 && index2 == polygon.Length - 1) || (index2 == 0 && index1 == polygon.Length - 1);
    }

    private static int? find(SvgPoint v, NFP p, ISvgNestConfig config)
    {
      for (var i = 0; i < p.Length; i++)
      {
        if (GeometryUtil._withinDistance(v, p[i], config.CurveTolerance / 1000))
        {
          return i;
        }
      }

      return null;
    }

    // offset tree recursively
    public static void OffsetTree(NFP t, double offset, ISvgNestConfig config, bool? inside = null)
    {
      var simple = simplifyFunction(t, (inside == null) ? false : inside.Value);
      var offsetpaths = new NFP[] { simple };
      if (Math.Abs(offset) > 0)
      {
        offsetpaths = polygonOffsetDeepNest(simple, offset);
      }

      if (offsetpaths.Count() > 0)
      {
        List<SvgPoint> rett = new List<SvgPoint>();
        rett.AddRange(offsetpaths[0].Points);
        rett.AddRange(t.Points.Skip(t.Length));
        t.ReplacePoints(rett);

        // replace array items in place

        // Array.prototype.splice.apply(t, [0, t.length].concat(offsetpaths[0]));
      }

      if (simple.Children != null && simple.Children.Count > 0)
      {
        if (t.Children == null)
        {
          t.Children = new List<NFP>();
        }

        for (var i = 0; i < simple.Children.Count; i++)
        {
          t.Children.Add(simple.Children[i]);
        }
      }

      if (t.Children != null && t.Children.Count > 0)
      {
        for (var i = 0; i < t.Children.Count; i++)
        {
          OffsetTree(t.Children[i], -offset, config, (inside == null) ? true : (!inside));
        }
      }
    }

    // use the clipper library to return an offset to the given polygon. Positive offset expands the polygon, negative contracts
    // note that this returns an array of polygons
    public static NFP[] polygonOffsetDeepNest(NFP polygon, double offset)
    {
      if (offset == 0 || GeometryUtil._almostEqual(offset, 0))
      {
        return new[] { polygon };
      }

      var p = svgToClipper(polygon).ToList();

      var miterLimit = 4;
      var co = new ClipperLib.ClipperOffset(miterLimit, Config.CurveTolerance * Config.ClipperScale);
      co.AddPath(p.ToList(), ClipperLib.JoinType.jtMiter, ClipperLib.EndType.etClosedPolygon);

      var newpaths = new List<List<ClipperLib.IntPoint>>();
      co.Execute(ref newpaths, offset * Config.ClipperScale);

      var result = new List<NFP>();
      for (var i = 0; i < newpaths.Count; i++)
      {
        result.Add(clipperToSvg(newpaths[i]));
      }

      return result.ToArray();
    }

    // converts a polygon from normal float coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    public static IntPoint[] svgToClipper2(NFP polygon, double? scale = null)
    {
      var d = _Clipper.ScaleUpPaths(polygon.Points, scale == null ? Config.ClipperScale : scale.Value);
      return d.ToArray();
    }

    // converts a polygon from normal float coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    public static ClipperLib.IntPoint[] svgToClipper(NFP polygon)
    {
      var d = _Clipper.ScaleUpPaths(polygon.Points, Config.ClipperScale);
      return d.ToArray();

      return polygon.Points.Select(z => new IntPoint((long)z.x, (long)z.y)).ToArray();
    }

    // returns a less complex polygon that satisfies the curve tolerance
    public static NFP cleanPolygon(NFP polygon)
    {
      var p = svgToClipper2(polygon);

      // remove self-intersections and find the biggest polygon that's left
      var simple = ClipperLib.Clipper.SimplifyPolygon(p.ToList(), ClipperLib.PolyFillType.pftNonZero);

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
      var clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 * Config.CurveTolerance * Config.ClipperScale);

      if (clean == null || clean.Count == 0)
      {
        return null;
      }

      return clipperToSvg(clean);
    }

    public static NFP cleanPolygon2(NFP polygon)
    {
      var p = svgToClipper(polygon);

      // remove self-intersections and find the biggest polygon that's left
      var simple = ClipperLib.Clipper.SimplifyPolygon(p.ToList(), ClipperLib.PolyFillType.pftNonZero);

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
      var clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 * Config.CurveTolerance * Config.ClipperScale);

      if (clean == null || clean.Count == 0)
      {
        return null;
      }

      var cleaned = clipperToSvg(clean);

      // remove duplicate endpoints
      var start = cleaned[0];
      var end = cleaned[cleaned.Length - 1];
      if (start == end || (GeometryUtil._almostEqual(start.x, end.x)
          && GeometryUtil._almostEqual(start.y, end.y)))
      {
        cleaned.ReplacePoints(cleaned.Points.Take(cleaned.Points.Count() - 1));
      }

      return cleaned;
    }

    private static NFP clipperToSvg(IList<IntPoint> polygon)
    {
      List<SvgPoint> ret = new List<SvgPoint>();

      for (var i = 0; i < polygon.Count; i++)
      {
        ret.Add(new SvgPoint(polygon[i].X / Config.ClipperScale, polygon[i].Y / Config.ClipperScale));
      }

      return new NFP(ret);
    }

    private int toTree(PolygonTreeItem[] list, int idstart = 0)
    {
      List<PolygonTreeItem> parents = new List<PolygonTreeItem>();
      int i, j;

      // assign a unique id to each leaf
      // var id = idstart || 0;
      var id = idstart;

      for (i = 0; i < list.Length; i++)
      {
        var p = list[i];

        var ischild = false;
        for (j = 0; j < list.Length; j++)
        {
          if (j == i)
          {
            continue;
          }

          if (GeometryUtil.pointInPolygon(p.Polygon.Points[0], list[j].Polygon).Value)
          {
            if (list[j].Childs == null)
            {
              list[j].Childs = new List<PolygonTreeItem>();
            }

            list[j].Childs.Add(p);
            p.Parent = list[j];
            ischild = true;
            break;
          }
        }

        if (!ischild)
        {
          parents.Add(p);
        }
      }

      for (i = 0; i < list.Length; i++)
      {
        if (parents.IndexOf(list[i]) < 0)
        {
          list = list.Skip(i).Take(1).ToArray();
          i--;
        }
      }

      for (i = 0; i < parents.Count; i++)
      {
        parents[i].Polygon.Id = id;
        id++;
      }

      for (i = 0; i < parents.Count; i++)
      {
        if (parents[i].Childs != null)
        {
          id = this.toTree(parents[i].Childs.ToArray(), id);
        }
      }

      return id;
    }

    private static NFP cloneTree(NFP tree)
    {
      NFP newtree = new NFP();
      foreach (var t in tree.Points)
      {
        newtree.AddPoint(new SvgPoint(t.x, t.y) { exact = t.exact });
      }

      if (tree.Children != null && tree.Children.Count > 0)
      {
        foreach (var c in tree.Children)
        {
          newtree.Children.Add(cloneTree(c));
        }
      }

      return newtree;
    }

    public Background background = new Background();

    private PopulationItem individual = null;
    private NFP[] placelist;
    private GeneticAlgorithm ga;

    public List<SheetPlacement> nests = new List<SheetPlacement>();

    public void ResponseProcessor(SheetPlacement payload)
    {
      // console.log('ipc response', payload);
      if (this.ga == null)
      {
        // user might have quit while we're away
        return;
      }

      this.ga.Population[payload.index].processing = null;
      this.ga.Population[payload.index].fitness = payload.fitness;

      // render placement
      if (this.nests.Count == 0 || this.nests[0].fitness > payload.fitness)
      {
        this.nests.Insert(0, payload);

        if (this.nests.Count > Config.PopulationSize)
        {
          this.nests.RemoveAt(this.nests.Count - 1);
        }

        // if (displayCallback)
        {
          // displayCallback();
        }
      }
    }

    public void launchWorkers(NestItem[] parts)
    {
      try
      {
        this.background.ResponseAction = this.ResponseProcessor;

        if (this.ga == null)
        {
          List<NFP> adam = new List<NFP>();
          var id = 0;
          for (int i = 0; i < parts.Count(); i++)
          {
            if (!parts[i].IsSheet)
            {
              for (int j = 0; j < parts[i].Quantity; j++)
              {
                var poly = cloneTree(parts[i].Polygon); // deep copy
                poly.Id = id; // id is the unique id of all parts that will be nested, including cloned duplicates
                poly.Source = i; // source is the id of each unique part from the main part list

                adam.Add(poly);
                id++;
              }
            }
          }

          adam = adam.OrderByDescending(z => Math.Abs(GeometryUtil.polygonArea(z))).ToList();
          /*List<NFP> shuffle = new List<NFP>();
          Random r = new Random(DateTime.Now.Millisecond);
          while (adam.Any())
          {
              var rr = r.Next(adam.Count);
              shuffle.Add(adam[rr]);
              adam.RemoveAt(rr);
          }
          adam = shuffle;*/

          /*#region special case
          var temp = adam[1];
          adam.RemoveAt(1);
          adam.Insert(9, temp);

          #endregion*/
          this.ga = new GeneticAlgorithm(adam.ToArray(), Config);
        }

        this.individual = null;

        // check if current generation is finished
        var finished = true;
        for (int i = 0; i < this.ga.Population.Count; i++)
        {
          if (this.ga.Population[i].fitness == null)
          {
            finished = false;
            break;
          }
        }

        if (finished)
        {
          // console.log('new generation!');
          // all individuals have been evaluated, start next generation
          this.ga.Generation();
        }

        var running = this.ga.Population.Where((p) =>
        {
          return p.processing != null;
        }).Count();

        List<NFP> sheets = new List<NFP>();
        List<int> sheetids = new List<int>();
        List<int> sheetsources = new List<int>();
        List<List<NFP>> sheetchildren = new List<List<NFP>>();
        var sid = 0;
        for (int i = 0; i < parts.Count(); i++)
        {
          if (parts[i].IsSheet)
          {
            var poly = parts[i].Polygon;
            for (int j = 0; j < parts[i].Quantity; j++)
            {
              var cln = cloneTree(poly);
              cln.Id = sid; // id is the unique id of all parts that will be nested, including cloned duplicates
              cln.Source = poly.Source; // source is the id of each unique part from the main part list

              sheets.Add(cln);
              sheetids.Add(sid);
              sheetsources.Add(i);
              sheetchildren.Add(poly.Children.ToList());
              sid++;
            }
          }
        }

        for (int i = 0; i < this.ga.Population.Count; i++)
        {
          // if(running < config.threads && !GA.population[i].processing && !GA.population[i].fitness){
          // only one background window now...
          if (running < 1 && this.ga.Population[i].processing == null && this.ga.Population[i].fitness == null)
          {
            this.ga.Population[i].processing = true;

            // hash values on arrays don't make it across ipc, store them in an array and reassemble on the other side....
            List<int> ids = new List<int>();
            List<int> sources = new List<int>();
            List<List<NFP>> children = new List<List<NFP>>();

            for (int j = 0; j < this.ga.Population[i].placements.Count; j++)
            {
              var id = this.ga.Population[i].placements[j].Id;
              var source = this.ga.Population[i].placements[j].Source;
              var child = this.ga.Population[i].placements[j].Children;

              // ids[j] = id;
              ids.Add(id);

              // sources[j] = source;
              sources.Add(source);

              // children[j] = child;
              children.Add(child.ToList());
            }

            DataInfo data = new DataInfo()
            {
              index = i,
              sheets = sheets,
              sheetids = sheetids.ToArray(),
              sheetsources = sheetsources.ToArray(),
              sheetchildren = sheetchildren,
              individual = this.ga.Population[i],
              config = Config,
              ids = ids.ToArray(),
              sources = sources.ToArray(),
              children = children,
            };

            this.background.BackgroundStart(data);

            // ipcRenderer.send('background-start', { index: i, sheets: sheets, sheetids: sheetids, sheetsources: sheetsources, sheetchildren: sheetchildren, individual: GA.population[i], config: config, ids: ids, sources: sources, children: children});
            running++;
          }
        }
      }
      catch (Exception ex)
      {
        this.messageService.DisplayMessage(ex);
        this.setIsErrored();
      }
    }

    public PolygonTreeItem[] tree;

    public static IntPoint[] toClipperCoordinates(NFP polygon)
    {
      var clone = new List<IntPoint>();
      for (var i = 0; i < polygon.Length; i++)
      {
        clone.Add(
            new IntPoint(
             polygon[i].x,
             polygon[i].y));
      }

      return clone.ToArray();
    }

    public bool useHoles;
    public bool searchEdges;
  }

  public interface IDeprecatedClipper
  {
    ClipperLib.IntPoint[] ScaleUpPathsOriginal(NFP p, double scale);

    ClipperLib.IntPoint[] ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale = 1);
  }

  public class _Clipper : IDeprecatedClipper
  {
    ClipperLib.IntPoint[] IDeprecatedClipper.ScaleUpPathsOriginal(NFP p, double scale = 1)
    {
      List<ClipperLib.IntPoint> ret = new List<ClipperLib.IntPoint>();

      for (int i = 0; i < p.Points.Length; i++)
      {
        // p.Points[i] = new SvgNestPort.SvgPoint((float)Math.Round(p.Points[i].x * scale), (float)Math.Round(p.Points[i].y * scale));
        ret.Add(new ClipperLib.IntPoint(
            (long)Math.Round((decimal)p.Points[i].x * (decimal)scale),
            (long)Math.Round((decimal)p.Points[i].y * (decimal)scale)));
      }

      return ret.ToArray();
    } // 5 secs

    ClipperLib.IntPoint[] IDeprecatedClipper.ScaleUpPathsSlowerParallel(SvgPoint[] points, double scale = 1)
    {
      var result = from point in points.AsParallel().AsSequential()
                   select new ClipperLib.IntPoint((long)Math.Round((decimal)point.x * (decimal)scale), (long)Math.Round((decimal)point.y * (decimal)scale));

      return result.ToArray();
    } // 2 secs

    public static ClipperLib.IntPoint[] ScaleUpPaths(SvgPoint[] points, double scale = 1)
    {
      var result = new ClipperLib.IntPoint[points.Length];

      Parallel.For(0, points.Length, i => result[i] = new ClipperLib.IntPoint((long)Math.Round((decimal)points[i].x * (decimal)scale), (long)Math.Round((decimal)points[i].y * (decimal)scale)));

      return result.ToArray();
    } // 2 secs

    /*public static IntPoint[] ScaleUpPath(IntPoint[] p, double scale = 1)
    {
        for (int i = 0; i < p.Length; i++)
        {

            //p[i] = new IntPoint(p[i].X * scale, p[i].Y * scale);
            p[i] = new IntPoint(
                (long)Math.Round((decimal)p[i].X * (decimal)scale),
                (long)Math.Round((decimal)p[i].Y * (decimal)scale));
        }
        return p.ToArray();
    }
    public static void ScaleUpPaths(List<List<IntPoint>> p, double scale = 1)
    {
        for (int i = 0; i < p.Count; i++)
        {
            for (int j = 0; j < p[i].Count; j++)
            {
                p[i][j] = new IntPoint(p[i][j].X * scale, p[i][j].Y * scale);

            }
        }


    }*/
  }

  public class DataInfo
  {
    public int index;
    public List<NFP> sheets;
    public int[] sheetids;
    public int[] sheetsources;
    public List<List<NFP>> sheetchildren;
    public PopulationItem individual;
    public ISvgNestConfig config;
    public int[] ids;
    public int[] sources;
    public List<List<NFP>> children;
    //ipcRenderer.send('background-start', { index: i, sheets: sheets, sheetids: sheetids, sheetsources: sheetsources, sheetchildren: sheetchildren, 
    //individual: GA.population[i], config: config, ids: ids, sources: sources, children: children});
  }

  public class PolygonTreeItem
  {
    public NFP Polygon;
    public PolygonTreeItem Parent;
    public List<PolygonTreeItem> Childs = new List<PolygonTreeItem>();
  }

  public enum PlacementTypeEnum
  {
    BoundingBox,
    Gravity,
    Squeeze
  }

  public class DbCacheKey
  {
    public DbCacheKey(int? a, int? b, float aRotation, float bRotation, IEnumerable<NFP> nfps)
    {
      A = a;
      B = b;
      ARotation = aRotation;
      BRotation = bRotation;
      nfp = nfps.ToArray();
    }

    public DbCacheKey(int? a, int? b, float aRotation, float bRotation)
    // : this(a, b, aRotation, bRotation, null)
    {
      A = a;
      B = b;
      ARotation = aRotation;
      BRotation = bRotation;
    }

    public int? A { get; }

    public int? B { get; }

    public float ARotation { get; }

    public float BRotation { get; }

    public NFP[] nfp { get; }

    public int Type { get; }

    public string Key
    {
      get
      {
        var key = "A" + this.A + "B" + this.B + "Arot" + (int)Math.Round(this.ARotation * 10000) + "Brot" + (int)Math.Round(this.BRotation * 10000) + ";" + this.Type;
        return key;
      }
    }
  }

  public class NfpPair
  {
    public NFP A;
    public NFP B;
    public NfpKey Key;
    public NFP nfp;

    public float ARotation;
    public float BRotation;

    public int Asource { get; internal set; }
    public int Bsource { get; internal set; }
  }

  public class NonameReturn
  {
    public NfpKey key;
    public NFP[] nfp;
    public NFP[] value
    {
      get
      {
        return nfp;
      }
    }

    public NonameReturn(NfpKey key, NFP[] nfp)
    {
      this.key = key;
      this.nfp = nfp;
    }
  }

  public interface IStringify
  {
    string stringify();
  }
  public class NfpKey : IStringify
  {

    public NFP A;
    public NFP B;
    public float ARotation { get; set; }
    public float BRotation { get; set; }
    public bool Inside { get; set; }

    public int AIndex { get; set; }
    public int BIndex { get; set; }
    public object Asource;
    public object Bsource;


    public string stringify()
    {
      return $"A:{AIndex} B:{BIndex} inside:{Inside} Arotation:{ARotation} Brotation:{BRotation}";
    }
  }

  public class PopulationItem
  {
    public object processing = null;

    public double? fitness;

    public float[] Rotation;
    public List<NFP> placements;

    public NFP[] paths;
    public double area;
  }

  public class SheetPlacementItem
  {
    public int sheetId;
    public int sheetSource;

    public List<PlacementItem> sheetplacements = new List<PlacementItem>();
    public List<PlacementItem> placements = new List<PlacementItem>();
  }

  public class PlacementItem
  {
    public double? mergedLength;
    public object mergedSegments;
    public List<List<ClipperLib.IntPoint>> nfp;
    public int id;
    public NFP hull;
    public NFP hullsheet;

    public float rotation;
    public double x;
    public double y;
    public int source;
  }

  public class SheetPlacement
  {
    public double? fitness;

    public float[] Rotation;
    public List<SheetPlacementItem>[] placements;

    public NFP[] paths;
    public double area;
    public double mergedLength;
    internal int index;
  }

  public class Sheet : NFP
  {
    public double Width;
    public double Height;
  }

  public class RectangleSheet : Sheet
  {
    public void Rebuild()
    {
      this.ReplacePoints(new SvgPoint[4]
      {
        new SvgPoint(x, y),
        new SvgPoint(x + Width, y),
        new SvgPoint(x + Width, y + Height),
        new SvgPoint(x, y + Height),
      });
    }
  }

  public class NestItem
  {
    public NFP Polygon;
    public int Quantity;
    public bool IsSheet;
  }
}
