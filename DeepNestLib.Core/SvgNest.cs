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
      (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) nestItems)
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

    public bool IsStopped { get => isStopped; private set => isStopped = value; }

    private (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) NestItems { get; }

    private INestStateSvgNest State { get; }

    internal static INfp CleanPolygon2(INfp polygon)
    {
      var p = SvgToClipper(polygon);

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

    // offset tree recursively
    internal static void OffsetTree(ref INfp t, double offset, ISvgNestConfig config, bool? inside = null)
    {
      var simple = NfpSimplifier.SimplifyFunction(t, (inside == null) ? false : inside.Value, config);
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
    internal static INfp[] PolygonOffsetDeepNest(INfp polygon, double offset)
    {
      if (offset == 0 || GeometryUtil.AlmostEqual(offset, 0))
      {
        return new[] { polygon };
      }

      var p = SvgToClipper(polygon);

      var miterLimit = 4;
      var co = new ClipperLib.ClipperOffset(miterLimit, Config.CurveTolerance * Config.ClipperScale);
      co.AddPath(p.ToList(), ClipperLib.JoinType.jtMiter, ClipperLib.EndType.etClosedPolygon);

      var newpaths = new List<List<ClipperLib.IntPoint>>();
      co.Execute(ref newpaths, offset * Config.ClipperScale);

      var result = new List<NoFitPolygon>();
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

        State.IncrementPopulation();
        State.SetLastNestTime(payload.BackgroundTime);
        State.SetLastPlacementTime(payload.PlacePartTime);
        State.IncrementNestCount();
        State.IncrementPlacementTime(payload.PlacePartTime);
        State.IncrementNestTime(payload.BackgroundTime);

#if NCRUNCH
        Trace.WriteLine("payload.Index I don't think is being set right; double check before retrying threaded execution.");
#endif
        this.procreant.Population[payload.Index].Processing = false;
        this.procreant.Population[payload.Index].Fitness = payload.FitnessTotal;

        //int currentPlacements = 0;
        string suffix = string.Empty;
        if (!payload.IsValid || payload.UsedSheets.Count == 0)
        {
          this.State.IncrementRejected();
          suffix = " Rejected";
        }
        else
        {
          var result = this.State.TopNestResults.TryAdd(payload);
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
            else if (result == TryAddResult.Substitute)
            {
              suffix = "Substitute";
            }
            else
            {
              suffix = "Sub-optimal";
            }

            this.progressDisplayer.DisplayTransientMessage($"Nesting time = {payload.PlacePartTime}ms ({suffix})");
          }

          IncrementSecondaryProgressBar();
          if (this.State.TopNestResults.Top.TotalPlacedCount > 0)
          {
            progressDisplayer.DisplayProgress(State.Population, State.TopNestResults.Top);
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
            InitialiseAnotherGeneration();
          }

          var sheets = new List<ISheet>();
          var sid = 0;
          for (int i = 0; i < this.NestItems.SheetsLocal.Count(); i++)
          {
            var poly = this.NestItems.SheetsLocal[i].Polygon;
            for (int j = 0; j < this.NestItems.SheetsLocal[i].Quantity; j++)
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
              clone.Children = poly.Children.ToList();

              sheets.Add(new Sheet(clone, WithChildren.Included));
              sid++;
            }
          }

          this.progressDisplayer.DisplayTransientMessage("Executing Nest. . .");
          if (config.UseParallel)
          {
            var end1 = this.procreant.Population.Length / 3;
            var end2 = this.procreant.Population.Length * 2 / 3;
            var end3 = this.procreant.Population.Length;
            Parallel.Invoke(
              () => ProcessPopulation(0, end1, config, sheets.ToArray(), nestStateBackground),
              () => ProcessPopulation(end1, end2, config, sheets.ToArray(), nestStateBackground),
              () => ProcessPopulation(end2, this.procreant.Population.Length, config, sheets.ToArray(), nestStateBackground));
          }
          else
          {
            ProcessPopulation(0, this.procreant.Population.Length, config, sheets.ToArray(), nestStateBackground);
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
      finally
      {
        this.progressDisplayer.IsVisibleSecondaryProgressBar = false;
      }
    }

    // converts a polygon from normal double coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    private static List<ClipperLib.IntPoint> SvgToClipper(INfp polygon)
    {
      var d = DeepNestClipper.ScaleUpPath(polygon.Points, Config.ClipperScale);
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

      State.IncrementGenerations();
      State.ResetPopulation();
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

    private void ProcessPopulation(int start, int end, ISvgNestConfig config, ISheet[] sheets, INestStateBackground nestStateBackground)
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
        var individual = procreant.Population[i];
        if (!this.IsStopped && individual.IsPending)
        {
          individual.Processing = true;
          if (this.IsStopped)
          {
            this.ResponseProcessor(null);
          }
          else
          {
            var background = new Background(this.progressDisplayer, this, minkowskiSumService, nestStateBackground, config.UseDllImport);
            background.BackgroundStart(individual, sheets, config);
          }
        }
      }

      State.DecrementThreads();
    }
  }
}