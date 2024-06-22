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

    private readonly IProgressDisplayer progressDisplayer;
    private readonly Procreant procreant;

    private volatile bool isStopped;

    public SvgNest(
      IMessageService messageService,
      IProgressDisplayer progressDisplayer,
      NestState nestState,
      ISvgNestConfig config,
      (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal, List<NestItem<ISheet>> OriginalSheetsLocal) nestItems)
    {
      this.State = nestState;
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
      this.minkowskiSumService = MinkowskiSum.CreateInstance(config, nestState);
      this.NestItems = nestItems;
      this.procreant = new Procreant(this.NestItems.PartsLocal, config, progressDisplayer);
    }

    public static ISvgNestConfig Config { get; }
#if NCRUNCH
    = new TestSvgNestConfig();
#else
    = new SvgNestConfig();
#endif

    public bool IsStopped { get => this.isStopped; private set => this.isStopped = value; }

    public (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal, List<NestItem<ISheet>> OriginalSheetsLocal) NestItems { get; }

    private INestStateSvgNest State { get; }

    internal static INfp CleanPolygon2(INfp polygon)
    {
      List<IntPoint> p = SvgToClipper(polygon);

      // remove self-intersections and find the biggest polygon that's left
      List<List<IntPoint>> simple = ClipperLib.Clipper.SimplifyPolygon(p, ClipperLib.PolyFillType.pftNonZero);

      if (simple == null || simple.Count == 0)
      {
        return null;
      }

      List<IntPoint> biggest = simple[0];
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
      List<IntPoint> clean = ClipperLib.Clipper.CleanPolygon(biggest, 0.01 * Config.CurveTolerance * Config.ClipperScale);

      if (clean == null || clean.Count == 0)
      {
        return null;
      }

      NoFitPolygon cleaned = ClipperToSvg(clean);

      // remove duplicate endpoints
      SvgPoint start = cleaned[0];
      SvgPoint end = cleaned[cleaned.Length - 1];
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

    // offset tree recursively
    internal static void OffsetTree(ref INfp t, double offset, ISvgNestConfig config, bool? inside = null)
    {
      INfp simple = NfpSimplifier.SimplifyFunction(t, (inside == null) ? false : inside.Value, config);
      INfp[] offsetpaths = new INfp[] { simple };
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
          INfp child = t.Children[i];
          OffsetTree(ref child, -offset, config, (inside == null) ? true : (!inside));
        }
      }
    }

    // use the clipper library to return an offset to the given polygon. Positive offset expands the polygon, negative contracts
    // note that this returns an array of polygons
    internal static INfp[] PolygonOffsetDeepNest(INfp polygon, double offset)
    {
      if (offset == 0 || GeometryUtil.AlmostEqual(offset, 0))
      {
        return new[] { polygon };
      }

      List<IntPoint> p = SvgToClipper(polygon);

      var miterLimit = 4;
      ClipperOffset co = new ClipperLib.ClipperOffset(miterLimit, Config.CurveTolerance * Config.ClipperScale);
      co.AddPath(p.ToList(), ClipperLib.JoinType.jtMiter, ClipperLib.EndType.etClosedPolygon);

      List<List<IntPoint>> newpaths = new List<List<ClipperLib.IntPoint>>();
      co.Execute(ref newpaths, offset * Config.ClipperScale);

      List<NoFitPolygon> result = new List<NoFitPolygon>();
      for (var i = 0; i < newpaths.Count; i++)
      {
        result.Add(ClipperToSvg(newpaths[i]));
      }

      return result.ToArray();
    }

    internal static NoFitPolygon ClipperToSvg(IList<IntPoint> polygon)
    {
      List<SvgPoint> ret = new List<SvgPoint>();

      for (var i = 0; i < polygon.Count; i++)
      {
        ret.Add(new SvgPoint(polygon[i].X / Config.ClipperScale, polygon[i].Y / Config.ClipperScale));
      }

      return new NoFitPolygon(ret);
    }

    internal void Stop()
    {
      System.Diagnostics.Debug.Print("SvgNest.Stop()");
      this.IsStopped = true;
    }

    internal void ResponseProcessor(NestResult payload)
    {
      try
      {
        if (this.procreant == null || payload == null)
        {
          // user might have quit while we're away
          return;
        }

        this.State.IncrementPopulation();
        this.State.SetLastNestTime(payload.BackgroundTime);
        this.State.SetLastPlacementTime(payload.PlacePartTime);
        this.State.IncrementNestCount();
        this.State.IncrementPlacementTime(payload.PlacePartTime);
        this.State.IncrementNestTime(payload.BackgroundTime);

#if NCRUNCH
        Trace.WriteLine("payload.Index I don't think is being set right; double check before retrying threaded execution.");
#endif
        this.procreant.Population[payload.Index].Processing = false;
        this.procreant.Population[payload.Index].Fitness = payload.Fitness;

        //int currentPlacements = 0;
        string suffix = string.Empty;
        if (!payload.IsValid || payload.UsedSheets.Count == 0)
        {
          this.State.IncrementRejected();
          suffix = " Rejected";
        }
        else
        {
          TryAddResult result = this.State.TopNestResults.TryAdd(payload);
          if (result == TryAddResult.Added)
          {
            if (this.State.TopNestResults.IndexOf(payload) < this.State.TopNestResults.EliteSurvivors)
            {
              suffix = "Elite";
              this.progressDisplayer?.UpdateNestsList();
            }
            else
            {
              suffix = "Top";
            }

            this.progressDisplayer.DisplayTransientMessage($"New top {this.State.TopNestResults.MaxCapacity} nest found: nesting time = {payload.PlacePartTime}ms");
          }
          else
          {
            if (result == TryAddResult.Duplicate)
            {
              suffix = "Duplicate";
            }
            else
            {
              suffix = "Sub-optimal";
            }

            this.progressDisplayer.DisplayTransientMessage($"Nesting time = {payload.PlacePartTime}ms ({suffix})");
          }

          this.IncrementSecondaryProgressBar();
          if (this.State.TopNestResults.Top.TotalPlacedCount > 0)
          {
            this.progressDisplayer.DisplayProgress(this.State.Population, this.State.TopNestResults.Top);
          }

          System.Diagnostics.Debug.Print($"Nest {payload.BackgroundTime}ms {suffix}");
        }
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
    internal void LaunchWorkers(ISvgNestConfig config, INestStateBackground nestStateBackground)
    {
      try
      {
        if (!this.IsStopped)
        {
          if (this.procreant.IsCurrentGenerationFinished)
          {
            this.InitialiseAnotherGeneration();
          }

          List<ISheet> sheets = new List<ISheet>();
          List<ISheet> originalSheets = new List<ISheet>();
          var sid = 0;
          for (int i = 0; i < this.NestItems.SheetsLocal.Count(); i++)
          {
            ISheet poly = this.NestItems.SheetsLocal[i].Polygon;
            ISheet originalPoly = this.NestItems.OriginalSheetsLocal[i].Polygon;
            for (int j = 0; j < this.NestItems.SheetsLocal[i].Quantity; j++)
            {
              ISheet clone;
              ISheet originalClone;
              if (poly is Sheet sheet && originalPoly is Sheet originalSheet)
              {
                clone = (ISheet)poly.CloneTree();
                originalClone = (ISheet)originalPoly.CloneTree();
              }
              else
              {
#if DEBUG || NCRUNCH
                throw new InvalidOperationException("Sheet should have been a sheet; why wasn't it?");
#endif
                clone = new Sheet(poly.CloneTree(), WithChildren.Excluded);
              }

              clone.Id = sid; // id is the unique id of all parts that will be nested, including cloned duplicates
              originalClone.Id = sid;
              clone.Source = poly.Source; // source is the id of each unique part from the main part list
              originalClone.Source = originalPoly.Source;
              clone.Children = poly.Children.ToList();
              originalClone.Children = originalPoly.Children.ToList();

              sheets.Add(new Sheet(clone, WithChildren.Included));
              originalSheets.Add(new Sheet(originalClone, WithChildren.Included));
              sid++;
            }
          }

          this.progressDisplayer.DisplayTransientMessage("Executing Nest...");
          if (config.UseParallel)
          {
            var end1 = this.procreant.Population.Length / 3;
            var end2 = this.procreant.Population.Length * 2 / 3;
            var end3 = this.procreant.Population.Length;
            Parallel.Invoke(
              () => this.ProcessPopulation(0, end1, config, sheets.ToArray(), originalSheets.ToArray(), nestStateBackground),
              () => this.ProcessPopulation(end1, end2, config, sheets.ToArray(), originalSheets.ToArray(), nestStateBackground),
              () => this.ProcessPopulation(end2, this.procreant.Population.Length, config, sheets.ToArray(), originalSheets.ToArray(), nestStateBackground));
          }
          else
          {
            this.ProcessPopulation(0, this.procreant.Population.Length, config, sheets.ToArray(), originalSheets.ToArray(), nestStateBackground);
          }
        }
      }
      catch (DllNotFoundException)
      {
        this.DisplayMinkowskiDllError();
        this.State.SetIsErrored();
      }
      catch (BadImageFormatException badImageEx)
      {
        if (badImageEx.StackTrace.Contains("Minkowski"))
        {
          this.DisplayMinkowskiDllError();
        }
        else
        {
          this.messageService.DisplayMessage(badImageEx);
        }

        this.State.SetIsErrored();
      }
      catch (Exception ex)
      {
        this.messageService.DisplayMessage(ex);
        this.State.SetIsErrored();
#if NCRUNCH
        throw;
#endif
      }
      finally
      {
        this.progressDisplayer.IsVisibleSecondaryProgressBar = false;
      }
    }

    // converts a polygon from normal double coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    private static List<ClipperLib.IntPoint> SvgToClipper(INfp polygon)
    {
      List<IntPoint> d = DeepNestClipper.ScaleUpPath(polygon.Points, Config.ClipperScale);
      return d;
    }

    private void IncrementSecondaryProgressBar()
    {
      if (!this.progressDisplayer.IsVisibleSecondaryProgressBar)
      {
        this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, Config.PopulationSize);
      }

      this.progressDisplayer.IncrementLoopProgress(ProgressBar.Secondary);
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
        PolygonTreeItem p = list[i];

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

    /// <summary>
    /// All individuals have been evaluated, start next generation
    /// </summary>
    private void InitialiseAnotherGeneration()
    {
      this.procreant.Generate();
#if !NCRUNCH
      if (this.procreant.Population.Length == 0)
      {
        this.Stop();
        this.messageService.DisplayMessageBox("Terminating the nest because we're just recalculating the same nests over and over again.", "Terminating Nest", MessageBoxIcon.Information);
      }
#endif

      this.State.IncrementGenerations();
      this.State.ResetPopulation();
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

    private void ProcessPopulation(int start, int end, ISvgNestConfig config, ISheet[] sheets, ISheet[] originalSheets, INestStateBackground nestStateBackground)
    {
      this.State.IncrementThreads();
      for (int i = start; i < end; i++)
      {
        if (this.IsStopped)
        {
          break;
        }

        // if(running < config.threads && !GA.population[i].processing && !GA.population[i].fitness){
        // only one background window now...
        PopulationItem individual = this.procreant.Population[i];
        if (!this.IsStopped && individual.IsPending)
        {
          individual.Processing = true;
          if (this.IsStopped)
          {
            this.ResponseProcessor(null);
          }
          else
          {
            Background background = new Background(this.progressDisplayer, this, this.minkowskiSumService, nestStateBackground, config.UseDllImport);
            background.BackgroundStart(individual, sheets, originalSheets, config);
          }
        }
      }

      this.State.DecrementThreads();
    }
  }
}