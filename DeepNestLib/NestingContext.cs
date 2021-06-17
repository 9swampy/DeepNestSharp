namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class NestingContext
    {
        public List<NFP> Polygons { get; private set; } = new List<NFP>();

        public List<NFP> Sheets { get; private set; } = new List<NFP>();

        public double MaterialUtilization { get; private set; } = 0;

        public int PlacedPartsCount { get; private set; } = 0;

        SheetPlacement current = null;

        public SheetPlacement Current { get { return this.current; } }

        public SvgNest Nest { get; private set; }

        public int Iterations { get; private set; } = 0;

        public void StartNest()
        {
            this.current = null;
            this.Nest = new SvgNest();
            Background.cacheProcess = new Dictionary<string, NFP[]>();
            Background.window = new windowUnk();
            Background.callCounter = 0;
            this.Iterations = 0;
        }

        bool offsetTreePhase = true;

        public void NestIterate()
        {
            List<NFP> lsheets = new List<NFP>();
            List<NFP> lpoly = new List<NFP>();

            for (int i = 0; i < this.Polygons.Count; i++)
            {
                this.Polygons[i].id = i;
            }

            for (int i = 0; i < this.Sheets.Count; i++)
            {
                this.Sheets[i].id = i;
            }

            foreach (var item in this.Polygons)
            {
                NFP clone = new NFP();
                clone.id = item.id;
                clone.source = item.source;
                clone.Points = item.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                if (item.children != null)
                {
                    clone.children = new List<NFP>();
                    foreach (var citem in item.children)
                    {
                        clone.children.Add(new NFP());
                        var l = clone.children.Last();
                        l.id = citem.id;
                        l.source = citem.source;
                        l.Points = citem.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                    }
                }

                lpoly.Add(clone);
            }

            foreach (var item in this.Sheets)
            {
                NFP clone = new NFP();
                clone.id = item.id;
                clone.source = item.source;
                clone.Points = item.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                if (item.children != null)
                {
                    clone.children = new List<NFP>();
                    foreach (var citem in item.children)
                    {
                        clone.children.Add(new NFP());
                        var l = clone.children.Last();
                        l.id = citem.id;
                        l.source = citem.source;
                        l.Points = citem.Points.Select(z => new SvgPoint(z.x, z.y) { exact = z.exact }).ToArray();
                    }
                }

                lsheets.Add(clone);
            }

            if (this.offsetTreePhase)
            {
                var grps = lpoly.GroupBy(z => z.source).ToArray();
                if (Background.UseParallel)
                {
                    Parallel.ForEach(grps, (item) =>
                    {
                        SvgNest.offsetTree(item.First(), 0.5 * SvgNest.Config.spacing, SvgNest.Config);
                        foreach (var zitem in item)
                        {
                            zitem.Points = item.First().Points.ToArray();
                        }
                    });
                }
                else
                {
                    foreach (var item in grps)
                    {
                        SvgNest.offsetTree(item.First(), 0.5 * SvgNest.Config.spacing, SvgNest.Config);
                        foreach (var zitem in item)
                        {
                            zitem.Points = item.First().Points.ToArray();
                        }
                    }
                }

                foreach (var item in lsheets)
                {
                    var gap = SvgNest.Config.sheetSpacing - SvgNest.Config.spacing / 2;
                    SvgNest.offsetTree(item, -gap, SvgNest.Config, true);
                }
            }

            List<NestItem> partsLocal = new List<NestItem>();
            var p1 = lpoly.GroupBy(z => z.source).Select(z => new NestItem()
            {
                Polygon = z.First(),
                IsSheet = false,
                Quanity = z.Count(),
            });

            var p2 = lsheets.GroupBy(z => z.source).Select(z => new NestItem()
            {
                Polygon = z.First(),
                IsSheet = true,
                Quanity = z.Count(),
            });

            partsLocal.AddRange(p1);
            partsLocal.AddRange(p2);
            int srcc = 0;
            foreach (var item in partsLocal)
            {
                item.Polygon.source = srcc++;
            }

            this.Nest.launchWorkers(partsLocal.ToArray());
            var plcpr = this.Nest.nests.First();

            if (this.current == null || plcpr.fitness < this.current.fitness)
            {
                this.AssignPlacement(plcpr);
            }

            this.Iterations++;
        }

        public void ExportSvg(string v)
        {
            SvgParser.Export(v, this.Polygons, this.Sheets);
        }

        public void AssignPlacement(SheetPlacement plcpr)
        {
            this.current = plcpr;
            double totalSheetsArea = 0;
            double totalPartsArea = 0;

            this.PlacedPartsCount = 0;
            List<NFP> placed = new List<NFP>();
            foreach (var item in this.Polygons)
            {
                item.sheet = null;
            }

            List<int> sheetsIds = new List<int>();

            foreach (var item in plcpr.placements)
            {
                foreach (var zitem in item)
                {
                    var sheetid = zitem.sheetId;
                    if (!sheetsIds.Contains(sheetid))
                    {
                        sheetsIds.Add(sheetid);
                    }

                    var sheet = this.Sheets.First(z => z.id == sheetid);
                    totalSheetsArea += GeometryUtil.polygonArea(sheet);

                    foreach (var ssitem in zitem.sheetplacements)
                    {
                        this.PlacedPartsCount++;
                        var poly = this.Polygons.First(z => z.id == ssitem.id);
                        totalPartsArea += GeometryUtil.polygonArea(poly);
                        placed.Add(poly);
                        poly.sheet = sheet;
                        poly.x = ssitem.x + sheet.x;
                        poly.y = ssitem.y + sheet.y;
                        poly.rotation = ssitem.rotation;
                    }
                }
            }

            var emptySheets = this.Sheets.Where(z => !sheetsIds.Contains(z.id)).ToArray();

            this.MaterialUtilization = Math.Abs(totalPartsArea / totalSheetsArea);

            var ppps = this.Polygons.Where(z => !placed.Contains(z));
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
            for (int i = 0; i < this.Sheets.Count; i++)
            {
                this.Sheets[i].x = x;
                this.Sheets[i].y = y;
                if (this.Sheets[i] is Sheet)
                {
                    var r = this.Sheets[i] as Sheet;
                    x += r.Width + gap;
                }
                else
                {
                    var maxx = this.Sheets[i].Points.Max(z => z.x);
                    var minx = this.Sheets[i].Points.Min(z => z.x);
                    var w = maxx - minx;
                    x += w + gap;
                }
            }
        }

        public void AddSheet(int w, int h, int src)
        {
            var tt = new RectangleSheet();
            tt.Name = "sheet" + (this.Sheets.Count + 1);
            this.Sheets.Add(tt);

            tt.source = src;
            tt.Height = h;
            tt.Width = w;
            tt.Rebuild();
            this.ReorderSheets();
        }

        Random r = new Random();

        public void LoadSampleData()
        {
            Console.WriteLine("Adding sheets..");

            // add sheets
            for (int i = 0; i < 5; i++)
            {
                this.AddSheet(3000, 1500, 0);
            }

            Console.WriteLine("Adding parts..");

            // add parts
            int src1 = this.GetNextSource();
            for (int i = 0; i < 200; i++)
            {
                this.AddRectanglePart(src1, 250, 220);
            }
        }

        public void LoadInputData(string path, int count)
        {
            var dir = new DirectoryInfo(path);
            foreach (var item in dir.GetFiles("*.svg"))
            {
                try
                {
                    var src = this.GetNextSource();
                    for (int i = 0; i < count; i++)
                    {
                        this.ImportFromRawDetail(SvgParser.LoadSvg(item.FullName), src);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading " + item.FullName + ". skip");
                }
            }
        }

        public NFP ImportFromRawDetail(RawDetail raw, int src)
        {
            NFP po = null;
            List<NFP> nfps = new List<NFP>();
            foreach (var item in raw.Outers)
            {
                var nn = new NFP();
                nfps.Add(nn);
                foreach (var pitem in item.Points)
                {
                    nn.AddPoint(new SvgPoint(pitem.X, pitem.Y));
                }
            }

            if (nfps.Any())
            {
                var tt = nfps.OrderByDescending(z => z.Area).First();
                po = tt;
                po.Name = raw.Name;

                foreach (var r in nfps)
                {
                    if (r == tt)
                    {
                        continue;
                    }

                    if (po.children == null)
                    {
                        po.children = new List<NFP>();
                    }

                    po.children.Add(r);
                }

                po.source = src;
                this.Polygons.Add(po);
            }

            return po;
        }

        public int GetNextSource()
        {
            if (this.Polygons.Any())
            {
                return this.Polygons.Max(z => z.source.Value) + 1;
            }

            return 0;
        }

        public int GetNextSheetSource()
        {
            if (this.Sheets.Any())
            {
                return this.Sheets.Max(z => z.source.Value) + 1;
            }

            return 0;
        }

        public void AddRectanglePart(int src, int ww = 50, int hh = 80)
        {
            int xx = 0;
            int yy = 0;
            NFP pl = new NFP();

            this.Polygons.Add(pl);
            pl.source = src;
            pl.Points = new SvgPoint[] { };
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
            SvgNest.Config.spacing = gap;

            foreach (var item in d.Descendants("sheet"))
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

            foreach (var item in d.Descendants("part"))
            {
                var cnt = int.Parse(item.Attribute("count").Value);
                var path = item.Attribute("path").Value;
                var r = SvgParser.LoadSvg(path);
                var src = this.GetNextSource();

                for (int i = 0; i < cnt; i++)
                {
                    this.ImportFromRawDetail(r, src);
                }
            }
        }
    }
}
