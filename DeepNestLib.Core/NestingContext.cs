namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Xml.Linq;
  using DeepNestLib.IO;
  using DeepNestLib.Placement;

  public class NestingContext : INestingContext
  {
    private readonly IMessageService messageService;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly ISvgNestConfig config;
    private readonly NestState state;
    private readonly INestStateBackground stateBackground;
    private readonly INestStateNestingContext stateNestingContext;
    private volatile bool isStopped;
    private volatile SvgNest nest;

    public NestingContext(IMessageService messageService, IProgressDisplayer progressDisplayer, NestState state, ISvgNestConfig config)
    {
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
      this.State = state;
      this.config = config;
      this.state = state;
      this.stateNestingContext = state;
      this.stateBackground = state;
    }

    public ICollection<INfp> Polygons { get; } = new HashSet<INfp>();

    public IList<ISheet> Sheets { get; } = new List<ISheet>();

    public INestResult Current { get; private set; } = null;

    public SvgNest Nest
    {
      get => nest;
      private set => nest = value;
    }

    public INestState State { get; }

    /// <summary>
    /// Reinitializes the context and start a new nest.
    /// </summary>
    /// <returns>awaitable Task.</returns>
    public async Task StartNest()
    {
      this.progressDisplayer.DisplayTransientMessage($"Pre-processing. . .");
      this.ReorderSheets();
      this.InternalReset();
      this.Current = null;

      (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) nestItems = await Task.Run(
        () =>
        {
          return SvgNestInitializer.BuildNestItems(config, this.Polygons, this.Sheets, progressDisplayer);
        }).ConfigureAwait(false);

      this.Nest = new SvgNest(
        this.messageService,
        this.progressDisplayer,
        this.state,
        this.config,
        nestItems);
      this.isStopped = false;
    }

    public void ResumeNest()
    {
      this.isStopped = false;
    }

    public async Task NestIterate(ISvgNestConfig config)
    {
      try
      {
        if (!this.isStopped)
        {
          if (Nest.IsStopped)
          {
            this.StopNest();
          }
          else
          {
            Nest.LaunchWorkers(config, stateBackground);
          }
        }

        if (state.TopNestResults != null && State.TopNestResults.Count > 0)
        {
          var plcpr = State.TopNestResults.Top;

          if (Current == null || plcpr.FitnessTotal < Current.FitnessTotal)
          {
            AssignPlacement(plcpr);
          }
        }

        this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, config.PopulationSize);
        stateNestingContext.IncrementIterations();
      }
      catch (Exception ex)
      {
        if (!State.IsErrored)
        {
          state.SetIsErrored();
          this.messageService.DisplayMessage(ex);
        }

#if NCRUNCH
        throw;
#endif
      }
    }

    public void AssignPlacement(INestResult plcpr)
    {
      Current = plcpr;

      List<INfp> placed = new List<INfp>();
      foreach (var item in Polygons)
      {
        item.Sheet = null;
      }

      List<int> sheetsIds = new List<int>();

      foreach (var sheetPlacement in plcpr.UsedSheets)
      {
        var sheetid = sheetPlacement.SheetId;
        if (!sheetsIds.Contains(sheetid))
        {
          sheetsIds.Add(sheetid);
        }

        var sheet = Sheets.First(z => z.Id == sheetid);

        foreach (var partPlacement in sheetPlacement.PartPlacements)
        {
          var poly = Polygons.First(z => z.Id == partPlacement.Id);
          placed.Add(poly);
          poly.Sheet = sheet;
          poly.X = partPlacement.X + sheet.X;
          poly.Y = partPlacement.Y + sheet.Y;
          poly.PlacementOrder = sheetPlacement.PartPlacements.IndexOf(partPlacement);
        }
      }

      var ppps = Polygons.Where(z => !placed.Contains(z));
      foreach (var item in ppps)
      {
        item.X = -1000;
        item.Y = 0;
      }
    }

    public void ReorderSheets()
    {
      double x = 0;
      double y = 0;
      int gap = 10;
      for (int i = 0; i < Sheets.Count; i++)
      {
        Sheets[i].X = x;
        Sheets[i].Y = y;
        if (Sheets[i] is Sheet sheet)
        {
          x += sheet.WidthCalculated + gap;
        }
        else
        {
          var maxx = Sheets[i].Points.Max(z => z.X);
          var minx = Sheets[i].Points.Min(z => z.X);
          var w = maxx - minx;
          x += w + gap;
        }
      }
    }

    private void AddSheet(int width, int height, int src)
    {
      var tt = new RectangleSheet();
      tt.Name = "sheet" + (Sheets.Count + 1);
      Sheets.Add(tt);
      tt.Source = src;
      tt.Build(width, height);
      ReorderSheets();
    }

    public void LoadSampleData()
    {
      Console.WriteLine("Adding sheets..");
      for (int i = 0; i < 5; i++)
      {
        AddSheet(3000, 1500, 0);
      }

      Console.WriteLine("Adding parts..");
      int src1 = GetNextSource();
      for (int i = 0; i < 200; i++)
      {
        AddRectanglePart(src1, 250, 220);
      }
    }

    public int GetNextSource()
    {
      if (Polygons.Any())
      {
        return Polygons.Max(z => z.Source) + 1;
      }

      return 0;
    }

    public int GetNextSheetSource()
    {
      if (Sheets.Any())
      {
        return Sheets.Max(z => z.Source) + 1;
      }

      return 0;
    }

    public void AddRectanglePart(int src, int ww = 50, int hh = 80)
    {
      int xx = 0;
      int yy = 0;
      NoFitPolygon pl = new NoFitPolygon();

      Polygons.Add(pl);
      pl.Source = src;
      pl.AddPoint(new SvgPoint(xx, yy));
      pl.AddPoint(new SvgPoint(xx + ww, yy));
      pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
      pl.AddPoint(new SvgPoint(xx, yy + hh));
    }

    public void LoadXml(string v)
    {
      var d = XDocument.Load(v);
      var f = d.Descendants().First();
      var gap = int.Parse(f.Attribute("gap").Value);
      SvgNest.Config.Spacing = gap;

      foreach (var item in d.Descendants("sheet"))
      {
        int src = GetNextSheetSource();
        var cnt = int.Parse(item.Attribute("count").Value);
        var ww = int.Parse(item.Attribute("width").Value);
        var hh = int.Parse(item.Attribute("height").Value);

        for (int i = 0; i < cnt; i++)
        {
          AddSheet(ww, hh, src);
        }
      }

      foreach (var item in d.Descendants("part"))
      {
        var cnt = int.Parse(item.Attribute("count").Value);
        var path = item.Attribute("path").Value;
        IRawDetail r = null;
        if (path.ToLower().EndsWith("svg"))
        {
          r = SvgParser.LoadSvg(path);
        }
        else if (path.ToLower().EndsWith("dxf"))
        {
          r = DxfParser.LoadDxfFile(path).Result;
        }
        else
        {
          continue;
        }

        var src = GetNextSource();

        for (int i = 0; i < cnt; i++)
        {
          INfp loadedNfp;
          if (r.TryConvertToNfp(src, out loadedNfp))
          {
            this.Polygons.Add(loadedNfp);
          }
        }
      }
    }

    /// <summary>
    /// A full reset of the Context and all internals; Polygons and Sheets will need to be reinitialized.
    /// Caches remain intact.
    /// </summary>
    public void Reset()
    {
      this.Polygons.Clear();
      this.Sheets.Clear();
      InternalReset();
      progressDisplayer.UpdateNestsList();
    }

    /// <summary>
    /// An internal reset to facilitate restarting the nest only; won't clear down the Polygons or Sheets.
    /// </summary>
    private void InternalReset()
    {
      stateNestingContext.Reset();
      Current = null;
      this.Current = null;
    }

    public void StopNest()
    {
      System.Diagnostics.Debug.Print("NestingContext.StopNest()");
      this.isStopped = true;
      this.Nest?.Stop();
    }
  }
}
