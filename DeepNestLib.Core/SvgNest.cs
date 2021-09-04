namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
#if NCRUNCH
  using System.Diagnostics;
#endif
  using System.Linq;
  using System.Threading.Tasks;
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;

  public partial class SvgNest
  {
    private readonly IMessageService messageService;
    private readonly IMinkowskiSumService minkowskiSumService;
    private Procreant ga;

    private IProgressDisplayer progressDisplayer;

    private static readonly PolygonSimplificationDictionary cache = new PolygonSimplificationDictionary();

    private static volatile object cacheSyncLock = new object();
    private volatile bool isStopped;

    public SvgNest(
      IMessageService messageService,
      IProgressDisplayer progressDisplayer,
      IMinkowskiSumService minkowskiSumService,
      INestStateSvgNest nestState)
    {
      this.State = nestState;
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
      this.minkowskiSumService = minkowskiSumService;
    }

    public static ISvgNestConfig Config { get; }
#if NCRUNCH
    = new TestSvgNestConfig();
#else
    = new SvgNestConfig();
#endif


    public bool IsStopped { get => isStopped; private set => isStopped = value; }

    private INestStateSvgNest State { get; }

    public static bool PointInPolygon(SvgPoint point, INfp polygon)
    {
      // scaling is deliberately coarse to filter out points that lie *on* the polygon
      var p = SvgToClipper2(polygon, 1000);
      var pt = new IntPoint(1000 * point.X, 1000 * point.Y);

      return Clipper.PointInPolygon(pt, p.ToList()) > 0;
    }

    // returns true if any complex vertices fall outside the simple polygon
    private static bool Exterior(INfp simple, INfp complex, bool inside, double curveTolerance)
    {
      // find all protruding vertices
      for (var i = 0; i < complex.Length; i++)
      {
        var v = complex[i];
        if (!inside && !PointInPolygon(v, simple) && Find(v, simple, curveTolerance) == null)
        {
          return true;
        }

        if (inside && PointInPolygon(v, simple) && Find(v, simple, curveTolerance) != null)
        {
          return true;
        }
      }

      return false;
    }

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
      int i, j;

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

      var cleaned = CleanPolygon2(polygon);
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
      if (cache.TryGetValue(new PolygonSimplificationKey(polygon.Points, curveTolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker), out resultSource))
      {
        return new NFP(resultSource);
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
        for (i = 0; i < copy.Length - 1; i++)
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

        simple = Simplify.SimplifyPolygon(copy.Points, tolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker);

        // now a polygon again
        // simple.pop();
        simple.ReplacePoints(simple.Points.Take(simple.Points.Count() - 1));

        // could be dirty again (self intersections and/or coincident points)
        simple = CleanPolygon2(simple);
      }

      // simplification process reduced poly to a line or point; or it came back just as complex as the original
      if (simple == null || simple.Points.Count() > polygon.Points.Count() * 0.9)
      {
        simple = polygon;
      }

      var offsets = PolygonOffsetDeepNest(simple, inside ? -tolerance : tolerance);

      INfp offset = null;
      double offsetArea = 0;
      List<INfp> holes = new List<INfp>();
      for (i = 0; i < offsets.Length; i++)
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
        var shell = PolygonOffsetDeepNest(simple, delta);
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
          var target = GetTarget(o, simple, 2 * tolerance);

          // reverse point offset and try to find exterior points
          var test = offset.CloneTop();
          test.Points[i] = new SvgPoint(target.X, target.Y);

          if (!Exterior(test, polygon, inside, curveTolerance))
          {
            o.X = target.X;
            o.Y = target.Y;
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
                target = GetTarget(o, shell, 2 * delta);
                test = offset.CloneTop();
                test.Points[i] = new SvgPoint(target.X, target.Y);
                if (!Exterior(test, polygon, inside, curveTolerance))
                {
                  o.X = target.X;
                  o.Y = target.Y;
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
          var Ac = DeepNestClipper.ScaleUpPaths(offset.Points, 10000000);
          var Bc = DeepNestClipper.ScaleUpPaths(polygon.Points, 10000000);

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

      cleaned = CleanPolygon2(offset);
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
        MarkExactSvg(polygon, offset, curveTolerance);
      }

      if (!inside && holes != null && holes.Count > 0)
      {
        offset.Children = holes;
      }

      lock (cacheSyncLock)
      {
        if (!cache.ContainsKey(new PolygonSimplificationKey(polygon.Points, curveTolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker)))
        {
          cache.Add(new PolygonSimplificationKey(polygon.Points, curveTolerance, doSimplifyRadialDist, doSimplifyDouglasPeucker), offset.Points);
        }
      }

      return offset;
    }

    internal void Stop()
    {
      this.IsStopped = true;
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
      var clipperSubject = NfpHelper.InnerNfpToClipperCoordinates(new INfp[] { subject }, clipperScale);
      var clipperClip = NfpHelper.InnerNfpToClipperCoordinates(new INfp[] { clipBounds }, clipperScale);

      var clipper = new Clipper();
      clipper.AddPaths(clipperClip.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
      clipper.AddPaths(clipperSubject.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

      List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
      if (clipper.Execute(ClipType.ctIntersection, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero) && finalNfp != null && finalNfp.Count > 0)
      {
        return finalNfp[0].ToArray().ToNestCoordinates(clipperScale);
      }

      return subject;
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
        var seg = new NFP();
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

    private static int? Find(SvgPoint v, INfp p, double curveTolerance)
    {
      for (var i = 0; i < p.Length; i++)
      {
        if (GeometryUtil.WithinDistance(v, p[i], curveTolerance / 1000))
        {
          return i;
        }
      }

      return null;
    }

    // offset tree recursively
    public static void OffsetTree(ref INfp t, double offset, ISvgNestConfig config, bool? inside = null)
    {
      var simple = SimplifyFunction(t, (inside == null) ? false : inside.Value, config);
      var offsetpaths = new INfp[] { simple };
      if (Math.Abs(offset) > 0)
      {
        offsetpaths = PolygonOffsetDeepNest(simple, offset);
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
          t.Children = new List<INfp>();
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
          var child = t.Children[i];
          OffsetTree(ref child, -offset, config, (inside == null) ? true : (!inside));
        }
      }
    }

    // use the clipper library to return an offset to the given polygon. Positive offset expands the polygon, negative contracts
    // note that this returns an array of polygons
    public static INfp[] PolygonOffsetDeepNest(INfp polygon, double offset)
    {
      if (offset == 0 || GeometryUtil.AlmostEqual(offset, 0))
      {
        return new[] { polygon };
      }

      var p = SvgToClipper(polygon).ToList();

      var miterLimit = 4;
      var co = new ClipperLib.ClipperOffset(miterLimit, Config.CurveTolerance * Config.ClipperScale);
      co.AddPath(p.ToList(), ClipperLib.JoinType.jtMiter, ClipperLib.EndType.etClosedPolygon);

      var newpaths = new List<List<ClipperLib.IntPoint>>();
      co.Execute(ref newpaths, offset * Config.ClipperScale);

      var result = new List<NFP>();
      for (var i = 0; i < newpaths.Count; i++)
      {
        result.Add(ClipperToSvg(newpaths[i]));
      }

      return result.ToArray();
    }

    // converts a polygon from normal double coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    public static IntPoint[] SvgToClipper2(INfp polygon, double? scale = null)
    {
      var d = DeepNestClipper.ScaleUpPaths(polygon.Points, scale == null ? Config.ClipperScale : scale.Value);
      return d.ToArray();
    }

    // converts a polygon from normal double coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    public static ClipperLib.IntPoint[] SvgToClipper(INfp polygon)
    {
      var d = DeepNestClipper.ScaleUpPaths(polygon.Points, Config.ClipperScale);
      return d.ToArray();

      return polygon.Points.Select(z => new IntPoint((long)z.X, (long)z.Y)).ToArray();
    }

    // returns a less complex polygon that satisfies the curve tolerance
    public static INfp CleanPolygon(INfp polygon)
    {
      var p = SvgToClipper2(polygon);

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

      return ClipperToSvg(clean);
    }

    public static INfp CleanPolygon2(INfp polygon)
    {
      var p = SvgToClipper(polygon);

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

      var cleaned = ClipperToSvg(clean);

      // remove duplicate endpoints
      var start = cleaned[0];
      var end = cleaned[cleaned.Length - 1];
      if (start == end || (GeometryUtil.AlmostEqual(start.X, end.X)
          && GeometryUtil.AlmostEqual(start.Y, end.Y)))
      {
        cleaned.ReplacePoints(cleaned.Points.Take(cleaned.Points.Count() - 1));
      }

      if (polygon.IsClosed)
      {
        cleaned.EnsureIsClosed();
      }

      return cleaned;
    }

    private static NFP ClipperToSvg(IList<IntPoint> polygon)
    {
      List<SvgPoint> ret = new List<SvgPoint>();

      for (var i = 0; i < polygon.Count; i++)
      {
        ret.Add(new SvgPoint(polygon[i].X / Config.ClipperScale, polygon[i].Y / Config.ClipperScale));
      }

      return new NFP(ret);
    }

    private int ToTree(PolygonTreeItem[] list, int idstart = 0)
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

          if (GeometryUtil.PointInPolygon(p.Polygon.Points[0], list[j].Polygon).Value)
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
          id = this.ToTree(parents[i].Childs.ToArray(), id);
        }
      }

      return id;
    }

    public void ResponseProcessor(NestResult payload)
    {
      try
      {
        if (this.ga == null || payload == null)
        {
          // user might have quit while we're away
          return;
        }

        State.IncrementPopulation();
        State.SetLastNestTime(payload.BackgroundTime);
        State.SetLastPlacementTime(payload.PlacePartTime);
        State.IncrementNestCount();
        State.IncrementPlacementTime(payload.PlacePartTime);
        State.IncrementNestTime(payload.BackgroundTime);

#if NCRUNCH
      Trace.WriteLine("payload.Index I don't think is being set right; double check before retrying threaded execution.");
#endif
        this.ga.Population[payload.Index].Processing = false;
        this.ga.Population[payload.Index].Fitness = payload.Fitness;

        int currentPlacements = 0;
        string suffix = string.Empty;
        if (!payload.IsValid || payload.UsedSheets.Count == 0)
        {
          this.State.IncrementRejected();
          suffix = " Rejected";
        }
        else if (this.State.TopNestResults.TryAdd(payload))
        {
          if (this.State.TopNestResults.Contains(payload))
          {
            currentPlacements = this.State.TopNestResults.Top.UsedSheets[0].PartPlacements.Count;
            if (this.State.TopNestResults.IndexOf(payload) < this.State.TopNestResults.EliteSurvivors)
            {
              suffix = " Elite";
              this.progressDisplayer.DisplayTransientMessage($"New top {this.State.TopNestResults.MaxCapacity} nest found: nesting time = {payload.PlacePartTime}ms");
              this.progressDisplayer?.UpdateNestsList();
            }
            else
            {
              suffix = " Top";
            }
          }
          else
          {
            suffix = " Duplicate";
          }
        }
        else
        {
          this.progressDisplayer.DisplayTransientMessage($"Nesting time = {payload.PlacePartTime}ms");
          suffix = " Sub-optimal";
        }

        if (currentPlacements > 0)
        {
          progressDisplayer.DisplayProgress(currentPlacements, State.Population);
        }

        System.Diagnostics.Debug.Print($"Nest {payload.BackgroundTime}ms{suffix}");
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    /// <summary>
    /// Starts next generation if none started or prior finished. Will keep rehitting the outstanding population
    /// set up for the generation until all have processed.
    /// </summary>
    public void LaunchWorkers(NestItem<INfp>[] partNestItems, NestItem<ISheet>[] sheetNestItems, ISvgNestConfig config, INestStateBackground nestStateBackground)
    {
      try
      {
        if (!this.IsStopped)
        {
          if (this.ga == null)
          {
            this.ga = new Procreant(partNestItems, config, progressDisplayer);
          }

          if (this.ga.IsCurrentGenerationFinished)
          {
            // console.log('new generation!');
            // all individuals have been evaluated, start next generation

            this.ga.Generate();
            if (this.ga.Population.Length == 0)
            {
              this.Stop();
              this.messageService.DisplayMessageBox("Terminating the nest because we're just recalculating the same nests over and over again.", "Terminating Nest", MessageBoxIcon.Information);
            }

            State.IncrementGenerations();
            State.ResetPopulation();
          }

          var sheets = new List<ISheet>();
          var sheetids = new List<int>();
          var sheetsources = new List<int>();
          var sheetchildren = new List<List<INfp>>();
          var sid = 0;
          for (int i = 0; i < sheetNestItems.Count(); i++)
          {
            var poly = sheetNestItems[i].Polygon;
            for (int j = 0; j < sheetNestItems[i].Quantity; j++)
            {
              ISheet clone;
              if (poly is Sheet sheet)
              {
                clone = (ISheet)poly.CloneTree();
              }
              else
              {
#if DEBUG || NCRUNCH
                throw new InvalidOperationException("Sheet should have been a sheet; why wasn't it?");
#endif
                clone = new Sheet(poly.CloneTree(), WithChildren.Excluded);
              }

              clone.Id = sid; // id is the unique id of all parts that will be nested, including cloned duplicates
              clone.Source = poly.Source; // source is the id of each unique part from the main part list

              sheets.Add(new Sheet(clone, WithChildren.Excluded));
              sheetids.Add(sid);
              sheetsources.Add(poly.Source);
              sheetchildren.Add(poly.Children.ToList());
              sid++;
            }
          }

          this.progressDisplayer.DisplayTransientMessage("Executing Nest. . .");
          if (config.UseParallel)
          {
            var end1 = this.ga.Population.Length / 3;
            var end2 = this.ga.Population.Length * 2 / 3;
            var end3 = this.ga.Population.Length;
            Parallel.Invoke(
              () => ProcessPopulation(0, end1, config, sheets.ToArray(), sheetids.ToArray(), sheetsources.ToArray(), sheetchildren, nestStateBackground),
              () => ProcessPopulation(end1, end2, config, sheets.ToArray(), sheetids.ToArray(), sheetsources.ToArray(), sheetchildren, nestStateBackground),
              () => ProcessPopulation(end2, this.ga.Population.Length, config, sheets.ToArray(), sheetids.ToArray(), sheetsources.ToArray(), sheetchildren, nestStateBackground));
          }
          else
          {
            ProcessPopulation(0, this.ga.Population.Length, config, sheets.ToArray(), sheetids.ToArray(), sheetsources.ToArray(), sheetchildren, nestStateBackground);
          }
        }
      }
      catch (DllNotFoundException)
      {
        DisplayMinkowskiDllError();
        State.SetIsErrored();
      }
      catch (BadImageFormatException badImageEx)
      {
        if (badImageEx.StackTrace.Contains("Minkowski"))
        {
          DisplayMinkowskiDllError();
        }
        else
        {
          this.messageService.DisplayMessage(badImageEx);
        }

        State.SetIsErrored();
      }
      catch (Exception ex)
      {
        this.messageService.DisplayMessage(ex);
        State.SetIsErrored();
#if NCRUNCH
        throw;
#endif
      }
    }

    private void DisplayMinkowskiDllError()
    {
      this.messageService.DisplayMessageBox(
                  "An error has occurred attempting to execute the C++ Minkowski DllImport.\r\n" +
                  "\r\n" +
                  "You can turn off the C++ DllImport in Settings and use the internal C# implementation " +
                  "instead; but this is experimental. Alternatively try using another build (x86/x64) or " +
                  "recreate the Minkowski.Dlls on your own system.\r\n" +
                  "\r\n" +
                  "You can continue to use the import/edit/export functionality but unless you fix " +
                  "the problem/switch to the internal implementation you will be unable to execute " +
                  "any nests.",
                  "DeepNestSharp Error!",
                  MessageBoxIcon.Error);
    }

    private void ProcessPopulation(int start, int end, ISvgNestConfig config, ISheet[] sheets, int[] sheetids, int[] sheetsources, List<List<INfp>> sheetchildren, INestStateBackground nestStateBackground)
    {
      State.IncrementThreads();
      for (int i = start; i < end; i++)
      {
        if (this.IsStopped)
        {
          break;
        }

        // if(running < config.threads && !GA.population[i].processing && !GA.population[i].fitness){
        // only one background window now...
        var individual = ga.Population[i];
        if (!this.IsStopped && individual.IsPending)
        {
          individual.Processing = true;

          // hash values on arrays don't make it across ipc, store them in an array and reassemble on the other side....
          var ids = new int[individual.Parts.Count];
          var sources = new int[individual.Parts.Count];
          var children = new List<List<INfp>>();

          for (int j = 0; j < individual.Parts.Count; j++)
          {
            var placement = individual.Parts[j];
            ids[j] = placement.Id;
            sources[j] = placement.Source;
            children.Add(placement.Children.ToList());
          }

          var data = new DataInfo()
          {
            Index = i,
            Sheets = sheets,
            SheetIds = sheetids,
            SheetSources = sheetsources,
            SheetChildren = sheetchildren,
            Individual = individual,
            Ids = ids,
            Sources = sources,
            Children = children,
          };

          if (this.IsStopped)
          {
            this.ResponseProcessor(null);
          }
          else
          {
            var background = new Background(this.progressDisplayer, this, minkowskiSumService, nestStateBackground, config.UseDllImport);
            background.BackgroundStart(data, config);
          }
        }
      }

      State.DecrementThreads();
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