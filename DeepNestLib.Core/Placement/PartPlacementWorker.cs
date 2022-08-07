namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif
  using ClipperLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Geometry;
  using DeepNestLib.NestProject;
  using DeepNestLib.Placement;
#if NCRUNCH
  using Light.GuardClauses;
#endif

  public class PartPlacementWorker : ITestPartPlacementWorker
  {
    bool ITestPartPlacementWorker.EnableCaches { get; set; } = true;

    bool EnableCaches => ((ITestPartPlacementWorker)this).EnableCaches;

    private readonly IList<string> logList = new List<string>();
    private INestState state;
    private Dictionary<string, ClipCacheItem> clipCache;
    private IPlacementWorker placementWorker;
    private volatile object processPartLock = new object();
    private int exportIndex = 0;

    private IList<(string Filename, string Content)> exportList = new List<(string, string)>();

    [JsonConstructor]
    public PartPlacementWorker(Dictionary<string, ClipCacheItem> clipCache)
    {
      this.clipCache = clipCache;
    }

    public PartPlacementWorker(IPlacementWorker placementWorker, IPlacementConfig config, DeepNestGene gene, List<IPartPlacement> placements, ISheet sheet, NfpHelper nfpHelper, INestState state)
      : this(placementWorker, config, gene, placements, sheet, nfpHelper, new Dictionary<string, ClipCacheItem>(), state)
    {
    }

    public PartPlacementWorker(IPlacementWorker placementWorker, IPlacementConfig config, DeepNestGene gene, List<IPartPlacement> placements, ISheet sheet, NfpHelper nfpHelper, Dictionary<string, ClipCacheItem> clipCache, INestState state)
    {
      this.clipCache = clipCache;
      this.state = state;
      this.placementWorker = placementWorker;
      this.Config = config;
      this.Sheet = sheet;
      this.NfpHelper = nfpHelper;
      this.Gene = gene.ToList();
      this.Placements = placements;
    }

    [JsonIgnore]
    public bool ExportExecutions { get => this.Config?.ExportExecutions ?? false; private set => this.Config.ExportExecutions = value; }

    bool ITestPartPlacementWorker.ExportExecutions
    {
      get
      {
        return this.ExportExecutions;
      }

      set
      {
        this.ExportExecutions = value;
      }
    }

    [JsonInclude]
    public List<IPartPlacement> Placements { get; private set; }

    // total length of merged lines
    public double MergedLength { get; private set; }

    [JsonInclude]
    public IPlacementConfig Config { get; private set; }

    [JsonPropertyName("Parts")]
    [JsonInclude]
    public IList<Chromosome> Gene { get; private set; }

    [JsonInclude]
    public ISheet Sheet { get; private set; }

    [JsonInclude]
    public SheetNfp SheetNfp { get; private set; }

    [JsonInclude]
    public List<List<IntPoint>> CombinedNfp { get; private set; }

    [JsonInclude]
    public NfpHelper NfpHelper { get; private set; }

    NfpHelper ITestPartPlacementWorker.NfpHelper { get => this.NfpHelper; set => this.NfpHelper = value; }

    [JsonInclude]
    public Dictionary<string, ClipCacheItem> ClipCache { get => this.clipCache; set => this.clipCache = value; }

    IPlacementWorker ITestPartPlacementWorker.PlacementWorker { get => this.placementWorker; set => this.placementWorker = value; }

    [JsonInclude]
    public INfpCandidateList FinalNfp { get; private set; }

    [JsonInclude]
    public SheetPlacement SheetPlacement { get; private set; }

    [JsonInclude]
    public INfp InputPart { get; private set; }

    [JsonInclude]
    public IList<string> Log
    {
      get
      {
        return this.logList;
      }

      private set
      {
        this.logList.Clear();
        foreach (var log in value)
        {
          this.logList.Add(log);
        }
      }
    }

    INestState ITestPartPlacementWorker.State { get => this.state; set => this.state = value; }

    private static void LogCondition(string description, Func<bool> condition, Action<string> verboseLog)
    {
      try
      {
        verboseLog($"{description} : {condition()}");
      }
      catch (Exception ex)
      {
        verboseLog($"ERROR: {description} : {ex.Message}");
      }
    }

    public InnerFlowResult ProcessPart(INfp inputPart, int inputPartIndex)
    {
      try
      {
        lock (this.processPartLock)
        {
          this.SheetNfp = null;
          this.SheetPlacement = null;
          this.FinalNfp = null;
          this.CombinedNfp = null;
          this.InputPart = inputPart;
          this.logList.Clear();
          this.exportList.Clear();
          this.PrepExport(inputPartIndex, "In.json", () => this.ToJson(true));

          var processedPart = new NoFitPolygon(inputPart, WithChildren.Included) as INfp;
          this.VerboseLog($"ProcessPart {inputPart.ToShortString()}.");

          return this.Placements.Count == 0
            ? ProcessFirstPartOnSheet(inputPart, inputPartIndex, processedPart)
            : ProcessSecondaryPartOnSheet(inputPart, inputPartIndex, processedPart);
        }
      }
      catch (Exception ex)
      {
        VerboseLog(ex.Message);
        VerboseLog(ex.StackTrace);
        throw;
      }
      finally
      {
        if (this.ExportExecutions)
        {
          this.PrepExport(inputPartIndex, "LogOut", JsonSerializer.Serialize(this.logList));
          this.PersistExports();
        }
      }
    }

    private InnerFlowResult ProcessSecondaryPartOnSheet(INfp inputPart, int inputPartIndex, INfp processedPart)
    {
      this.VerboseLog("Already has a first placement.");
      this.VerboseLog($"Calculate placement #{this.Placements.Count} on SheetNfp");
      this.SheetNfp = this.InitialiseSheetNfp(processedPart);
      if (this.SheetNfp.CanAcceptPart && this.Placements.Count != 0)
      {
        this.VerboseLog($"{processedPart.ToShortString()} could be placed if sheet empty (but there's already {this.Placements.Count} placement[s] on the sheet - unconsidered).");
      }

      this.PrepExport(inputPartIndex, "SheetNfpItems.scad", () => this.SheetNfp.Items.ToOpenScadPolygon());
      this.PrepExport(inputPartIndex, "SheetNfpItemsFaux.dnsp", () => this.SheetNfp.Items.ToFauxSheetPlacement(this.Config.PlacementType, this.Sheet, this.Config.ClipperScale));

      if (this.SheetNfp.CanAcceptPart)
      {
        this.VerboseLog($"Placement #{this.Placements.Count + 1}. . .");
        string clipkey = "s:" + processedPart.Source + "r:" + processedPart.Rotation;

        List<List<IntPoint>> combinedNfp;
        if (!TryGetCombinedNfp(this.Placements, processedPart, clipkey, out combinedNfp))
        {
          this.VerboseLog($"{nameof(this.TryGetCombinedNfp)} clipper error.");
          return InnerFlowResult.Continue;
        }
        else
        {
          this.CombinedNfp = combinedNfp;
          this.PrepExport(inputPartIndex, "combinedNfp.scad", () => combinedNfp.ToOpenScadPolygon());
          this.PrepExport(inputPartIndex, "combinedNfpFaux.dnsp", () => combinedNfp.ToFauxSheetPlacement(this.Config.PlacementType, this.Sheet, this.Config.ClipperScale));
        }

        if (this.EnableCaches)
        {
          this.VerboseLog($"Add {clipkey} to {nameof(this.ClipCache)}");
          this.ClipCache[clipkey] = new ClipCacheItem()
          {
            index = this.Placements.Count - 1,
            nfpp = this.CombinedNfp.Select(z => z.ToArray()).ToArray(),
          };
        }

        // console.log('save cache', placed.length - 1);

        //Moved because I'm certain SheetNfp isn't accessed between where this was and here...
        var clipperSheetNfp = NfpHelper.InnerNfpToClipperCoordinates(this.SheetNfp.Items, this.Config.ClipperScale);
        this.PrepExport(inputPartIndex, "clipperSheetNfp.scad", () => clipperSheetNfp.ToOpenScadPolygon());

        List<INfp> finalNfp;
        InnerFlowResult clipperForDifferenceResult = this.TryGetDifferenceWithSheetPolygon(this.Config.ClipperScale, this.CombinedNfp, processedPart, clipperSheetNfp, out finalNfp);
        if (clipperForDifferenceResult == InnerFlowResult.Break)
        {
          this.VerboseLog("ProcessPart returns InnerFlowResult.Break");
          return InnerFlowResult.Break;
        }
        else if (clipperForDifferenceResult == InnerFlowResult.Continue)
        {
          this.VerboseLog("ProcessPart returns InnerFlowResult.Continue");
          return InnerFlowResult.Continue;
        }

        this.PrepExport(inputPartIndex, "finalNfp.scad", () => finalNfp.ToOpenScadPolygon());
        this.PrepExport(inputPartIndex, "finalNfpFaux.dnsp", () => finalNfp.ToFauxSheetPlacement(this.Config.PlacementType, this.Sheet, this.Config.ClipperScale));
        PartPlacement position = GetBestPosition(
                                          processedPart,
                                          finalNfp,
                                          this.VerboseLog,
                                          this.Config.PlacementType,
                                          SheetPlacement.CombinedPoints(this.Placements));
        if (position != null)
        {
          this.FinalNfp = new NfpCandidateList(finalNfp.ToArray(), this.Sheet, position.PlacedPart);
          this.SheetPlacement = this.AddPlacement(inputPart, processedPart, position, inputPartIndex);
          if (position.MergedLength.HasValue)
          {
            this.MergedLength += position.MergedLength.Value;
          }
        }
        else
        {
          this.VerboseLog($"Could not place {processedPart}.");
          return InnerFlowResult.Continue;
        }
      }
      else
      {
        this.VerboseLog($"Could not place {processedPart.ToShortString()} even on empty {this.Sheet.ToShortString()}.");
        return InnerFlowResult.Continue;
      }

      return InnerFlowResult.Success;
    }

    /// <summary>
    /// Try all possible rotations until it fits. Only do this for the first part of each sheet, 
    /// to ensure that all parts that can be placed are, even if we have to open a lot of sheets.
    /// </summary>
    /// <param name="inputPart">Part to fit.</param>
    /// <param name="inputPartIndex">Index of part to fit.</param>
    /// <param name="processedPart"></param>
    /// <returns><see cref="InnerFlowResult.Success"/> if part placed.
    /// <see cref="Continue"/> if part not placed.
    /// <see cref="Break"/> if process failed.
    /// </returns>
    private InnerFlowResult ProcessFirstPartOnSheet(INfp inputPart, int inputPartIndex, INfp processedPart)
    {
#if NCRUNCH
      this.Config.Rotations.MustBeGreaterThan(0, "Config.Rotations", "is this a test and you've passed in a Fake<Config>?");
#endif
      for (int j = 0; j < this.Config.Rotations; j++)
      {
        this.VerboseLog("Calculate first on SheetNfp");
        if (this.WouldFitOnRectangularSheet(processedPart))
        {
          this.SheetNfp = this.InitialiseSheetNfp(processedPart);
          if (this.SheetNfp.CanAcceptPart)
          {
            this.VerboseLog($"{processedPart.ToShortString()} could be placed if sheet empty (only do this for the first part on each sheet).");
            break;
          }
        }

        processedPart = processedPart.Rotate(360D / this.Config.Rotations);
      }

      if (this.SheetNfp != null && SheetNfp.CanAcceptPart)
      {
        this.VerboseLog("First placement, put it on the bottom left corner. . .");
        var candidatePoint = this.SheetNfp.GetCandidatePointClosestToOrigin();
        var position = new PartPlacement(processedPart)
        {
          X = candidatePoint.X - processedPart[0].X,
          Y = candidatePoint.Y - processedPart[0].Y,
          Id = processedPart.Id,
          Rotation = processedPart.Rotation,
          Source = processedPart.Source,
        };

        this.SheetPlacement = this.AddPlacement(inputPart, processedPart, position, inputPartIndex);
        return InnerFlowResult.Success;
      }
      else
      {
        this.VerboseLog($"{processedPart.ToShortString()} could not be placed even when sheet empty (only do this for the first part on each sheet).");
        this.PrepExport(inputPartIndex, $"Out-UnplaceableSheetNfp.dnsnfp", () => this.SheetNfp.ToJson());

        return InnerFlowResult.Continue;
      }
    }

    private PartPlacement GetBestPosition(INfp processedPart, List<INfp> finalNfp, Action<string> verboseLog, PlacementTypeEnum placementType, NoFitPolygon allPlacementsFlattened)
    {
      // choose placement that results in the smallest bounding box/hull etc
      // todo: generalize gravity direction
      /* var nf, area, shiftvector;*/
      double? minwidth = null;
      double? minarea = null;
      double? minx = null;
      double? miny = null;
      INfp nf;
      double area;

      NoFitPolygon allpoints = allPlacementsFlattened;
      PolygonBounds allbounds = null;
      PolygonBounds partbounds = null;
      if (placementType == PlacementTypeEnum.Gravity || placementType == PlacementTypeEnum.BoundingBox)
      {
        allbounds = GeometryUtil.GetPolygonBounds(allpoints);
        partbounds = GeometryUtil.GetPolygonBounds(processedPart.Points);
      }
      else
      {
        allpoints = allpoints.GetHull();
      }

      verboseLog($"Iterate nfps in differenceWithSheetPolygonNfp:");
      PartPlacement position = null;
      bool isRejected = false;
      for (int j = 0; j < finalNfp.Count; j++)
      {
        verboseLog($"  For j={j}");
        nf = finalNfp[j];

        verboseLog($"evalnf {nf.Length}");
        for (int k = 0; k < nf.Length; k++)
        {
          verboseLog($"    For k={k}");
          PartPlacement shiftvector = new PartPlacement(processedPart)
          {
            Id = processedPart.Id,
            X = nf[k].X - processedPart[0].X,
            Y = nf[k].Y - processedPart[0].Y,
            Source = processedPart.Source,
            Rotation = processedPart.Rotation,
          };

          if (Config.OverlapDetection && !IsPositionValid(shiftvector))
          {
            isRejected = true;
            verboseLog("Would overlap so skip.");
          }
          else
          {
            PolygonBounds rectbounds = null;
            if (placementType == PlacementTypeEnum.Gravity || placementType == PlacementTypeEnum.BoundingBox)
            {
              NoFitPolygon poly = new NoFitPolygon();
              poly.AddPoint(new SvgPoint(allbounds.X, allbounds.Y));
              poly.AddPoint(new SvgPoint(allbounds.X + allbounds.Width, allbounds.Y));
              poly.AddPoint(new SvgPoint(allbounds.X + allbounds.Width, allbounds.Y + allbounds.Height));
              poly.AddPoint(new SvgPoint(allbounds.X, allbounds.Y + allbounds.Height));

              poly.AddPoint(new SvgPoint(partbounds.X + shiftvector.X, partbounds.Y + shiftvector.Y));
              poly.AddPoint(new SvgPoint(partbounds.X + partbounds.Width + shiftvector.X, partbounds.Y + shiftvector.Y));
              poly.AddPoint(new SvgPoint(partbounds.X + partbounds.Width + shiftvector.X, partbounds.Y + partbounds.Height + shiftvector.Y));
              poly.AddPoint(new SvgPoint(partbounds.X + shiftvector.X, partbounds.Y + partbounds.Height + shiftvector.Y));

              rectbounds = GeometryUtil.GetPolygonBounds(poly);

              // weigh width more, to help compress in direction of gravity
              if (placementType == PlacementTypeEnum.Gravity)
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

              area = Math.Abs(GeometryUtil.PolygonArea(localpoints.GetHull()));
              //shiftvector.Hull = localpoints.GetHull();
              //shiftvector.HullSheet = sheet.GetHull();
            }

            // console.timeEnd('evalbounds');
            // console.time('evalmerge');

            verboseLog("evalmerge");
            if (minarea == null ||
                area < minarea ||
                (GeometryUtil.AlmostEqual(minarea, area) && (minx == null || shiftvector.X < minx)) ||
                (GeometryUtil.AlmostEqual(minarea, area) && minx != null && GeometryUtil.AlmostEqual(shiftvector.X, minx) && shiftvector.Y < miny))
            {
              LogCondition("minArea == null : {0}", () => minarea == null, verboseLog);
              if (minarea != null)
              {
                LogCondition($"area < minarea : {0}", () => area < minarea, verboseLog);
                LogCondition($"minx condition : {0}", () => GeometryUtil.AlmostEqual(minarea, area) && (minx == null || shiftvector.X < minx), verboseLog);
                LogCondition($"miny condition : {0}", () => GeometryUtil.AlmostEqual(minarea, area) && minx != null && GeometryUtil.AlmostEqual(shiftvector.X, minx) && shiftvector.Y < miny, verboseLog);
              }

              verboseLog($"evalmerge-entered minarea={minarea ?? -1:0.000000} x={shiftvector?.X ?? -1:0.000000} y={shiftvector?.Y ?? -1:0.000000}");
              minarea = area;

              minwidth = rectbounds == null ? 0 : rectbounds.Width;
              position = shiftvector;
              if (minx == null || shiftvector.X < minx)
              {
                minx = shiftvector.X;
              }

              if (miny == null || shiftvector.Y < miny)
              {
                miny = shiftvector.Y;
              }

              verboseLog($"evalmerge-exit minarea={minarea ?? -1:0.000000} x={shiftvector?.X ?? -1:0.000000} y={shiftvector?.Y ?? -1:0.000000}");
            }
          }
        }
      }

      if (position == null && isRejected && state is INestStateSvgNest nestStateSvgNest)
      {
        nestStateSvgNest.IncrementRejected();
      }

      return position;
    }

    private bool IsPositionValid(PartPlacement position)
    {
      var proposed = position.PlacedPart;
      foreach (var prior in this.Placements)
      {
        var shiftedPrior = prior.PlacedPart;
        if (proposed.Overlaps(shiftedPrior))
        {
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Pre-filter check that if the sheet was a full regular rectangle would the part even fit?
    /// </summary>
    /// <param name="processedPart"></param>
    /// <returns>.t if it would (so go ahead with expensive Clipper fitment.</returns>
    private bool WouldFitOnRectangularSheet(INfp processedPart)
    {
      return processedPart.WidthCalculated <= this.Sheet.WidthCalculated &&
                        processedPart.HeightCalculated <= this.Sheet.HeightCalculated;
    }

    private SheetNfp InitialiseSheetNfp(INfp processedPart)
    {
      var result = new SheetNfp(this.NfpHelper, this.Sheet, processedPart, this.Config.ClipperScale, this.Config.UseDllImport, this.VerboseLog);
      this.VerboseLog($"SheetNfp has {result.NumberOfNfps}.Items");
      return result;
    }

    public string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SvgNestConfigJsonConverter());
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      options.Converters.Add(new MinkowskiSumJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      options.Converters.Add(new ClipperLibIntPointJsonConverter());
      options.Converters.Add(new ClipCacheItemJsonConverter());
      options.WriteIndented = writeIndented;
      return System.Text.Json.JsonSerializer.Serialize(this, options);
    }

    private SheetPlacement AddPlacement(INfp inputPart, INfp processedPart, PartPlacement position, int inputPartIndex)
    {
      var result = this.placementWorker.AddPlacement(inputPart, this.Placements, processedPart, position, this.Config.PlacementType, this.Sheet, this.MergedLength);
      if (this.ExportExecutions)
      {
        this.PrepExport(inputPartIndex, "Out.json", () => this.ToJson(true));
        this.PrepExport(inputPartIndex, $"Out-Parts{result.PartPlacements.Count}.dnsp", () => result.ToJson(true));
        if (this.SheetNfp == null)
        {
          this.PrepExport(inputPartIndex, $"Out-SheetNfpNone.dnsnfp", string.Empty);
        }
        else
        {
          this.PrepExport(inputPartIndex, $"Out-SheetNfp.dnsnfp", this.SheetNfp.ToJson());
        }

        if (this.FinalNfp == null)
        {
          this.PrepExport(inputPartIndex, $"Out-FinalNfpNone.dnnfps", string.Empty);
        }
        else
        {
          this.PrepExport(inputPartIndex, $"Out-FinalNfp.dnnfps", this.FinalNfp.ToJson());
        }
      }

      return result;
    }

    /// <summary>
    /// Call this overload if the content already exists or cannot be defered.
    /// </summary>
    private void PrepExport(int inputPartIndex, string fileNameSuffix, string content)
    {
      try
      {
        if (this.state == null)
        {
          return;
        }

        var fileName = $"N{this.state.NestCount}-S{this.Sheet.Id}-{this.exportIndex}-P{inputPartIndex}-{fileNameSuffix}";
        this.exportList.Add((fileName, content));
        System.Diagnostics.Debug.Print($"Prep-Export {fileName}");
        this.exportIndex++;
      }
      catch (Exception)
      {
        // NOP
      }
    }

    /// <summary>
    /// Call this overload to only evaluate the content if ExportExecutions set.
    /// </summary>
    private void PrepExport(int inputPartIndex, string fileNameSuffix, Func<string> contentFunc)
    {
      try
      {
        if (this.ExportExecutions)
        {
          this.PrepExport(inputPartIndex, fileNameSuffix, contentFunc());
        }
      }
      catch (Exception ex)
      {
        VerboseLog(ex.Message);
        VerboseLog(ex.StackTrace);
      }
    }

    private void PersistExports()
    {
      if (!string.IsNullOrEmpty(this.Config.ExportExecutionPath))
      {
        var dirInfo = new DirectoryInfo(this.Config.ExportExecutionPath);
        if (dirInfo.Exists)
        {
          foreach (var export in this.exportList)
          {
            var filePath = Path.Combine(this.Config.ExportExecutionPath, export.Filename);
            System.Diagnostics.Debug.Print($"Export {filePath}");
            File.WriteAllText(filePath, export.Content);
          }
        }
        else
        {
          System.Diagnostics.Debug.Print($"Export path {this.Config.ExportExecutionPath} does not exist.");
        }
      }
    }

    internal static PartPlacementWorker FromJson(string json)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new ListJsonConverter<INfp>());
      options.Converters.Add(new IListInterfaceConverterFactory(typeof(NoFitPolygon)));
      options.Converters.Add(new WindowUnkJsonConverter());
      options.Converters.Add(new SvgNestConfigJsonConverter());
      options.Converters.Add(new SheetPlacementJsonConverter());
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      options.Converters.Add(new MinkowskiDictionaryJsonConverter());
      options.Converters.Add(new MinkowskiSumJsonConverter());
      options.Converters.Add(new PartPlacementJsonConverter());
      options.Converters.Add(new ClipperLibIntPointJsonConverter());
      options.Converters.Add(new ClipCacheItemJsonConverter());
      return System.Text.Json.JsonSerializer.Deserialize<PartPlacementWorker>(json, options);
    }

    /// <summary>
    /// Starting from startIndex add parts placed to generate a combined NFP.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="placed"></param>
    /// <param name="placements"></param>
    /// <param name="part"></param>
    /// <param name="clipper">Has been preloaded with the paths of the SheetNfp for the processing part, and up to startIndex if prior clip found in the cache.</param>
    /// <param name="startIndex">If cache enabled this will be the index to start adding previously loaded parts to clipper; otherwise it'll build all from 0</param>
    /// <param name="combinedNfp"></param>
    /// <returns></returns>
    private bool TryGetCombinedNfp(List<IPartPlacement> placements, INfp part, string clipkey, out List<List<IntPoint>> combinedNfp)
    {
      // check if stored in clip cache
      var startIndex = 0;
      var clipper = new Clipper();
      if (this.EnableCaches && this.ClipCache.ContainsKey(clipkey))
      {
        var prevNfp = this.ClipCache[clipkey].nfpp;
        clipper.AddPaths(prevNfp.Select(z => z.ToList()).ToList(), PolyType.ptSubject, true);
        startIndex = this.ClipCache[clipkey].index;
        this.VerboseLog($"Retrieve {clipkey}:{startIndex} from {nameof(this.ClipCache)}; populate {nameof(clipper)}");
      }

      this.VerboseLog("TryGetCombinedNfp");
      combinedNfp = new List<List<IntPoint>>();

      // For each part not already in clipper add the outerNfp of already placed part and the processing part.
      for (int j = startIndex; j < placements.Count; j++)
      {
        this.VerboseLog($"TryGetCombinedNfp(j={j})=>NfpHelper.GetOuterNfp");
        ((MinkowskiSum)((ITestNfpHelper)this.NfpHelper).MinkowskiSumService).VerboseLogAction = s => this.VerboseLog(s);
        var outerNfp = this.NfpHelper.GetOuterNfp(placements[j].Part, part, MinkowskiCache.Cache, this.Config.UseDllImport);
        ((MinkowskiSum)((ITestNfpHelper)this.NfpHelper).MinkowskiSumService).VerboseLogAction = s => { };
        this.VerboseLog($"NfpHelper.GetOuterNfp=>TryGetCombinedNfp(j={j})");
        if (outerNfp == null)
        {
          this.VerboseLog("Minkowski difference failed: very rare but could happen. . .");
          return false;
        }

        this.VerboseLog($"TryGetCombinedNfp(j={j})=>shift to placed location");
        for (int m = 0; m < outerNfp.Length; m++)
        {
          outerNfp[m].X += placements[j].X;
          outerNfp[m].Y += placements[j].Y;
        }

        if (outerNfp.Children != null && outerNfp.Children.Count > 0)
        {
          this.VerboseLog($"TryGetCombinedNfp(j={j})=>has children.");
          for (int n = 0; n < outerNfp.Children.Count; n++)
          {
            for (var o = 0; o < outerNfp.Children[n].Length; o++)
            {
              outerNfp.Children[n][o].X += placements[j].X;
              outerNfp.Children[n][o].Y += placements[j].Y;
            }
          }
        }

        var clipperNfp = NfpHelper.NfpToClipperCoordinates(outerNfp, this.Config.ClipperScale);
        this.VerboseLog($"Add {placements[j].Part.ToShortString()} paths to {nameof(clipper)} ({placements[j].Part.Name})");
        clipper.AddPaths(clipperNfp, PolyType.ptSubject, true);
      }

      // TODO: a lot here to insert
      if (!clipper.Execute(ClipType.ctUnion, combinedNfp, PolyFillType.pftNonZero, PolyFillType.pftNonZero))
      {
        this.VerboseLog($"{nameof(clipper)} union failed => {nameof(combinedNfp)}");
        return false;
      }
      else
      {
        this.VerboseLog($"{nameof(clipper)} union executed => {nameof(combinedNfp)}");
      }

      return true;
    }

    private void VerboseLog(string message)
    {
      this.logList.Add(message);
      this.placementWorker.VerboseLog(message);
    }

    private InnerFlowResult TryGetDifferenceWithSheetPolygon(double clipperScale, List<List<IntPoint>> combinedNfp, INfp partx, List<List<IntPoint>> clipperSheetNfp, out List<INfp> differenceWithSheetPolygonNfp)
    {
      differenceWithSheetPolygonNfp = new List<INfp>();

      List<List<IntPoint>> differenceWithSheetPolygonNfpPoints = new List<List<IntPoint>>();
      var clipperForDifference = new Clipper();

      this.VerboseLog($"Add clip {nameof(combinedNfp)} to {nameof(clipperForDifference)}");
      clipperForDifference.AddPaths(combinedNfp, PolyType.ptClip, true);

      this.VerboseLog($"Add subject {nameof(clipperSheetNfp)} to {nameof(clipperForDifference)}");
      clipperForDifference.AddPaths(clipperSheetNfp, PolyType.ptSubject, true);

      if (!clipperForDifference.Execute(ClipType.ctDifference, differenceWithSheetPolygonNfpPoints, PolyFillType.pftEvenOdd, PolyFillType.pftNonZero))
      {
        this.VerboseLog("Clipper execute failed; move on to next part.");
        return InnerFlowResult.Continue;
      }
      else
      {
        this.VerboseLog($"{nameof(clipperForDifference)} execute completed normally => {nameof(differenceWithSheetPolygonNfpPoints)}");
      }

      if (differenceWithSheetPolygonNfpPoints == null || differenceWithSheetPolygonNfpPoints.Count == 0)
      {
        this.VerboseLog("Could not place part. Move on to next part.");
        return InnerFlowResult.Continue; // Part can't be fitted, so move on to the next part
      }

      this.VerboseLog($"Found {differenceWithSheetPolygonNfpPoints.Count} difference Nfps.");
      for (int j = 0; j < differenceWithSheetPolygonNfpPoints.Count; j++)
      {
        // back to normal scale
        differenceWithSheetPolygonNfp.Add(differenceWithSheetPolygonNfpPoints[j].ToArray().ToNestCoordinates(clipperScale));
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
    bool EnableCaches { get; set; }

    NfpHelper NfpHelper { get; set; }

    IPlacementWorker PlacementWorker { get; set; }

    INestState State { get; set; }

    bool ExportExecutions { get; set; }
  }
}