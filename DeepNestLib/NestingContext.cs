namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Xml.Linq;
  using DeepNestLib.Placement;

  public class NestingContext : INestingContext
  {
    private readonly IMessageService messageService;
    private readonly IProgressDisplayer progressDisplayer;
    private volatile bool isStopped;
    private volatile SvgNest nest;

    public NestingContext(IMessageService messageService, IProgressDisplayer progressDisplayer)
      : this(messageService, progressDisplayer, NestState.Default)
    { }

    public NestingContext(IMessageService messageService, IProgressDisplayer progressDisplayer, NestState state)
    {
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
      this.State = state;
    }

    public bool IsErrored { get; private set; }

    public ICollection<INfp> Polygons { get; } = new HashSet<INfp>();

    public List<INfp> Sheets { get; private set; } = new List<INfp>();

    public INestResult Current { get; private set; } = null;

    public SvgNest Nest
    {
      get => nest;
      private set => nest = value;
    }

    public NestState State { get; }

    public void StartNest(IMinkowskiSumService minkowskiSumService)
    {
      this.ReorderSheets();
      this.InternalReset();
      this.Current = null;
      this.Nest = new SvgNest(
        this.messageService,
        this.progressDisplayer,
        () => this.IsErrored = true,
        minkowskiSumService,
        State);
      this.progressDisplayer.DisplayTransientMessage($"Pre-processing. . .");
      this.isStopped = false;
    }

    public void StartNest()
    {
      this.StartNest(MinkowskiSum.Default);
    }

    public void ResumeNest()
    {
      this.isStopped = false;
    }

    public void NestIterate(ISvgNestConfig config)
    {
      try
      {
        var lsheets = new List<INfp>();
        var lpoly = new List<INfp>();

        int id = 0;
        foreach (var item in Polygons)
        {
          item.Id = id;
          id++;
        }

        for (int i = 0; i < Sheets.Count; i++)
        {
          Sheets[i].Id = i;
        }

        foreach (var item in Polygons)
        {
          NFP clone = item.CloneExact();
          lpoly.Add(clone);
        }

        foreach (var item in Sheets)
        {
          var clone = new NFP();
          clone.Id = item.Id;
          clone.Source = item.Source;
          clone.ReplacePoints(item.Points.Select(z => new SvgPoint(z.X, z.Y) { Exact = z.Exact }));
          if (item.Children != null)
          {
            foreach (var citem in item.Children)
            {
              clone.Children.Add(new NFP());
              var l = clone.Children.Last();
              l.Id = citem.Id;
              l.Source = citem.Source;
              l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.X, z.Y) { Exact = z.Exact }));
            }
          }

          lsheets.Add(clone);
        }

        if (config.OffsetTreePhase)
        {
          var grps = lpoly.GroupBy(z => z.Source).ToArray();
          if (config.UseParallel)
          {
            Parallel.ForEach(grps, (item) =>
            {
              OffsetTreeReplace(config, item);
            });

          }
          else
          {
            foreach (var item in grps)
            {
              OffsetTreeReplace(config, item);
            }
          }

          foreach (var item in lsheets)
          {
            var gap = config.SheetSpacing - (config.Spacing / 2);
            var sheet = item;
            SvgNest.OffsetTree(ref sheet, -gap, config, true);
          }
        }

        List<NestItem> partsLocal = new List<NestItem>();
        var p1 = lpoly.GroupBy(z => z.Source).Select(z => new NestItem()
        {
          Polygon = z.First(),
          IsSheet = false,
          Quantity = z.Count(),
        });

        var p2 = lsheets.GroupBy(z => z.Source).Select(z => new NestItem()
        {
          Polygon = z.First(),
          IsSheet = true,
          Quantity = z.Count(),
        });

        partsLocal.AddRange(p1);
        partsLocal.AddRange(p2);
        int srcc = 0;
        foreach (var item in partsLocal)
        {
          item.Polygon.Source = srcc++;
        }

        if (Nest == null)
        {
          throw new FieldAccessException("Nest was null when it should not be possible.");
        }
        else
        {
          if (!this.isStopped)
          {
            Nest.launchWorkers(partsLocal.ToArray(), config);
          }

          if (Nest.TopNestResults != null && Nest.TopNestResults.Count > 0)
          {
            var plcpr = Nest.TopNestResults.Top;

            if (Current == null || plcpr.Fitness < Current.Fitness)
            {
              AssignPlacement(plcpr);
            }
          }

          State.IncrementIterations();
        }
      }
      catch (Exception ex)
      {
        if (!this.IsErrored)
        {
          this.IsErrored = true;
          this.messageService.DisplayMessage(ex);
        }

#if NCRUNCH
        throw;
#endif
      }
    }

    /// <summary>
    /// This is the only point where simplification feeds in to the process so use this in tests to apply config-simplifications to imports.
    /// </summary>
    /// <param name="config">Config to use when simplifying.</param>
    /// <param name="item">The item that will be modified.</param>
    private static void OffsetTreeReplace(ISvgNestConfig config, IGrouping<int, INfp> item)
    {
      // Don't see the reason to apply to all in the group because later we regroup and just use the first again.
      var target = item.First();
      SvgNest.OffsetTree(ref target, 0.5 * config.Spacing, config);
      foreach (var zitem in item)
      {
        zitem.ReplacePoints(item.First().Points);
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
          poly.Rotation = partPlacement.Rotation;
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
        if (Sheets[i] is Sheet)
        {
          var r = Sheets[i] as Sheet;
          x += r.Width + gap;
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

    private void AddSheet(int w, int h, int src)
    {
      var tt = new RectangleSheet();
      tt.Name = "sheet" + (Sheets.Count + 1);
      Sheets.Add(tt);

      tt.Source = src;
      tt.Height = h;
      tt.Width = w;
      tt.Rebuild();
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
      NFP pl = new NFP();

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
        RawDetail r = null;
        if (path.ToLower().EndsWith("svg"))
        {
          r = SvgParser.LoadSvg(path);
        }
        else if (path.ToLower().EndsWith("dxf"))
        {
          r = DxfParser.LoadDxfFile(path);
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
      State.Reset();
      this.nest?.TopNestResults.Clear();
      this.nest?.TopNestResults.Clear();
      Current = null;
      this.IsErrored = false;
      this.Current = null;
    }

    public void StopNest()
    {
      this.isStopped = true;
      this.Nest.Stop();
    }
  }
}
