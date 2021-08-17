namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif
  using ClipperLib;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
  using static DeepNestLib.PlacementWorker;

  public class PartPlacementWorker : ITestPartPlacementWorker
  {
    private const bool EnableCaches = true;

    private readonly Dictionary<string, ClipCacheItem> clipCache;

    private IPlacementWorker placementWorker;
    private volatile object processPartLock = new object();
    
    [JsonConstructor]
    public PartPlacementWorker(Dictionary<string, ClipCacheItem> clipCache)
    {
      this.clipCache = clipCache;
    }

    public PartPlacementWorker(IPlacementWorker placementWorker, ISvgNestConfig config, INfp[] parts, List<IPartPlacement> placements, ISheet sheet, NfpHelper nfpHelper)
      : this(placementWorker, config, parts, placements, sheet, nfpHelper, new Dictionary<string, ClipCacheItem>())
    {
    }

    public PartPlacementWorker(IPlacementWorker placementWorker, ISvgNestConfig config, INfp[] parts, List<IPartPlacement> placements, ISheet sheet, NfpHelper nfpHelper, Dictionary<string, ClipCacheItem> clipCache)
    {
      this.clipCache = clipCache;
      this.placementWorker = placementWorker;
      this.Config = config;
      this.Sheet = sheet;
      this.NfpHelper = nfpHelper;
      this.Parts = parts.ToList();
      this.Placements = placements.ToList();
    }

    [JsonInclude]
    public List<IPartPlacement> Placements { get; private set; }

    // total length of merged lines
    public double MergedLength { get; private set; }

    [JsonInclude]
    public ISvgNestConfig Config { get; private set; }

    [JsonInclude]
    public IList<INfp> Parts { get; private set; }

    [JsonInclude]
    public ISheet Sheet { get; private set; }

    [JsonInclude]
    public NfpHelper NfpHelper { get; private set; }

    NfpHelper ITestPartPlacementWorker.NfpHelper { get => NfpHelper; set => NfpHelper = value; }

    [JsonInclude]
    public Dictionary<string, ClipCacheItem> ClipCache => this.clipCache;

    IPlacementWorker ITestPartPlacementWorker.PlacementWorker { get => this.placementWorker; set => this.placementWorker = value; }

    public string ToJson()
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SvgNestConfigJsonConverter());
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      options.Converters.Add(new ClipCacheItemJsonConverter());
      return System.Text.Json.JsonSerializer.Serialize(this, options);
    }

    [JsonInclude]
    public INfp InputPart { get; private set; }

    public InnerFlowResult ProcessPart(INfp inputPart)
    {
      lock (processPartLock)
      {
        var combinedNfp = new List<List<IntPoint>>();
        double? minwidth = null;
        double? minarea = null;

        InputPart = inputPart;
        var processedPart = inputPart;
        this.placementWorker.VerboseLog($"Process {inputPart.ToShortString()}.");

        // inner NFP
        INfp[] sheetNfp = null;
        if (Placements.Count == 0)
        {
          var canBePlaced = false;

          // try all possible rotations until it fits
          // (only do this for the first part of each sheet, to ensure that all parts that can be placed are, even if we have to to open a lot of sheets)
          for (int j = 0; j < this.Config.Rotations; j++)
          {
            if (this.CanBePlaced(Sheet, processedPart, this.Config.ClipperScale, out sheetNfp))
            {
              this.placementWorker.VerboseLog($"{processedPart.ToShortString()} could be placed if sheet empty (only do this for the first part on each sheet).");
              canBePlaced = true;
              break;
            }

            var r = processedPart.Rotate(360D / this.Config.Rotations);
            r.Rotation = processedPart.Rotation + (360D / this.Config.Rotations);
            r.Source = processedPart.Source;
            r.Id = processedPart.Id;

            // rotation is not in-place
            processedPart = r;
          }

          // part unplaceable, skip
          if (!canBePlaced)
          {
            this.placementWorker.VerboseLog($"{processedPart.ToShortString()} could not be placed even if sheet empty (only do this for the first part on each sheet).");
            return InnerFlowResult.Continue;
          }
        }

        PartPlacement position = null;
        if (Placements.Count == 0)
        {
          this.placementWorker.VerboseLog("First placement, put it on the bottom left corner. . .");

          // first placement, put it on the bottom left corner
          for (int j = 0; j < sheetNfp.Count(); j++)
          {
            for (int k = 0; k < sheetNfp[j].Length; k++)
            {
              if (position == null ||
                  ((sheetNfp[j][k].X - processedPart[0].X) < position.X) ||
                  (
                  GeometryUtil._almostEqual(sheetNfp[j][k].X - processedPart[0].X, position.X)
                  && ((sheetNfp[j][k].Y - processedPart[0].Y) < position.Y)))
              {
                position = new PartPlacement(processedPart)
                {
                  X = sheetNfp[j][k].X - processedPart[0].X,
                  Y = sheetNfp[j][k].Y - processedPart[0].Y,
                  Id = processedPart.Id,
                  Rotation = processedPart.Rotation,
                  Source = processedPart.Source,
                };
              }
            }
          }

          if (position == null)
          {
            var json = this.ToJson();
            throw new Exception("position null");

            // console.log(sheetNfp);
          }

          this.placementWorker.AddPlacement(inputPart, Placements, processedPart, position, this.Config.PlacementType, Sheet, MergedLength);
        }
        else if (this.CanBePlaced(Sheet, processedPart, this.Config.ClipperScale, out sheetNfp))
        {
          var clipper = new Clipper();
          string clipkey = "s:" + processedPart.Source + "r:" + processedPart.Rotation;
          var error = false;
          IntPoint[][] clipperSheetNfp = NfpHelper.InnerNfpToClipperCoordinates(sheetNfp, this.Config.ClipperScale);

          // check if stored in clip cache
          // var startindex = 0;
          var startIndex = 0;
          if (EnableCaches && this.ClipCache.ContainsKey(clipkey))
          {
            var prevNfp = this.ClipCache[clipkey].nfpp;
            clipper.AddPaths(prevNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);
            startIndex = this.ClipCache[clipkey].index;
            this.placementWorker.VerboseLog($"Retrieve {clipkey}:{startIndex} from {nameof(ClipCache)}; populate {nameof(clipper)}");
          }

          if (!this.TryGetCombinedNfp(this.Config.ClipperScale, Placements, processedPart, clipper, startIndex, out combinedNfp))
          {
            this.placementWorker.VerboseLog($"{nameof(TryGetCombinedNfp)} clipper error.");
            error = true;
            return InnerFlowResult.Continue;
          }

          if (EnableCaches)
          {
            this.placementWorker.VerboseLog($"Add {clipkey} to {nameof(ClipCache)}");
            this.ClipCache[clipkey] = new ClipCacheItem()
            {
              index = Placements.Count - 1,
              nfpp = combinedNfp.Select(z => z.ToArray()).ToArray(),
            };
          }

          // console.log('save cache', placed.length - 1);

          List<INfp> finalNfp;
          InnerFlowResult clipperForDifferenceResult = this.TryGetDifferenceWithSheetPolygon(this.Config, combinedNfp, processedPart, clipperSheetNfp, out finalNfp);
          if (clipperForDifferenceResult == InnerFlowResult.Break)
          {
            return InnerFlowResult.Break;
          }
          else if (clipperForDifferenceResult == InnerFlowResult.Continue)
          {
            return InnerFlowResult.Continue;
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
          INfp nf;
          double area;
          PartPlacement shiftvector = null;

          NFP allpoints = SheetPlacement.CombinedPoints(Placements);
          PolygonBounds allbounds = null;
          PolygonBounds partbounds = null;
          if (this.Config.PlacementType == PlacementTypeEnum.Gravity || this.Config.PlacementType == PlacementTypeEnum.BoundingBox)
          {
            allbounds = GeometryUtil.getPolygonBounds(allpoints);

            NFP partpoints = new NFP();
            for (int m = 0; m < processedPart.Length; m++)
            {
              partpoints.AddPoint(new SvgPoint(processedPart[m].X, processedPart[m].Y));
            }

            partbounds = GeometryUtil.getPolygonBounds(partpoints);
          }
          else
          {
            allpoints = allpoints.GetHull();
          }

          this.placementWorker.VerboseLog($"Iterate nfps in differenceWithSheetPolygonNfp:");
          for (int j = 0; j < finalNfp.Count; j++)
          {
            this.placementWorker.VerboseLog($"  For j={j}");
            nf = finalNfp[j];

            this.placementWorker.VerboseLog($"evalnf {nf.Length}");
            for (int k = 0; k < nf.Length; k++)
            {
              this.placementWorker.VerboseLog($"    For k={k}");
              shiftvector = new PartPlacement(processedPart)
              {
                Id = processedPart.Id,
                X = nf[k].X - processedPart[0].X,
                Y = nf[k].Y - processedPart[0].Y,
                Source = processedPart.Source,
                Rotation = processedPart.Rotation,
              };

              PolygonBounds rectbounds = null;
              if (this.Config.PlacementType == PlacementTypeEnum.Gravity || this.Config.PlacementType == PlacementTypeEnum.BoundingBox)
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
                if (this.Config.PlacementType == PlacementTypeEnum.Gravity)
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

                for (int m = 0; m < processedPart.Length; m++)
                {
                  localpoints.AddPoint(new SvgPoint(processedPart[m].X + shiftvector.X, processedPart[m].Y + shiftvector.Y));
                }

                area = Math.Abs(GeometryUtil.polygonArea(localpoints.GetHull()));
                shiftvector.Hull = localpoints.GetHull();
                shiftvector.HullSheet = Sheet.GetHull();
              }

              // console.timeEnd('evalbounds');
              // console.time('evalmerge');
              MergedResult merged = null;
              if (this.Config.MergeLines)
              {
                throw new NotImplementedException();

                // if lines can be merged, subtract savings from area calculation
                var shiftedpart = processedPart.Shift(shiftvector);
                List<INfp> shiftedplaced = new List<INfp>();

                for (int m = 0; m < Placements.Count; m++)
                {
                  shiftedplaced.Add(Placements[m].Part.Shift(Placements[m]));
                }

                // don't check small lines, cut off at about 1/2 in
                double minlength = 0.5 * this.Config.Scale;
                merged = CalculateMergedLength(shiftedplaced.ToArray(), shiftedpart, minlength, 0.1 * this.Config.CurveTolerance);
                area -= merged.TotalLength * this.Config.TimeRatio;
              }

              this.placementWorker.VerboseLog("evalmerge");
              if (minarea == null ||
                  area < minarea ||
                  (GeometryUtil._almostEqual(minarea, area) && (minx == null || shiftvector.X < minx)) ||
                  (GeometryUtil._almostEqual(minarea, area) && (minx != null && GeometryUtil._almostEqual(shiftvector.X, minx) && shiftvector.Y < miny)))
              {
                this.placementWorker.VerboseLog($"evalmerge-entered minarea={minarea ?? -1:0.000000} x={shiftvector?.X ?? -1:0.000000} y={shiftvector?.Y ?? -1:0.000000}");
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

                if (this.Config.MergeLines)
                {
                  position.MergedLength = merged.TotalLength;
                  position.MergedSegments = merged.Segments;
                }

                this.placementWorker.VerboseLog($"evalmerge-exit minarea={minarea ?? -1:0.000000} x={shiftvector?.X ?? -1:0.000000} y={shiftvector?.Y ?? -1:0.000000}");
              }
            }
          }

          if (position != null)
          {
            this.placementWorker.AddPlacement(inputPart, Placements, processedPart, position, this.Config.PlacementType, Sheet, MergedLength);
            if (position.MergedLength.HasValue)
            {
              var json = this.ToJson();
              this.MergedLength += position.MergedLength.Value;
            }
          }
          else if (processedPart.IsPriority)
          {
            this.placementWorker.VerboseLog($"Could not place {processedPart}. As it's Priority skip to next part.");
            return InnerFlowResult.Break;
          }
        }
        else
        {
          this.placementWorker.VerboseLog($"Could not place {processedPart.ToShortString()} even on empty {Sheet.ToShortString()}.");
        }

        return InnerFlowResult.Success;
      }
    }

    internal static PartPlacementWorker FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new ListJsonConverter<INfp>());
      options.Converters.Add(new IListInterfaceConverterFactory(typeof(NFP)));
      options.Converters.Add(new WindowUnkJsonConverter());
      options.Converters.Add(new SvgNestConfigJsonConverter());
      options.Converters.Add(new SheetPlacementJsonConverter());
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      options.Converters.Add(new ClipCacheItemJsonConverter());
      return System.Text.Json.JsonSerializer.Deserialize<PartPlacementWorker>(json, options);
    }

    internal bool CanBePlaced(INfp sheet, INfp part, double clipperScale, out INfp[] sheetNfp)
    {
      sheetNfp = NfpHelper.GetInnerNfp(sheet, part, MinkowskiCache.Cache, clipperScale);
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
    private bool TryGetCombinedNfp(double clipperScale, List<IPartPlacement> placements, INfp part, Clipper clipper, int startIndex, out List<List<IntPoint>> combinedNfp)
    {
      combinedNfp = new List<List<IntPoint>>();
      for (int j = startIndex; j < placements.Count; j++)
      {
        var outerNfp = NfpHelper.GetOuterNfp(placements[j].Part, part, MinkowskiCache.Cache);
        if (outerNfp == null)
        {
          this.placementWorker.VerboseLog("Minkowski difference failed: very rare but could happen. . .");
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

        var clipperNfp = NfpHelper.NfpToClipperCoordinates(outerNfp, clipperScale);
        this.placementWorker.VerboseLog($"Add {placements[j].Part.ToShortString()} paths to {nameof(clipper)} ({placements[j].Part.Name})");
        clipper.AddPaths(clipperNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);
      }

      // TODO: a lot here to insert
      if (!clipper.Execute(ClipType.ctUnion, combinedNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
      {
        return false;
      }
      else
      {
        this.placementWorker.VerboseLog($"{nameof(clipper)} union executed => {nameof(combinedNfp)}");
      }

      return true;
    }

    private InnerFlowResult TryGetDifferenceWithSheetPolygon(ISvgNestConfig config, List<List<IntPoint>> combinedNfp, INfp part, IntPoint[][] clipperSheetNfp, out List<INfp> differenceWithSheetPolygonNfp)
    {
      differenceWithSheetPolygonNfp = new List<INfp>();

      List<List<IntPoint>> differenceWithSheetPolygonNfpPoints = new List<List<IntPoint>>();
      var clipperForDifference = new Clipper();

      this.placementWorker.VerboseLog($"Add clip {nameof(combinedNfp)} to {nameof(clipperForDifference)}");
      clipperForDifference.AddPaths(combinedNfp, PolyType.ptClip, true);

      this.placementWorker.VerboseLog($"Add subject {nameof(clipperSheetNfp)} to {nameof(clipperForDifference)}");
      clipperForDifference.AddPaths(clipperSheetNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);

      if (!clipperForDifference.Execute(ClipType.ctDifference, differenceWithSheetPolygonNfpPoints, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero))
      {
        this.placementWorker.VerboseLog("Clipper execute failed; move on to next part.");
        return InnerFlowResult.Continue;
      }
      else
      {
        this.placementWorker.VerboseLog($"{nameof(clipperForDifference)} execute => {nameof(differenceWithSheetPolygonNfpPoints)}");
      }

      if (differenceWithSheetPolygonNfpPoints == null || differenceWithSheetPolygonNfpPoints.Count == 0)
      {
        if (part.IsPriority)
        {
          this.placementWorker.VerboseLog("Could not place part. As it's Priority add another sheet.");
          return InnerFlowResult.Break; /* However that means we'll leave additional space on the first sheet though that won't get used again
                          as everything remaining will be fit to the consequent sheet? */
        }

        this.placementWorker.VerboseLog("Could not place part. As it's not Priority move on to next part.");
        return InnerFlowResult.Continue; // Part can't be fitted but it wasn't a primary, so move on to the next part
      }

      for (int j = 0; j < differenceWithSheetPolygonNfpPoints.Count; j++)
      {
        // back to normal scale
        differenceWithSheetPolygonNfp.Add(differenceWithSheetPolygonNfpPoints[j].ToArray().ToNestCoordinates(config.ClipperScale));
      }

      return InnerFlowResult.Success;
    }

    // returns the square of the length of any merged lines
    // filter out any lines less than minlength long
    private static MergedResult CalculateMergedLength(INfp[] parts, INfp p, double minlength, double tolerance)
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

    private class MergedResult
    {
      public double TotalLength { get; set; }

      public object Segments { get; set; }
    }
  }

  public interface ITestPartPlacementWorker
  {
    NfpHelper NfpHelper { get; set; }

    IPlacementWorker PlacementWorker { get; set; }
  }
}