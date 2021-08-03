namespace DeepNestLib
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

    private static volatile object lockobj = new object();

    private readonly Dictionary<string, INfp[]> cacheProcess = new Dictionary<string, INfp[]>();
    private readonly IProgressDisplayer progressDisplayer;
    private readonly SvgNest nest;
    private readonly IMinkowskiSumService minkowskiSumService;

    // run the placement synchronously
    private IWindowUnk window = new WindowUnk();

    public Action<SvgNest, NestResult> ResponseAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="Background"/> class.
    /// Needs to be totally self contained so it can calculate multiple nests in parallel.
    /// </summary>
    /// <param name="progressDisplayer">Callback access to the executing UI</param>
    /// <param name="nest">Passed in because have had issues with nest.ResponseProcessor accepting responses after a new nest has already been started.</param>
    /// <param name="minkowskiSumService">MinkowskiSumService used to inject algorithms to calculate the No-Fit-Polygons critical to DeepNest.</param>
    public Background(IProgressDisplayer progressDisplayer, SvgNest nest, IMinkowskiSumService minkowskiSumService)
    {
      this.cacheProcess = new Dictionary<string, INfp[]>();
      this.window = new WindowUnk();
      this.progressDisplayer = progressDisplayer;
      this.nest = nest;
      this.minkowskiSumService = minkowskiSumService;
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
            nfp.Children[j].Reverse();
          }

          // var childNfp = SvgNest.toClipperCoordinates(nfp.Children[j]);
          var childNfp = DeepNestClipper.ScaleUpPaths(nfp.Children[j].Points, clipperScale);
          clipperNfp.Add(childNfp);
        }
      }

      if (GeometryUtil.polygonArea(nfp) > 0)
      {
        nfp.Reverse();
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
        List<NfpPair> pairs = new NfpPairsFactory(window).Generate(config.UseParallel, parts);

        // console.log('pairs: ', pairs.length);
        // console.time('Total');
        if (pairs.Count > 0)
        {
          var pmapWorker = new PmapWorker(pairs, progressDisplayer, config.UseParallel, minkowskiSumService);
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

    internal INfp[] ExecuteDllImportMinkowski(INfp a, INfp b, MinkowskiCache minkowskiCache)
    {
      var key = new StringBuilder(12).Append(a.Source).Append(";").Append(b.Source).Append(";").Append(a.Rotation).Append(";").Append(b.Rotation).ToString();
      bool cacheAllow = minkowskiCache == MinkowskiCache.Cache;

      if (cacheProcess.ContainsKey(key) && cacheAllow)
      {
        return cacheProcess[key];
      }

      var ret = minkowskiSumService.DllImportExecute(a, b, MinkowskiSumCleaning.None);
      var res = new INfp[] { ret };
      if (cacheAllow)
      {
        cacheProcess.Add(key, res);
      }

      return res;
    }

    internal NestResult PlaceParts(IEnumerable<INfp> sheets, INfp[] parts, ISvgNestConfig config)
    {
      VerboseLog("PlaceParts");
      if (sheets == null || sheets.Count() == 0)
      {
        return null;
      }

      Stopwatch sw = new Stopwatch();
      sw.Start();

      var unusedSheets = new Stack<INfp>(sheets.Reverse());

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
      while (unplacedParts.Length > 0 && unusedSheets.Count > 0)
      {
        List<INfp> placed = new List<INfp>();
        var placements = new List<IPartPlacement>();

        // open a new sheet
        INfp sheet = null;
        var requeue = new Queue<INfp>();
        while (unusedSheets.Count > 0 && sheet == null)
        {
          sheet = unusedSheets.Pop();
          if (allPlacements.Any(o => o.Sheet == sheet))
          {
            var sheetPlacement = allPlacements.Single(o => o.Sheet == sheet);
            placements = sheetPlacement.PartPlacements.ToList();
            placed = sheetPlacement.PartPlacements.Select(o => o.Part).ToList();
            if (unplacedParts.Any(o => o.IsPriority))
            {
              // Sheet's already used so by definition it's already full of priority parts, no point trying to add more
              requeue.Enqueue(sheet);
              sheet = null;
            }
            else
            {
              VerboseLog($"Using sheet {sheet.Id}:{sheet.Source} because although it's already used for {placed.Count()} priority parts there's no priority parts left so try fill spaces with non-priority:");
              allPlacements.Remove(sheetPlacement);
            }
          }
          else
          {
            VerboseLog($"Using sheet {sheet.ToShortString()} because it's a new sheet so just go ahead and use it for whatever's left:");
            placements = new List<IPartPlacement>();
            placed = new List<INfp>();
          }
        }

        if (sheet == null)
        {
          VerboseLog("No sheets left to place parts upon; break and end the nest.");
          break;
        }

        bool isPriorityPlacement = unplacedParts.Any(o => o.IsPriority);
        if (isPriorityPlacement)
        {
          VerboseLog("Priority Placement.");
        }

        Dictionary<string, ClipCacheItem> clipCache = new Dictionary<string, ClipCacheItem>();
        var processingParts = (isPriorityPlacement ? unplacedParts.Where(o => o.IsPriority) : unplacedParts).ToArray();
        for (int i = 0; i < processingParts.Length; i++)
        {
          var combinedNfp = new List<List<IntPoint>>();
          double? minwidth = null;
          double? minarea = null;

          NFP part = processingParts[i];
          VerboseLog($"Process {i}:{part.ToShortString()}.");

          // inner NFP
          INfp[] sheetNfp = null;
          if (placed.Count == 0)
          {
            var canBePlaced = false;

            // try all possible rotations until it fits
            // (only do this for the first part of each sheet, to ensure that all parts that can be placed are, even if we have to to open a lot of sheets)
            for (int j = 0; j < (360f / config.Rotations); j++)
            {
              if (CanBePlaced(sheet, part, config.ClipperScale, out sheetNfp))
              {
                VerboseLog($"{part.ToShortString()} could be placed if sheet empty (only do this for the first part on each sheet).");
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
              VerboseLog($"{part.ToShortString()} could not be placed even if sheet empty (only do this for the first part on each sheet).");
              continue;
            }
          }

          PartPlacement position = null;
          if (placed.Count == 0)
          {
            VerboseLog("First placement, put it on the bottom left corner. . .");
            // first placement, put it on the bottom left corner
            for (int j = 0; j < sheetNfp.Count(); j++)
            {
              for (int k = 0; k < sheetNfp[j].Length; k++)
              {
                if (position == null ||
                    ((sheetNfp[j][k].X - part[0].X) < position.X) ||
                    (
                    GeometryUtil._almostEqual(sheetNfp[j][k].X - part[0].X, position.X)
                    && ((sheetNfp[j][k].Y - part[0].Y) < position.Y)))
                {
                  position = new PartPlacement(part)
                  {
                    X = sheetNfp[j][k].X - part[0].X,
                    Y = sheetNfp[j][k].Y - part[0].Y,
                    Id = part.Id,
                    Rotation = part.Rotation,
                    Source = part.Source,
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
            VerboseLog($"Add part {part}");
            placed.Add(part);
          }
          else if (CanBePlaced(sheet, part, config.ClipperScale, out sheetNfp))
          {
            var clipper = new Clipper();
            string clipkey = "s:" + part.Source + "r:" + part.Rotation;
            var error = false;
            IntPoint[][] clipperSheetNfp = InnerNfpToClipperCoordinates(sheetNfp, config.ClipperScale);

            // check if stored in clip cache
            // var startindex = 0;
            var startIndex = 0;
            if (EnableCaches && clipCache.ContainsKey(clipkey))
            {
              var prevNfp = clipCache[clipkey].nfpp;
              clipper.AddPaths(prevNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);
              startIndex = clipCache[clipkey].index;
              VerboseLog($"Retrieve {clipkey}:{startIndex} from {nameof(clipCache)}; populate {nameof(clipper)}");
            }

            if (!TryGetCombinedNfp(config.ClipperScale, placed, placements, part, clipper, startIndex, out combinedNfp))
            {
              VerboseLog($"{nameof(TryGetCombinedNfp)} clipper error.");
              error = true;
              continue;
            }

            if (EnableCaches)
            {
              VerboseLog($"Add {clipkey} to {nameof(clipCache)}");
              clipCache[clipkey] = new ClipCacheItem()
              {
                index = placed.Count - 1,
                nfpp = combinedNfp.Select(z => z.ToArray()).ToArray(),
              };
            }

            // console.log('save cache', placed.length - 1);

            List<NFP> finalNfp;
            InnerFlowResult clipperForDifferenceResult = TryGetDifferenceWithSheetPolygon(config, combinedNfp, part, clipperSheetNfp, out finalNfp);
            if (clipperForDifferenceResult == InnerFlowResult.Break)
            {
              break;
            }
            else if (clipperForDifferenceResult == InnerFlowResult.Continue)
            {
              continue;
            }

#if NCRUNCH
            try
            {
              var openScadBuilder = new StringBuilder();
              foreach (var item in finalNfp)
              {
                openScadBuilder.AppendLine(item.ToOpenScadPolygon());
              }

              var openScad = openScadBuilder.ToString();
            }
            catch (Exception)
            {
              // NOP
            }
#endif
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
                partpoints.AddPoint(new SvgPoint(part[m].X, part[m].Y));
              }

              partbounds = GeometryUtil.getPolygonBounds(partpoints);
            }
            else
            {
              allpoints = allpoints.GetHull();
            }

            VerboseLog($"Iterate nfps in differenceWithSheetPolygonNfp:");
            for (int j = 0; j < finalNfp.Count; j++)
            {
              VerboseLog($"  For j={j}");
              nf = finalNfp[j];

              VerboseLog($"evalnf {nf.Length}");
              for (int k = 0; k < nf.Length; k++)
              {
                VerboseLog($"    For k={k}");
                shiftvector = new PartPlacement(part)
                {
                  Id = part.Id,
                  X = nf[k].X - part[0].X,
                  Y = nf[k].Y - part[0].Y,
                  Source = part.Source,
                  Rotation = part.Rotation,
                };

                PolygonBounds rectbounds = null;
                if (config.PlacementType == PlacementTypeEnum.Gravity || config.PlacementType == PlacementTypeEnum.BoundingBox)
                {
                  NFP poly = new NFP();
                  poly.AddPoint(new SvgPoint(allbounds.X, allbounds.Y));
                  poly.AddPoint(new SvgPoint(allbounds.X + allbounds.Width, allbounds.Y));
                  poly.AddPoint(new SvgPoint(allbounds.X + allbounds.Width, allbounds.Y + allbounds.Height));
                  poly.AddPoint(new SvgPoint(allbounds.X, allbounds.Y + allbounds.Height));

                  poly.AddPoint(new SvgPoint(partbounds.X + shiftvector.X, partbounds.Y + shiftvector.Y));
                  poly.AddPoint(new SvgPoint(partbounds.X + partbounds.Width + shiftvector.X, partbounds.Y + shiftvector.Y));
                  poly.AddPoint(new SvgPoint(partbounds.X + partbounds.Width + shiftvector.X, partbounds.Y + partbounds.Height + shiftvector.Y));
                  poly.AddPoint(new SvgPoint(partbounds.X + shiftvector.X, partbounds.Y + partbounds.Height + shiftvector.Y));

                  rectbounds = GeometryUtil.getPolygonBounds(poly);

                  // weigh width more, to help compress in direction of gravity
                  if (config.PlacementType == PlacementTypeEnum.Gravity)
                  {
                    area = (rectbounds.Width * 3) + rectbounds.Height;
                  }
                  else
                  {
                    area = rectbounds.Width * rectbounds.Height;
                  }
                }
                else
                {
                  // must be convex hull
                  var localpoints = allpoints.Clone();

                  for (int m = 0; m < part.Length; m++)
                  {
                    localpoints.AddPoint(new SvgPoint(part[m].X + shiftvector.X, part[m].Y + shiftvector.Y));
                  }

                  area = Math.Abs(GeometryUtil.polygonArea(localpoints.GetHull()));
                  shiftvector.Hull = localpoints.GetHull();
                  shiftvector.HullSheet = sheet.GetHull();
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
                  area -= merged.TotalLength * config.TimeRatio;
                }

                VerboseLog("evalmerge");
                if (minarea == null ||
                    area < minarea ||
                    (GeometryUtil._almostEqual(minarea, area) && (minx == null || shiftvector.X < minx)) ||
                    (GeometryUtil._almostEqual(minarea, area) && (minx != null && GeometryUtil._almostEqual(shiftvector.X, minx) && shiftvector.Y < miny)))
                {
                  VerboseLog($"evalmerge-entered minarea={minarea ?? -1:0.000000} x={shiftvector?.X ?? -1:0.000000} y={shiftvector?.Y ?? -1:0.000000}");
                  minarea = area;

                  minwidth = rectbounds != null ? rectbounds.Width : 0;
                  position = shiftvector;
                  if (minx == null || shiftvector.X < minx)
                  {
                    minx = shiftvector.X;
                  }

                  if (miny == null || shiftvector.Y < miny)
                  {
                    miny = shiftvector.Y;
                  }

                  if (config.MergeLines)
                  {
                    position.MergedLength = merged.TotalLength;
                    position.MergedSegments = merged.Segments;
                  }

                  VerboseLog($"evalmerge-exit minarea={minarea ?? -1:0.000000} x={shiftvector?.X ?? -1:0.000000} y={shiftvector?.Y ?? -1:0.000000}");
                }
              }
            }

            if (position != null)
            {
              VerboseLog($"Add part {part}");
              placed.Add(part);
              placements.Add(position);
              if (position.MergedLength.HasValue)
              {
                totalMerged += position.MergedLength.Value;
              }
            }
            else if (part.IsPriority)
            {
              VerboseLog($"Could not place {part}. As it's Priority skip to next part.");
              break;
            }

            // send placement progress signal
            var placednum = placed.Count;
            for (int j = 0; j < allPlacements.Count; j++)
            {
              placednum += allPlacements[j].PartPlacements.Count;
            }
          }
          else
          {
            VerboseLog($"Could not place {part.ToShortString()} even on empty {sheet.ToShortString()}.");
          }
        }

        VerboseLog("All parts processed for current sheet.");
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
          VerboseLog($"Requeue {sheet.ToShortString()} for reuse.");
          unusedSheets.Push(sheet);
        }
        else
        {
          VerboseLog($"No need to requeue {sheet.ToShortString()}.");
        }

        while (requeue.Count > 0)
        {
          VerboseLog($"Reinstate {sheet.ToShortString()} for reuse.");
          unusedSheets.Push(requeue.Dequeue());
        }

        if (placements != null && placements.Count > 0)
        {
          VerboseLog($"Add {config.PlacementType} placement {sheet.ToShortString()}.");
          allPlacements.Add(new SheetPlacement(config.PlacementType, sheet, placements));
        }
        else
        {
          VerboseLog($"Something's gone wrong; break out of nest.");
          break; // something went wrong
        }
      }

      VerboseLog($"Nest complete in {sw.ElapsedMilliseconds}");
      return new NestResult(allPlacements, unplacedParts, totalMerged, config.PlacementType, sw.ElapsedMilliseconds);
    }

    private static InnerFlowResult TryGetDifferenceWithSheetPolygon(ISvgNestConfig config, List<List<IntPoint>> combinedNfp, NFP part, IntPoint[][] clipperSheetNfp, out List<NFP> differenceWithSheetPolygonNfp)
    {
      differenceWithSheetPolygonNfp = new List<NFP>();

      List<List<IntPoint>> differenceWithSheetPolygonNfpPoints = new List<List<IntPoint>>();
      var clipperForDifference = new Clipper();

      VerboseLog($"Add clip {nameof(combinedNfp)} to {nameof(clipperForDifference)}");
      clipperForDifference.AddPaths(combinedNfp, PolyType.ptClip, true);

      VerboseLog($"Add subject {nameof(clipperSheetNfp)} to {nameof(clipperForDifference)}");
      clipperForDifference.AddPaths(clipperSheetNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

      if (!clipperForDifference.Execute(ClipType.ctDifference, differenceWithSheetPolygonNfpPoints, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero))
      {
        VerboseLog("Clipper execute failed; move on to next part.");
        return InnerFlowResult.Continue;
      }
      else
      {
        VerboseLog($"{nameof(clipperForDifference)} execute => {nameof(differenceWithSheetPolygonNfpPoints)}");
      }

      if (differenceWithSheetPolygonNfpPoints == null || differenceWithSheetPolygonNfpPoints.Count == 0)
      {
        if (part.IsPriority)
        {
          VerboseLog("Could not place part. As it's Priority add another sheet.");
          return InnerFlowResult.Break; /* However that means we'll leave additional space on the first sheet though that won't get used again
                          as everything remaining will be fit to the consequent sheet? */
        }

        VerboseLog("Could not place part. As it's not Priority move on to next part.");
        return InnerFlowResult.Continue; // Part can't be fitted but it wasn't a primary, so move on to the next part
      }

      for (int j = 0; j < differenceWithSheetPolygonNfpPoints.Count; j++)
      {
        // back to normal scale
        differenceWithSheetPolygonNfp.Add(differenceWithSheetPolygonNfpPoints[j].ToArray().ToNestCoordinates(config.ClipperScale));
      }

      return InnerFlowResult.Success;
    }

    /// <summary>
    /// Starting from startIndex add parts placed to generate a combined NFP.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="placed"></param>
    /// <param name="placements"></param>
    /// <param name="part"></param>
    /// <param name="clipper"></param>
    /// <param name="startIndex"></param>
    /// <param name="combinedNfp"></param>
    /// <returns></returns>
    private bool TryGetCombinedNfp(double clipperScale, List<INfp> placed, List<IPartPlacement> placements, NFP part, Clipper clipper, int startIndex, out List<List<IntPoint>> combinedNfp)
    {
      combinedNfp = new List<List<IntPoint>>();

      if (placed.Count != placements.Count)
      {
        throw new InvalidOperationException();
      }
      else
      {
        foreach (var p in placements)
        {
          if (p.Part.X != part.X)
          {
            throw new InvalidOperationException();
          }

          if (p.Part.Y != part.Y)
          {
            throw new InvalidOperationException();
          }

          if (p.Part.Children.Count != part.Children.Count)
          {
            VerboseLog($"Cannot substitute {nameof(placements)}.Part for {nameof(placed)}");
          }
        }
      }

      for (int j = startIndex; j < placed.Count; j++)
      {
        var outerNfp = GetOuterNfp(placed[j], part, 0);

        if (outerNfp == null)
        {
          VerboseLog("Minkowski difference failed: very rare but could happen. . .");
          return false;
        }

        // shift to placed location
        for (int m = 0; m < outerNfp.Length; m++)
        {
          outerNfp[m].X += placements[j].X;
          outerNfp[m].Y += placements[j].Y;
        }

        if (outerNfp.Children != null && outerNfp.Children.Count > 0)
        {
          for (int n = 0; n < outerNfp.Children.Count; n++)
          {
            for (var o = 0; o < outerNfp.Children[n].Length; o++)
            {
              outerNfp.Children[n][o].X += placements[j].X;
              outerNfp.Children[n][o].Y += placements[j].Y;
            }
          }
        }

        var clipperNfp = NfpToClipperCoordinates(outerNfp, clipperScale);

        VerboseLog($"Add {placed[j].ToShortString()} paths to {nameof(clipper)} ({placed[j].Name})");
        clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);
      }

      // TODO: a lot here to insert
      if (!clipper.Execute(ClipType.ctUnion, combinedNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
      {
        return false;
      }
      else
      {
        VerboseLog($"{nameof(clipper)} union executed => {nameof(combinedNfp)}");
      }

      return true;
    }

    private static void VerboseLog(string message)
    {
#if NCRUNCH
      Trace.WriteLine(message);
#endif
    }

    public static INfp ShiftPolygon(INfp p, IPartPlacement shift)
    {
      return ShiftPolygon(p, shift.X, shift.Y);
    }

    internal static INfp ShiftPolygon(INfp p, double x, double y)
    {
      NFP shifted = new NFP();
      shifted.Id = p.Id;
      shifted.Name = p.Name;
      shifted.PlacementOrder = p.PlacementOrder;
      shifted.Rotation = p.Rotation;
      shifted.Source = p.Source;
      shifted.StrictAngle = p.StrictAngle;
      for (var i = 0; i < p.Length; i++)
      {
        shifted.AddPoint(new SvgPoint(p[i].X + x, p[i].Y + y) { Exact = p[i].Exact });
      }

      if (p.Children != null /*&& p.Children.Count*/)
      {
        for (int i = 0; i < p.Children.Count(); i++)
        {
          shifted.Children.Add(ShiftPolygon(p.Children[i], x, y));
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

      // var Ax2 = (A2.X - A1.X) * (A2.X - A1.X);
      //                var Ay2 = (A2.Y - A1.Y) * (A2.Y - A1.Y);

      // if (Ax2 + Ay2 < min2)
      //                {
      //                    continue;
      //                }

      // var angle = Math.atan2((A2.Y - A1.Y), (A2.X - A1.X));

      // var c = Math.cos(-angle);
      //                var s = Math.sin(-angle);

      // var c2 = Math.cos(angle);
      //                var s2 = Math.sin(angle);

      // var relA2 = { x: A2.X - A1.X, y: A2.Y - A1.Y};
      //            var rotA2x = relA2.X * c - relA2.Y * s;

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
      //                        var Bx2 = (B2.X - B1.X) * (B2.X - B1.X);
      //                        var By2 = (B2.Y - B1.Y) * (B2.Y - B1.Y);

      // if (Bx2 + By2 < min2)
      //                        {
      //                            continue;
      //                        }

      // // B relative to A1 (our point of rotation)
      //                        var relB1 = { x: B1.X - A1.X, y: B1.Y - A1.Y};
      //                    var relB2 = { x: B2.X - A1.X, y: B2.Y - A1.Y};

      // // rotate such that A1 and A2 are horizontal
      //                var rotB1 = { x: relB1.X* c -relB1.Y * s, y: relB1.X* s +relB1.Y * c};
      //            var rotB2 = { x: relB2.X* c -relB2.Y * s, y: relB2.X* s +relB2.Y * c};

      // if(!GeometryUtil.almostEqual(rotB1.Y, 0, tolerance) || !GeometryUtil.almostEqual(rotB2.Y, 0, tolerance)){
      // continue;
      // }

      // var min1 = Math.min(0, rotA2x);
      //        var max1 = Math.max(0, rotA2x);

      // var min2 = Math.min(rotB1.X, rotB2.X);
      //        var max2 = Math.max(rotB1.X, rotB2.X);

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

      // var C1 = { x: relC1.X + A1.X, y: relC1.Y + A1.Y };
      // var C2 = { x: relC2.X + A1.X, y: relC2.Y + A1.Y };

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

    private static INfp GetFrame(INfp a)
    {
      var bounds = GeometryUtil.getPolygonBounds(a);

      // expand bounds by 10%
      bounds.Width *= 1.1;
      bounds.Height *= 1.1;
      bounds.X -= 0.5 * (bounds.Width - (bounds.Width / 1.1));
      bounds.Y -= 0.5 * (bounds.Height - (bounds.Height / 1.1));

      var frame = new NFP(new List<INfp>() { a });
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X, bounds.Y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X + bounds.Width, bounds.Y));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X + bounds.Width, bounds.Y + bounds.Height));
      ((IHiddenNfp)frame).Push(new SvgPoint(bounds.X, bounds.Y + bounds.Height));

      frame.Source = a.Source;
      frame.Rotation = 0;

      return frame;
    }

    internal INfp[] GetInnerNfp(INfp a, INfp b, MinkowskiCache type, double clipperScale)
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
      var clipper = new Clipper();

      clipper.AddPaths(clipperHoles.Select(z => z.ToList()).ToList(), PolyType.ptClip, true);
      clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

      if (!clipper.Execute(ClipType.ctDifference, finalNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
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

    internal bool CanBePlaced(INfp sheet, INfp part, double clipperScale, out INfp[] sheetNfp)
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
        this.nest.ResponseProcessor(nestResult);
      }
    }

    private void ThenIterate(NfpPair processed, NFP[] parts, double clipperScale)
    {
      // returned data only contains outer nfp, we have to account for any holes separately in the synchronous portion
      // this is because the c++ addon which can process interior nfps cannot run in the worker thread
      var a = this.GetPart(processed.Asource, parts);
      var b = this.GetPart(processed.Bsource, parts);

      List<NFP> aChildren = new List<NFP>();

      if (a.Children != null)
      {
        for (int j = 0; j < a.Children.Count; j++)
        {
          aChildren.Add(a.Children[j].Rotate(processed.ARotation));
        }
      }

      if (aChildren.Count > 0)
      {
        var bRotated = b.Rotate(processed.BRotation);
        var bBounds = GeometryUtil.getPolygonBounds(bRotated);
        List<INfp> cnfp = new List<INfp>();

        for (int j = 0; j < aChildren.Count; j++)
        {
          var cbounds = GeometryUtil.getPolygonBounds(aChildren[j]);
          if (cbounds.Width > bBounds.Width && cbounds.Height > bBounds.Height)
          {
            var n = GetInnerNfp(aChildren[j], bRotated, MinkowskiCache.NoCache, clipperScale);
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

    internal INfp GetOuterNfp(INfp a, INfp b, MinkowskiCache type, bool inside = false) // todo:?inside def?
    {
      INfp[] nfp;

      var key = new DbCacheKey(a.Source, b.Source, a.Rotation, b.Rotation);

      lock (lockobj)
      {
        var doc = window.Find(key);
        if (doc != null)
        {
          return doc.First();
        }

        // not found in cache
        if (inside || (a.Children != null && a.Children.Count > 0))
        {
          nfp = ExecuteDllImportMinkowski(a, b, type);
        }
        else
        {
          NFP clipperNfp = minkowskiSumService.ClipperExecute(a, b, MinkowskiSumPick.Smallest);
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
          var doc2 = new DbCacheKey(a.Source, b.Source, a.Rotation, b.Rotation, nfp);
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

    private class MergedResult
    {
      public double TotalLength { get; set; }

      public object Segments { get; set; }
    }
  }
}
