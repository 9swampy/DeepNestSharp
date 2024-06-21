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

    public IList<ISheet> Sheets { get; } = new List<ISheet>(); // list of the original, non-offset sheets, populated by NestExecutionHelper. remove before flight

    public INestResult Current { get; private set; } = null;

    public SvgNest Nest
    {
      get => this.nest;
      private set => this.nest = value;
    }

    public INestState State { get; }

    /// <summary>
    /// Reinitializes the context and starts a new nest.
    /// </summary>
    /// <returns>awaitable Task.</returns>
    public async Task StartNest()
    {
      this.progressDisplayer.DisplayTransientMessage($"Pre-processing...");
      this.ReorderSheets();
      this.InternalReset();
      this.Current = null;

      (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal, List<NestItem<ISheet>> SheetsOriginal) nestItems = await Task.Run(
        () =>
        {
          return SvgNestInitializer.BuildNestItems(this.config, this.Polygons, this.Sheets, this.Sheets, this.progressDisplayer);
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
          if (this.Nest.IsStopped)
          {
            this.StopNest();
          }
          else
          {
            this.Nest.LaunchWorkers(config, this.stateBackground);
          }
        }

        if (this.state.TopNestResults != null && this.State.TopNestResults.Count > 0)
        {
          INestResult plcpr = this.State.TopNestResults.Top;

          if (this.Current == null || plcpr.Fitness < this.Current.Fitness)
          {
            this.AssignPlacement(plcpr);
          }
        }

        this.progressDisplayer.InitialiseLoopProgress(ProgressBar.Secondary, config.PopulationSize);
        this.stateNestingContext.IncrementIterations();
      }
      catch (Exception ex)
      {
        if (!this.State.IsErrored)
        {
          this.state.SetIsErrored();
          this.messageService.DisplayMessage(ex);
        }

#if NCRUNCH
        throw;
#endif
      }
    }

    public void AssignPlacement(INestResult plcpr)
    {
      this.Current = plcpr;

      List<INfp> placed = new List<INfp>();
      foreach (INfp item in this.Polygons)
      {
        item.Sheet = null;
      }

      List<int> sheetsIds = new List<int>();

      foreach (ISheetPlacement sheetPlacement in plcpr.UsedSheets)
      {
        var sheetid = sheetPlacement.SheetId;
        if (!sheetsIds.Contains(sheetid))
        {
          sheetsIds.Add(sheetid);
        }

        ISheet sheet = this.Sheets.First(z => z.Id == sheetid);

        foreach (IPartPlacement partPlacement in sheetPlacement.PartPlacements)
        {
          INfp poly = this.Polygons.First(z => z.Id == partPlacement.Id);
          placed.Add(poly);
          poly.Sheet = sheet;
          poly.X = partPlacement.X + sheet.X;
          poly.Y = partPlacement.Y + sheet.Y;
          poly.PlacementOrder = sheetPlacement.PartPlacements.IndexOf(partPlacement);
        }
      }

      IEnumerable<INfp> ppps = this.Polygons.Where(z => !placed.Contains(z));
      foreach (INfp item in ppps)
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
      for (int i = 0; i < this.Sheets.Count; i++)
      {
        this.Sheets[i].X = x;
        this.Sheets[i].Y = y;
        if (this.Sheets[i] is Sheet sheet)
        {
          x += sheet.WidthCalculated + gap;
        }
        else
        {
          var maxx = this.Sheets[i].Points.Max(z => z.X);
          var minx = this.Sheets[i].Points.Min(z => z.X);
          var w = maxx - minx;
          x += w + gap;
        }
      }
    }

    private void AddSheet(int width, int height, int src)
    {
      RectangleSheet tt = new RectangleSheet();
      tt.Name = "sheet" + (this.Sheets.Count + 1);
      this.Sheets.Add(tt);
      tt.Source = src;
      tt.Build(width, height);
      this.ReorderSheets();
    }

    public void LoadSampleData()
    {
      for (int i = 0; i < 5; i++)
      {
        this.AddSheet(3000, 1500, 0);
      }

      int src1 = this.GetNextSource();
      for (int i = 0; i < 200; i++)
      {
        this.AddRectanglePart(src1, 250, 220);
      }
    }

    public int GetNextSource()
    {
      if (this.Polygons.Any())
      {
        return this.Polygons.Max(z => z.Source) + 1;
      }

      return 0;
    }

    public int GetNextSheetSource()
    {
      if (this.Sheets.Any())
      {
        return this.Sheets.Max(z => z.Source) + 1;
      }

      return 0;
    }

    public void AddRectanglePart(int src, int ww = 50, int hh = 80)
    {
      int xx = 0;
      int yy = 0;
      NoFitPolygon pl = new NoFitPolygon();

      this.Polygons.Add(pl);
      pl.Source = src;
      pl.AddPoint(new SvgPoint(xx, yy));
      pl.AddPoint(new SvgPoint(xx + ww, yy));
      pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
      pl.AddPoint(new SvgPoint(xx, yy + hh));
    }

    public void LoadXml(string v)
    {
      XDocument d = XDocument.Load(v);
      XElement f = d.Descendants().First();
      var gap = int.Parse(f.Attribute("gap").Value);
      SvgNest.Config.Spacing = gap;

      foreach (XElement item in d.Descendants("sheet"))
      {
        int src = this.GetNextSheetSource();
        var cnt = int.Parse(item.Attribute("count").Value);
        var ww = int.Parse(item.Attribute("width").Value);
        var hh = int.Parse(item.Attribute("height").Value);

        for (int i = 0; i < cnt; i++)
        {
          this.AddSheet(ww, hh, src);
        }
      }

      foreach (XElement item in d.Descendants("part"))
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

        var src = this.GetNextSource();

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
      this.InternalReset();
      this.progressDisplayer.UpdateNestsList();
    }

    /// <summary>
    /// An internal reset to facilitate restarting the nest only; won't clear down the Polygons or Sheets.
    /// </summary>
    private void InternalReset()
    {
      this.stateNestingContext.Reset();
      this.Current = null;
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
