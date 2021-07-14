namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Xml.Linq;
  using DeepNestLib.Placement;

  public class NestingContext
  {
    private readonly IMessageService messageService;
    private readonly IProgressDisplayer progressDisplayer;
    private int iterations = 0;
    Random random = new Random();
    private volatile bool isStopped;

    public NestingContext(IMessageService messageService, IProgressDisplayer progressDisplayer)
    {
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
    }

    public bool IsErrored { get; private set; }

    public ICollection<INfp> Polygons { get; } = new HashSet<INfp>();

    public List<INfp> Sheets { get; private set; } = new List<INfp>();

    public int PlacedPartsCount { get; private set; } = 0;

    public INestResult Current { get; private set; } = null;

    public SvgNest Nest
    {
      get;
      private set;
    }

    public int Iterations
    {
      get
      {
        return iterations;
      }
    }

    public void StartNest()
    {
      this.InternalReset();
      this.Current = null;
      this.Nest = new SvgNest(
        this.messageService,
        this.progressDisplayer,
        () => this.IsErrored = true);
      this.progressDisplayer.DisplayToolStripMessage($"Pre-processing. . .");
      this.isStopped = false;
    }

    public void NestIterate(ISvgNestConfig config)
    {
      try
      {
        List<NFP> lsheets = new List<NFP>();
        List<NFP> lpoly = new List<NFP>();

        //for (int i = 0; i < Polygons.Count; i++)
        //{
        //  Polygons[i].Id = i;
        //}

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
          NFP clone = new NFP();
          clone.Id = item.Id;
          clone.Source = item.Source;
          clone.ReplacePoints(item.Points.Select(z => new SvgPoint(z.x, z.y) { Exact = z.Exact }));
          if (item.Children != null)
          {
            foreach (var citem in item.Children)
            {
              clone.Children.Add(new NFP());
              var l = clone.Children.Last();
              l.Id = citem.Id;
              l.Source = citem.Source;
              l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.x, z.y) { Exact = z.Exact }));
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
              SvgNest.OffsetTree(item.First(), 0.5 * config.Spacing);
              foreach (var zitem in item)
              {
                zitem.ReplacePoints(item.First().Points);
              }

            });

          }
          else
          {
            foreach (var item in grps)
            {
              SvgNest.OffsetTree(item.First(), 0.5 * config.Spacing);
              foreach (var zitem in item)
              {
                zitem.ReplacePoints(item.First().Points);
              }
            }
          }

          foreach (var item in lsheets)
          {
            var gap = config.SheetSpacing - (config.Spacing / 2);
            SvgNest.OffsetTree(item, -gap, true);
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

          Interlocked.Increment(ref iterations);
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

    public void AssignPlacement(INestResult plcpr)
    {
      Current = plcpr;

      PlacedPartsCount = 0;
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
          PlacedPartsCount++;
          var poly = Polygons.First(z => z.Id == partPlacement.id);
          placed.Add(poly);
          poly.Sheet = sheet;
          poly.x = partPlacement.x + sheet.x;
          poly.y = partPlacement.y + sheet.y;
          poly.Rotation = partPlacement.rotation;
          poly.PlacementOrder = sheetPlacement.PartPlacements.IndexOf(partPlacement);
        }
      }

      var ppps = Polygons.Where(z => !placed.Contains(z));
      foreach (var item in ppps)
      {
        item.x = -1000;
        item.y = 0;
      }
    }

    public void ReorderSheets()
    {
      double x = 0;
      double y = 0;
      int gap = 10;
      for (int i = 0; i < Sheets.Count; i++)
      {
        Sheets[i].x = x;
        Sheets[i].y = y;
        if (Sheets[i] is Sheet)
        {
          var r = Sheets[i] as Sheet;
          x += r.Width + gap;
        }
        else
        {
          var maxx = Sheets[i].Points.Max(z => z.x);
          var minx = Sheets[i].Points.Min(z => z.x);
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
          if (r.TryGetNfp(src, out loadedNfp))
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
    }

    /// <summary>
    /// An internal reset to facilitate restarting the nest only; won't clear down the Polygons or Sheets.
    /// </summary>
    private void InternalReset()
    {
      SvgNest.Reset();
      PlacedPartsCount = 0;
      Current = null;
      Interlocked.Exchange(ref iterations, 0);
      this.IsErrored = false;
      this.Current = null;
      this.Nest = null;
    }

    public void StopNest()
    {
      this.isStopped = true;
      this.Nest.Stop();
    }
  }
}
