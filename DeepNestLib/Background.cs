﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading.Tasks;
  using ClipperLib;
  using DeepNestLib.Placement;
  using Light.GuardClauses;

  public class Background
  {
    private const bool EnableCaches = true;
    private readonly Dictionary<string, INfp[]> cacheProcess = new Dictionary<string, INfp[]>();

    // run the placement synchronously
    private IWindowUnk window = new WindowUnk();

    private Action<NestResult> responseAction;

    private static volatile object lockobj = new object();
    private readonly IProgressDisplayer progressDisplayer;

    public Background(IProgressDisplayer progressDisplayer, Action<NestResult> responseAction)
    {
      this.cacheProcess = new Dictionary<string, INfp[]>();
      this.window = new WindowUnk();
      this.progressDisplayer = progressDisplayer;
      this.responseAction = responseAction;
    }

    private static INfp ShiftPolygon(INfp p, PartPlacement shift)
    {
      NFP shifted = new NFP();
      for (var i = 0; i < p.Length; i++)
      {
        shifted.AddPoint(new SvgPoint(p[i].x + shift.x, p[i].y + shift.y) { Exact = p[i].Exact });
      }

      if (p.Children != null /*&& p.Children.Count*/)
      {
        for (int i = 0; i < p.Children.Count(); i++)
        {
          shifted.Children.Add(ShiftPolygon(p.Children[i], shift));
        }
      }

      return shifted;
    }

    // returns the square of the length of any merged lines
    // filter out any lines less than minlength long
    private static MergedResult MergedLength(INfp[] parts, INfp p, double minlength, double tolerance)
    {
      // var min2 = minlength * minlength;
      //            var totalLength = 0;
      //            var segments = [];

      // for (var i = 0; i < p.length; i++)
      //            {
      //                var A1 = p[i];

      // if (i + 1 == p.length)
      //                {
      //                    A2 = p[0];
      //                }
      //                else
      //                {
      //                    var A2 = p[i + 1];
      //                }

      // if (!A1.exact || !A2.exact)
      //                {
      //                    continue;
      //                }

      // var Ax2 = (A2.x - A1.x) * (A2.x - A1.x);
      //                var Ay2 = (A2.y - A1.y) * (A2.y - A1.y);

      // if (Ax2 + Ay2 < min2)
      //                {
      //                    continue;
      //                }

      // var angle = Math.atan2((A2.y - A1.y), (A2.x - A1.x));

      // var c = Math.cos(-angle);
      //                var s = Math.sin(-angle);

      // var c2 = Math.cos(angle);
      //                var s2 = Math.sin(angle);

      // var relA2 = { x: A2.x - A1.x, y: A2.y - A1.y};
      //            var rotA2x = relA2.x * c - relA2.y * s;

      // for (var j = 0; j < parts.length; j++)
      //            {
      //                var B = parts[j];
      //                if (B.length > 1)
      //                {
      //                    for (var k = 0; k < B.length; k++)
      //                    {
      //                        var B1 = B[k];

      // if (k + 1 == B.length)
      //                        {
      //                            var B2 = B[0];
      //                        }
      //                        else
      //                        {
      //                            var B2 = B[k + 1];
      //                        }

      // if (!B1.exact || !B2.exact)
      //                        {
      //                            continue;
      //                        }
      //                        var Bx2 = (B2.x - B1.x) * (B2.x - B1.x);
      //                        var By2 = (B2.y - B1.y) * (B2.y - B1.y);

      // if (Bx2 + By2 < min2)
      //                        {
      //                            continue;
      //                        }

      // // B relative to A1 (our point of rotation)
      //                        var relB1 = { x: B1.x - A1.x, y: B1.y - A1.y};
      //                    var relB2 = { x: B2.x - A1.x, y: B2.y - A1.y};

      // // rotate such that A1 and A2 are horizontal
      //                var rotB1 = { x: relB1.x* c -relB1.y * s, y: relB1.x* s +relB1.y * c};
      //            var rotB2 = { x: relB2.x* c -relB2.y * s, y: relB2.x* s +relB2.y * c};

      // if(!GeometryUtil.almostEqual(rotB1.y, 0, tolerance) || !GeometryUtil.almostEqual(rotB2.y, 0, tolerance)){
      // continue;
      // }

      // var min1 = Math.min(0, rotA2x);
      //        var max1 = Math.max(0, rotA2x);

      // var min2 = Math.min(rotB1.x, rotB2.x);
      //        var max2 = Math.max(rotB1.x, rotB2.x);

      // // not overlapping
      // if(min2 >= max1 || max2 <= min1){
      // continue;
      // }

      // var len = 0;
      //        var relC1x = 0;
      //        var relC2x = 0;

      // // A is B
      // if(GeometryUtil.almostEqual(min1, min2) && GeometryUtil.almostEqual(max1, max2)){
      // len = max1-min1;
      // relC1x = min1;
      // relC2x = max1;
      // }
      // // A inside B
      // else if(min1 > min2 && max1<max2){
      // len = max1-min1;
      // relC1x = min1;
      // relC2x = max1;
      // }
      // // B inside A
      // else if(min2 > min1 && max2<max1){
      // len = max2-min2;
      // relC1x = min2;
      // relC2x = max2;
      // }
      // else{
      // len = Math.max(0, Math.min(max1, max2) - Math.max(min1, min2));
      // relC1x = Math.min(max1, max2);
      // relC2x = Math.max(min1, min2);
      // }

      // if(len* len > min2){
      // totalLength += len;

      // var relC1 = { x: relC1x * c2, y: relC1x * s2 };
      // var relC2 = { x: relC2x * c2, y: relC2x * s2 };

      // var C1 = { x: relC1.x + A1.x, y: relC1.y + A1.y };
      // var C2 = { x: relC2.x + A1.x, y: relC2.y + A1.y };

      // segments.push([C1, C2]);
      // }
      // }
      // }

      // if(B.Children && B.Children.length > 0){
      // var child = mergedLength(B.Children, p, minlength, tolerance);
      // totalLength += child.totalLength;
      // segments = segments.concat(child.segments);
      // }
      // }
      // }

      // return {totalLength: totalLength, segments: segments};
      throw new NotImplementedException();
    }

    public class MergedResult
    {
      public double totalLength;
      public object segments;
    }

    public static int CallCounter
    {
      get
      {
        return MinkowskiSum.CallCounter;
      }
    }

    internal INfp[] ExecuteDllImportMinkowski(INfp A, INfp B, MinkowskiCache minkowskiCache)
    {
      var key = new StringBuilder(12).Append(A.Source).Append(";").Append(B.Source).Append(";").Append(A.Rotation).Append(";").Append(B.Rotation).ToString();
      bool cacheAllow = minkowskiCache == MinkowskiCache.Cache;

      if (cacheProcess.ContainsKey(key) && cacheAllow)
      {
        return cacheProcess[key];
      }

      var ret = MinkowskiSum.DllImportExecute(A, B);
      var res = new INfp[] { ret };
      if (cacheAllow)
      {
        cacheProcess.Add(key, res);
      }

      return res;
    }

    private static INfp GetFrame(INfp A)
    {
      var bounds = GeometryUtil.getPolygonBounds(A);

      // expand bounds by 10%
      bounds.width *= 1.1;
      bounds.height *= 1.1;
      bounds.x -= 0.5 * (bounds.width - (bounds.width / 1.1));
      bounds.y -= 0.5 * (bounds.height - (bounds.height / 1.1));

      var frame = new NFP(new List<INfp>() { A });
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.x, bounds.y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.x + bounds.width, bounds.y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.x + bounds.width, bounds.y + bounds.height));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.x, bounds.y + bounds.height));

      frame.Source = A.Source;
      frame.Rotation = 0;

      return frame;
    }

    private INfp[] GetInnerNfp(INfp a, INfp b, MinkowskiCache type, double clipperScale)
    {
      a.MustNotBeNull();
      b.MustNotBeNull();

      var key = new DbCacheKey(a.Source, b.Source, 0, b.Rotation);

      // var doc = window.db.find({ A: A.source, B: B.source, Arotation: 0, Brotation: B.rotation }, true);
      var res = window.Find(key, true);
      if (res != null)
      {
        return res;
      }

      var frame = GetFrame(a);

      var nfp = GetOuterNfp(frame, b, type, true);

      if (nfp == null || nfp.Children == null || nfp.Children.Count == 0)
      {
        return null;
      }

      List<INfp> holes = new List<INfp>();
      if (a.Children != null && a.Children.Count > 0)
      {
        for (var i = 0; i < a.Children.Count; i++)
        {
          var hnfp = GetOuterNfp(a.Children[i], b, MinkowskiCache.NoCache);
          if (hnfp != null)
          {
            holes.Add(hnfp);
          }
        }
      }

      if (holes.Count == 0)
      {
        return nfp.Children.ToArray();
      }

      var clipperNfp = InnerNfpToClipperCoordinates(nfp.Children.ToArray(), clipperScale);
      var clipperHoles = InnerNfpToClipperCoordinates(holes.ToArray(), clipperScale);

      List<List<IntPoint>> finalNfp = new List<List<IntPoint>>();
      var clipper = new ClipperLib.Clipper();

      clipper.AddPaths(clipperHoles.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptClip, true);
      clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);

      if (!clipper.Execute(ClipperLib.ClipType.ctDifference, finalNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
      {
        return nfp.Children.ToArray();
      }

      if (finalNfp.Count == 0)
      {
        return null;
      }

      List<NFP> f = new List<NFP>();
      for (var i = 0; i < finalNfp.Count; i++)
      {
        f.Add(finalNfp[i].ToArray().ToNestCoordinates(clipperScale));
      }

      if (a.Source != null && b.Source != null)
      {
        // insert into db
        // console.log('inserting inner: ', A.source, B.source, B.rotation, f);
        var doc = new DbCacheKey(a.Source, b.Source, 0, b.Rotation, f.ToArray());
        window.Insert(doc, true);
      }

      return f.ToArray();
    }

    internal NestResult PlaceParts(IEnumerable<INfp> sheets, INfp[] parts, ISvgNestConfig config)
    {
      if (sheets == null || sheets.Count() == 0)
      {
        return null;
      }

      Stopwatch sw = new Stopwatch();
      sw.Start();

      var unusedSheets = new Stack<INfp>(sheets.Reverse());
      NFP part = null;

      // total length of merged lines
      double totalMerged = 0;

      // rotate paths by given rotation
      var rotated = new List<NFP>();
      for (int i = 0; i < parts.Length; i++)
      {
        var r = parts[i].Rotate(parts[i].Rotation);
        r.Rotation = parts[i].Rotation;
        r.Source = parts[i].Source;
        r.Id = parts[i].Id;
        rotated.Add(r);
      }

      var unplacedParts = rotated.ToArray();
      SheetPlacementCollection allPlacements = new SheetPlacementCollection();
      INfp nfp;
      bool isPriorityPlacement;
      while (unplacedParts.Length > 0 && unusedSheets.Count > 0)
      {
        var done = parts.Length - unplacedParts.Length;
        if (done > 5 && sw.ElapsedMilliseconds / done * parts.Length > 5000)
        {
          this.progressDisplayer.DisplayProgress((float)done / parts.Length);
        }

        List<INfp> placed = new List<INfp>();
        IList<PartPlacement> placements = new List<PartPlacement>();

        // open a new sheet
        INfp sheet = null;
        var requeue = new Queue<INfp>();
        while (unusedSheets.Count > 0 && sheet == null)
        {
          sheet = unusedSheets.Pop();
          if (allPlacements.Any(o => o.Sheet == sheet))
          {
            var sheetPlacement = allPlacements.Single(o => o.Sheet == sheet);
            placements = sheetPlacement.PartPlacements;
            placed = sheetPlacement.PartPlacements.Select(o => o.Part).ToList();
            if (unplacedParts.Any(o => o.IsPriority))
            {
              // Sheet's already used so by definition it's already full of priority parts, no point trying to add more
              requeue.Enqueue(sheet);
              sheet = null;
            }
            else
            {
              // Sheet's already used for priority parts but there's no priority parts left so fill spaces with non-priority
              allPlacements.Remove(sheetPlacement);
            }
          }
          else
          {
            //it's a new sheet so just go ahead and use it for whatever's left
            placements = new List<PartPlacement>();
            placed = new List<INfp>();
          }
        }

        if (sheet == null)
        {
          break;
        }

        string clipkey = string.Empty;
        Dictionary<string, ClipCacheItem> clipCache = new Dictionary<string, ClipCacheItem>();
        var clipper = new ClipperLib.Clipper();
        var combinedNfp = new List<List<ClipperLib.IntPoint>>();
        var error = false;
        IntPoint[][] clipperSheetNfp = null;
        double? minwidth = null;
        double? minarea = null;

        isPriorityPlacement = unplacedParts.Any(o => o.IsPriority);
        var processingParts = (isPriorityPlacement ? unplacedParts.Where(o => o.IsPriority) : unplacedParts).ToArray();
        for (int i = 0; i < processingParts.Length; i++)
        {
          part = processingParts[i];

          // inner NFP
          INfp[] sheetNfp = null;
          var canBePlaced = false;

          // try all possible rotations until it fits
          // (only do this for the first part of each sheet, to ensure that all parts that can be placed are, even if we have to to open a lot of sheets)
          for (int j = 0; j < (360f / config.Rotations); j++)
          {
            if (CanBePlaced(sheet, part, config.ClipperScale, out sheetNfp))
            {
              canBePlaced = true;
              break;
            }

            var r = part.Rotate(360f / config.Rotations);
            r.Rotation = part.Rotation + (360f / config.Rotations);
            r.Source = part.Source;
            r.Id = part.Id;

            // rotation is not in-place
            part = r;
            processingParts[i] = r;

            if (part.Rotation > 360f)
            {
              part.Rotation = part.Rotation % 360f;
            }
          }

          // part unplaceable, skip
          if (!canBePlaced)
          {
            continue;
          }

          PartPlacement position = null;
          if (placed.Count == 0)
          {
            // first placement, put it on the bottom left corner
            for (int j = 0; j < sheetNfp.Count(); j++)
            {
              for (int k = 0; k < sheetNfp[j].Length; k++)
              {
                if (position == null ||
                    ((sheetNfp[j][k].x - part[0].x) < position.x) ||
                    (
                    GeometryUtil._almostEqual(sheetNfp[j][k].x - part[0].x, position.x)
                    && ((sheetNfp[j][k].y - part[0].y) < position.y)))
                {
                  position = new PartPlacement(part)
                  {
                    x = sheetNfp[j][k].x - part[0].x,
                    y = sheetNfp[j][k].y - part[0].y,
                    id = part.Id,
                    rotation = part.Rotation,
                    source = part.Source,
                  };
                }
              }
            }

            if (position == null)
            {
              throw new Exception("position null");

              // console.log(sheetNfp);
            }

            placements.Add(position);
            placed.Add(part);
          }
          else
          {
            clipperSheetNfp = InnerNfpToClipperCoordinates(sheetNfp, config.ClipperScale);

            clipper = new ClipperLib.Clipper();
            combinedNfp = new List<List<ClipperLib.IntPoint>>();

            error = false;

            // check if stored in clip cache
            // var startindex = 0;
            clipkey = "s:" + part.Source + "r:" + part.Rotation;
            var startindex = 0;
            if (EnableCaches && clipCache.ContainsKey(clipkey))
            {
              var prevNfp = clipCache[clipkey].nfpp;
              clipper.AddPaths(prevNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);
              startindex = clipCache[clipkey].index;
            }

            for (int j = startindex; j < placed.Count; j++)
            {
              nfp = GetOuterNfp(placed[j], part, 0);

              // minkowski difference failed. very rare but could happen
              if (nfp == null)
              {
                error = true;
                break;
              }

              // shift to placed location
              for (int m = 0; m < nfp.Length; m++)
              {
                nfp[m].x += placements[j].x;
                nfp[m].y += placements[j].y;
              }

              if (nfp.Children != null && nfp.Children.Count > 0)
              {
                for (int n = 0; n < nfp.Children.Count; n++)
                {
                  for (var o = 0; o < nfp.Children[n].Length; o++)
                  {
                    nfp.Children[n][o].x += placements[j].x;
                    nfp.Children[n][o].y += placements[j].y;
                  }
                }
              }

              var clipperNfp = NfpToClipperCoordinates(nfp, config.ClipperScale);

              clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);
            }

            // TODO: a lot here to insert
            if (error || !clipper.Execute(ClipperLib.ClipType.ctUnion, combinedNfp, ClipperLib.PolyFillType.pftNonZero, ClipperLib.PolyFillType.pftNonZero))
            {
              // console.log('clipper error', error);
              continue;
            }

            if (EnableCaches)
            {
              clipCache[clipkey] = new ClipCacheItem()
              {
                index = placed.Count - 1,
                nfpp = combinedNfp.Select(z => z.ToArray()).ToArray(),
              };
            }

            // console.log('save cache', placed.length - 1);

            // difference with sheet polygon
            List<List<IntPoint>> differenceWithSheetPolygonNfpPoints = new List<List<IntPoint>>();
            clipper = new ClipperLib.Clipper();

            clipper.AddPaths(combinedNfp, ClipperLib.PolyType.ptClip, true);

            clipper.AddPaths(clipperSheetNfp.Select(z => z.ToList()).ToList(), ClipperLib.PolyType.ptSubject, true);

            if (!clipper.Execute(ClipperLib.ClipType.ctDifference, differenceWithSheetPolygonNfpPoints, ClipperLib.PolyFillType.pftEvenOdd, ClipperLib.PolyFillType.pftNonZero))
            {
              continue;
            }

            if (differenceWithSheetPolygonNfpPoints == null || differenceWithSheetPolygonNfpPoints.Count == 0)
            {
              if (part.IsPriority)
              {
                break; /* Failed to fit a Primary part; add another sheet.
                          However that means we'll leave additional space on the first sheet though that won't get used again
                          as everything remaining will be fit to the consequent sheet? */
              }

              continue; // Part can't be fitted but it wasn't a primary, so move on to the next part
            }

            List<NFP> differenceWithSheetPolygonNfp = new List<NFP>();
            for (int j = 0; j < differenceWithSheetPolygonNfpPoints.Count; j++)
            {
              // back to normal scale
              differenceWithSheetPolygonNfp.Add(differenceWithSheetPolygonNfpPoints[j].ToArray().ToNestCoordinates(config.ClipperScale));
            }

            var finalNfp = differenceWithSheetPolygonNfp;

            // choose placement that results in the smallest bounding box/hull etc
            // todo: generalize gravity direction
            /*var minwidth = null;
            var minarea = null;
            var minx = null;
            var miny = null;
            var nf, area, shiftvector;*/
            minwidth = null;
            minarea = null;
            double? minx = null;
            double? miny = null;
            NFP nf;
            double area;
            PartPlacement shiftvector = null;

            NFP allpoints = SheetPlacement.CombinedPoints(placements);
            PolygonBounds allbounds = null;
            PolygonBounds partbounds = null;
            if (config.PlacementType == PlacementTypeEnum.Gravity || config.PlacementType == PlacementTypeEnum.BoundingBox)
            {
              allbounds = GeometryUtil.getPolygonBounds(allpoints);

              NFP partpoints = new NFP();
              for (int m = 0; m < part.Length; m++)
              {
                partpoints.AddPoint(new SvgPoint(part[m].x, part[m].y));
              }

              partbounds = GeometryUtil.getPolygonBounds(partpoints);
            }
            else
            {
              allpoints = allpoints.GetHull();
            }

            for (int j = 0; j < finalNfp.Count; j++)
            {
              nf = finalNfp[j];

              // console.log('evalnf',nf.length);
              for (int k = 0; k < nf.Length; k++)
              {
                shiftvector = new PartPlacement(part)
                {
                  id = part.Id,
                  x = nf[k].x - part[0].x,
                  y = nf[k].y - part[0].y,
                  source = part.Source,
                  rotation = part.Rotation,
                };

                PolygonBounds rectbounds = null;
                if (config.PlacementType == PlacementTypeEnum.Gravity || config.PlacementType == PlacementTypeEnum.BoundingBox)
                {
                  NFP poly = new NFP();
                  poly.AddPoint(new SvgPoint(allbounds.x, allbounds.y));
                  poly.AddPoint(new SvgPoint(allbounds.x + allbounds.width, allbounds.y));
                  poly.AddPoint(new SvgPoint(allbounds.x + allbounds.width, allbounds.y + allbounds.height));
                  poly.AddPoint(new SvgPoint(allbounds.x, allbounds.y + allbounds.height));

                  poly.AddPoint(new SvgPoint(partbounds.x + shiftvector.x, partbounds.y + shiftvector.y));
                  poly.AddPoint(new SvgPoint(partbounds.x + partbounds.width + shiftvector.x, partbounds.y + shiftvector.y));
                  poly.AddPoint(new SvgPoint(partbounds.x + partbounds.width + shiftvector.x, partbounds.y + partbounds.height + shiftvector.y));
                  poly.AddPoint(new SvgPoint(partbounds.x + shiftvector.x, partbounds.y + partbounds.height + shiftvector.y));

                  rectbounds = GeometryUtil.getPolygonBounds(poly);

                  // weigh width more, to help compress in direction of gravity
                  if (config.PlacementType == PlacementTypeEnum.Gravity)
                  {
                    area = (rectbounds.width * 3) + rectbounds.height;
                  }
                  else
                  {
                    area = rectbounds.width * rectbounds.height;
                  }
                }
                else
                {
                  // must be convex hull
                  var localpoints = allpoints.Clone();

                  for (int m = 0; m < part.Length; m++)
                  {
                    localpoints.AddPoint(new SvgPoint(part[m].x + shiftvector.x, part[m].y + shiftvector.y));
                  }

                  area = Math.Abs(GeometryUtil.polygonArea(localpoints.GetHull()));
                  shiftvector.hull = localpoints.GetHull();
                  shiftvector.hullsheet = sheet.GetHull();
                }

                // console.timeEnd('evalbounds');
                // console.time('evalmerge');
                MergedResult merged = null;
                if (config.MergeLines)
                {
                  throw new NotImplementedException();

                  // if lines can be merged, subtract savings from area calculation
                  var shiftedpart = ShiftPolygon(part, shiftvector);
                  List<INfp> shiftedplaced = new List<INfp>();

                  for (int m = 0; m < placed.Count; m++)
                  {
                    shiftedplaced.Add(ShiftPolygon(placed[m], placements[m]));
                  }

                  // don't check small lines, cut off at about 1/2 in
                  double minlength = 0.5 * config.Scale;
                  merged = MergedLength(shiftedplaced.ToArray(), shiftedpart, minlength, 0.1 * config.CurveTolerance);
                  area -= merged.totalLength * config.TimeRatio;
                }

                // console.timeEnd('evalmerge');
                if (minarea == null ||
                    area < minarea ||
                    (GeometryUtil._almostEqual(minarea, area) && (minx == null || shiftvector.x < minx)) ||
                    (GeometryUtil._almostEqual(minarea, area) && (minx != null && GeometryUtil._almostEqual(shiftvector.x, minx) && shiftvector.y < miny)))
                {
                  minarea = area;

                  minwidth = rectbounds != null ? rectbounds.width : 0;
                  position = shiftvector;
                  if (minx == null || shiftvector.x < minx)
                  {
                    minx = shiftvector.x;
                  }

                  if (miny == null || shiftvector.y < miny)
                  {
                    miny = shiftvector.y;
                  }

                  if (config.MergeLines)
                  {
                    position.mergedLength = merged.totalLength;
                    position.mergedSegments = merged.segments;
                  }
                }
              }
            }

            if (position != null)
            {
              placed.Add(part);
              placements.Add(position);
              if (position.mergedLength.HasValue)
              {
                totalMerged += position.mergedLength.Value;
              }
            }
            else if (part.IsPriority)
            {
              break;
            }

            // send placement progress signal
            var placednum = placed.Count;
            for (int j = 0; j < allPlacements.Count; j++)
            {
              placednum += allPlacements[j].PartPlacements.Count;
            }
          }
        }

        for (int i = 0; i < placed.Count; i++)
        {
          var index = Array.IndexOf(unplacedParts, placed[i]);
          if (index >= 0)
          {
            unplacedParts = unplacedParts.Splice(index, 1);
          }
        }

        if (isPriorityPlacement && unplacedParts.Length > 0)
        {
          unusedSheets.Push(sheet);
        }

        while (requeue.Count > 0)
        {
          unusedSheets.Push(requeue.Dequeue());
        }

        if (placements != null && placements.Count > 0)
        {
          allPlacements.Add(new SheetPlacement(config.PlacementType, sheet, placements));
        }
        else
        {
          break; // something went wrong
        }
      }

      return new NestResult(allPlacements, unplacedParts, totalMerged, config.PlacementType, sw.ElapsedMilliseconds);
    }

    private bool CanBePlaced(INfp sheet, INfp part, double clipperScale, out INfp[] sheetNfp)
    {
      sheetNfp = GetInnerNfp(sheet, part, 0, clipperScale);
      if (sheetNfp != null && sheetNfp.Count() > 0)
      {
        if (sheetNfp[0].Length == 0)
        {
          throw new ArgumentException();
        }
        else
        {
          return true;
        }
      }

      return false;
    }

    private void SyncPlaceParts(NFP[] parts, INfp[] sheets, ISvgNestConfig config, int index)
    {
      var nestResult = PlaceParts(sheets, parts, config);
      if (nestResult != null)
      {
        nestResult.index = index;
        this.responseAction(nestResult);
      }
    }

    internal void BackgroundStart(IDataInfo data, ISvgNestConfig config)
    {
      try
      {
        var individual = data.Individual;

        var parts = individual.Parts.ToArray();
        var rotations = individual.Rotation;
        var ids = data.Ids;
        var sources = data.Sources;
        var children = data.Children;

        for (var i = 0; i < parts.Length; i++)
        {
          parts[i].Rotation = rotations[i];
          parts[i].Id = ids[i];
          parts[i].Source = sources[i];
          if (!config.Simplify)
          {
            parts[i].Children = children[i];
          }
        }

        var sheets = data.Sheets;
        for (int i = 0; i < sheets.Length; i++)
        {
          var sheet = sheets[i];
          sheet.Id = data.SheetIds[i];
          sheet.Source = data.SheetSources[i];
          sheet.Children = data.SheetChildren[i];
        }

        // preprocess
        List<NfpPair> pairs = new NfpPairsFactory(window).Generate(config.UseParallel, parts.ToArray());

        // console.log('pairs: ', pairs.length);
        // console.time('Total');
        if (pairs.Count > 0)
        {
          var pmapWorker = new PmapWorker(pairs, progressDisplayer, config.UseParallel);
          var pmapResult = pmapWorker.PmapDeepNest();
          this.ThenDeepNest(pmapResult, parts, data.Sheets, config, data.Index);
        }
        else
        {
          this.SyncPlaceParts(parts, data.Sheets, config, data.Index);
        }
      }
      catch (ArgumentNullException)
      {
        throw;
      }
      catch (DllNotFoundException)
      {
        throw;
      }
      catch (BadImageFormatException)
      {
        throw;
      }
      catch (SEHException)
      {
        throw;
      }
    }

    public NFP GetPart(int source, NFP[] parts)
    {
      for (var k = 0; k < parts.Length; k++)
      {
        if (parts[k].Source == source)
        {
          return parts[k];
        }
      }

      return null;
    }

    private void ThenIterate(NfpPair processed, NFP[] parts, double clipperScale)
    {
      // returned data only contains outer nfp, we have to account for any holes separately in the synchronous portion
      // this is because the c++ addon which can process interior nfps cannot run in the worker thread
      var A = this.GetPart(processed.Asource, parts);
      var B = this.GetPart(processed.Bsource, parts);

      List<NFP> Achildren = new List<NFP>();

      if (A.Children != null)
      {
        for (int j = 0; j < A.Children.Count; j++)
        {
          Achildren.Add(A.Children[j].Rotate(processed.ARotation));
        }
      }

      if (Achildren.Count > 0)
      {
        var Brotated = B.Rotate(processed.BRotation);
        var bbounds = GeometryUtil.getPolygonBounds(Brotated);
        List<INfp> cnfp = new List<INfp>();

        for (int j = 0; j < Achildren.Count; j++)
        {
          var cbounds = GeometryUtil.getPolygonBounds(Achildren[j]);
          if (cbounds.width > bbounds.width && cbounds.height > bbounds.height)
          {
            var n = GetInnerNfp(Achildren[j], Brotated, MinkowskiCache.NoCache, clipperScale);
            if (n != null && n.Count() > 0)
            {
              cnfp.AddRange(n);
            }
          }
        }

        processed.nfp.Children = cnfp;
      }

      DbCacheKey doc = new DbCacheKey(processed.Asource, processed.Bsource, processed.ARotation, processed.BRotation, new[] { processed.nfp });

      /*var doc = {
              A: processed[i].Asource,
              B: processed[i].Bsource,
              Arotation: processed[i].Arotation,
              Brotation: processed[i].Brotation,
              nfp: processed[i].nfp

          };*/
      window.Insert(doc);
    }

    private void ThenDeepNest(NfpPair[] nfpPairs, NFP[] parts, INfp[] sheets, ISvgNestConfig config, int index)
    {
      int cnt = 0;
      if (config.UseParallel)
      {
        Parallel.For(0, nfpPairs.Count(), (i) =>
        {
          this.ThenIterate(nfpPairs[i], parts, config.ClipperScale);
        });
      }
      else
      {
        for (var i = 0; i < nfpPairs.Count(); i++)
        {
          this.ThenIterate(nfpPairs[i], parts, config.ClipperScale);
        }
      }

      // console.timeEnd('Total');
      // console.log('before sync');
      this.SyncPlaceParts(parts, sheets, config, index);
    }

    // returns clipper nfp. Remember that clipper nfp are a list of polygons, not a tree!
    public static IntPoint[][] NfpToClipperCoordinates(INfp nfp, double clipperScale)
    {
      List<IntPoint[]> clipperNfp = new List<IntPoint[]>();

      // children first
      if (nfp.Children != null && nfp.Children.Count > 0)
      {
        for (var j = 0; j < nfp.Children.Count; j++)
        {
          if (GeometryUtil.polygonArea(nfp.Children[j]) < 0)
          {
            nfp.Children[j].reverse();
          }

          // var childNfp = SvgNest.toClipperCoordinates(nfp.Children[j]);
          var childNfp = DeepNestClipper.ScaleUpPaths(nfp.Children[j].Points, clipperScale);
          clipperNfp.Add(childNfp);
        }
      }

      if (GeometryUtil.polygonArea(nfp) > 0)
      {
        nfp.reverse();
      }

      // var outerNfp = SvgNest.toClipperCoordinates(nfp);

      // clipper js defines holes based on orientation
      var outerNfp = DeepNestClipper.ScaleUpPaths(nfp.Points, clipperScale);

      // var cleaned = ClipperLib.Clipper.CleanPolygon(outerNfp, 0.00001*config.clipperScale);
      clipperNfp.Add(outerNfp);

      // var area = Math.abs(ClipperLib.Clipper.Area(cleaned));
      return clipperNfp.ToArray();
    }

    // inner nfps can be an array of nfps, outer nfps are always singular
    public static IntPoint[][] InnerNfpToClipperCoordinates(INfp[] nfp, double clipperScale)
    {
      List<IntPoint[]> clipperNfp = new List<IntPoint[]>();
      for (var i = 0; i < nfp.Count(); i++)
      {
        var clip = NfpToClipperCoordinates(nfp[i], clipperScale);
        clipperNfp.AddRange(clip);

        // clipperNfp = clipperNfp.Concat(new[] { clip }).ToList();
      }

      return clipperNfp.ToArray();
    }

    private INfp GetOuterNfp(INfp A, INfp B, MinkowskiCache type, bool inside = false) // todo:?inside def?
    {
      INfp[] nfp = null;

      var key = new DbCacheKey(A.Source, B.Source, A.Rotation, B.Rotation);

      lock (lockobj)
      {
      var doc = window.Find(key);
      if (doc != null)
      {
        return doc.First();
      }

      // not found in cache
      if (inside || (A.Children != null && A.Children.Count > 0))
      {
          nfp = ExecuteDllImportMinkowski(A, B, type);
        }
      else
      {
        NFP clipperNfp = MinkowskiSum.ClipperExecute(A, B, MinkowskiSumPick.Smallest);
        nfp = new NFP[] { new NFP(clipperNfp.Points) };
      }

      if (nfp == null || nfp.Length == 0)
      {
        // console.log('holy shit', nfp, A, B, JSON.stringify(A), JSON.stringify(B));
        return null;
      }

        INfp nfps = nfp.FirstOrDefault();
      if (nfps == null || nfps.Length == 0)
      {
        return null;
      }

        if (!inside)
      {
        var doc2 = new DbCacheKey(A.Source, B.Source, A.Rotation, B.Rotation, nfp);
        window.Insert(doc2);
      }

      /*
      if (!inside && typeof A.source !== 'undefined' && typeof B.source !== 'undefined')
      {
          // insert into db
          doc = {
              A: A.source,
      B: B.source,
      Arotation: A.rotation,
      Brotation: B.rotation,
      nfp: nfp

    };
          window.db.insert(doc);
      }
      */
      return nfps;
    }
  }
  }
}
