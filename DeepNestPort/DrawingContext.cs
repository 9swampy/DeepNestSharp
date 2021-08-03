namespace DeepNestPort
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;
  using System.Windows.Forms;
  using DeepNestLib;

  public class DrawingContext
  {
    private Graphics gr;
    private double startx;
    private double starty;
    private double origsx;
    private double origsy;
    private bool isDrag = false;
    private PictureBox box;
    private double sx;
    private double sy;
    private double zoom = 1;
    private Bitmap bmp;
    private bool invertY = true;

    public bool FocusOnMove { get; set; } = true;

    public DrawingContext(PictureBox pb)
    {
      box = pb;

      bmp = new Bitmap(pb.Width, pb.Height);
      pb.SizeChanged += Pb_SizeChanged;
      gr = Graphics.FromImage(bmp);
      gr.SmoothingMode = SmoothingMode.AntiAlias;
      box.Image = bmp;

      pb.MouseDown += PictureBox1_MouseDown;
      pb.MouseUp += PictureBox1_MouseUp;
      pb.MouseMove += Pb_MouseMove;
      sx = box.Width / 2;
      sy = -box.Height / 2;
      pb.MouseWheel += Pb_MouseWheel;
    }

    private GraphicsPath GetGraphicsPath(INfp nfp)
    {
      GraphicsPath gp = new GraphicsPath();
      gp.AddPolygon(nfp.Points.Select(z => Transform(z.X, z.Y)).ToArray());
      if (nfp.Children != null)
      {
        foreach (var item in nfp.Children)
        {
          gp.AddPolygon(item.Points.Select(z => Transform(z.X, z.Y)).ToArray());
        }
      }

      return gp;
    }

    private GraphicsPath GetGraphicsPath(RawDetail det)
    {
      GraphicsPath gp = new GraphicsPath();
      foreach (var item in det.Outers)
      {
        gp.AddPolygon(item.Points.Select(z => Transform(z)).ToArray());
      }

      return gp;
    }

    public double GetLabelHeight()
    {
      return SystemFonts.DefaultFont.GetHeight();
    }

    public SizeF DrawLabel(string text, Brush fontBrush, Color backColor, int x, int y, int opacity = 128)
    {
      var ms = this.gr.MeasureString(text, SystemFonts.DefaultFont);
      this.gr.FillRectangle(new SolidBrush(Color.FromArgb(opacity, backColor)), x, y, ms.Width, ms.Height);
      this.gr.DrawString(text, SystemFonts.DefaultFont, fontBrush, x, y);
      return ms;
    }

    public GraphicsPath Draw(INfp nfp, Pen pen = null, Brush brush = null)
    {
      var gp = GetGraphicsPath(nfp);
      if (brush != null)
      {
        this.gr.FillPath(brush, gp);
      }

      if (pen != null)
      {
        this.gr.DrawPath(pen, gp);
      }

      return gp;
    }

    public GraphicsPath Draw(RawDetail det, Pen pen = null, Brush brush = null)
    {
      var gp = GetGraphicsPath(det);
      if (brush != null)
        gr.FillPath(brush, gp);
      if (pen != null)
        gr.DrawPath(pen, gp);
      return gp;
    }

    private void Pb_MouseWheel(object sender, MouseEventArgs e)
    {
      double zold = zoom;
      if (e.Delta > 0) { zoom *= 1.5f; ; }
      else { zoom *= 0.5f; }
      if (zoom < 0.08) { zoom = 0.08f; }
      if (zoom > 1000) { zoom = 1000f; }

      var pos = box.PointToClient(Cursor.Position);

      sx = -((pos.X / zold) - sx - (pos.X / zoom));
      sy = ((pos.Y / zold) + sy - (pos.Y / zoom));
    }

    private void Pb_MouseMove(object sender, MouseEventArgs e)
    {
      if (!FocusOnMove) return;
      box.Focus();
    }

    private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
    {
      isDrag = false;

      var p = box.PointToClient(Cursor.Position);
      var pos = box.PointToClient(Cursor.Position);
      var posx = ((pos.X / zoom) - sx);
      var posy = ((-pos.Y / zoom) - sy);
    }

    private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
    {
      var pos = box.PointToClient(Cursor.Position);
      var p = Transform(pos);

      if (e.Button == MouseButtons.Right)
      {
        isDrag = true;
        startx = pos.X;
        starty = pos.Y;
        origsx = sx;
        origsy = sy;
      }
    }

    internal void Clear(Color color)
    {
      gr.Clear(color);
    }

    internal void Reset()
    {
      gr.ResetTransform();
    }

    public virtual PointF Transform(PointF p1)
    {
      return Transform(p1.X, p1.Y);
    }

    public virtual PointF Transform(double x, double y)
    {
      return Transform((float)x, (float)y);
    }

    public virtual PointF Transform(float x, float y)
    {
      return new PointF((float)((x + sx) * zoom), (float)((invertY ? (-1) : 1) * (y + sy) * zoom));
    }

    private void Pb_SizeChanged(object sender, EventArgs e)
    {
      bmp = new Bitmap(box.Width, box.Height);
      gr = Graphics.FromImage(bmp);

      box.Image = bmp;
    }

    public PointF GetPos()
    {
      var pos = box.PointToClient(Cursor.Position);
      var posx = (float)((pos.X / zoom) - sx);
      var posy = (float)((-pos.Y / zoom) - sy);

      return new PointF(posx, posy);
    }

    public void Update()
    {
      if (isDrag)
      {
        var p = box.PointToClient(Cursor.Position);

        sx = origsx + ((p.X - startx) / zoom);
        sy = origsy + (-(p.Y - starty) / zoom);
      }
    }

    public void Setup()
    {
      box.Invalidate();
    }

    public void FitToPoints(PointF[] points, int gap = 0)
    {
      var maxx = points.Max(z => z.X) + gap;
      var minx = points.Min(z => z.X) - gap;
      var maxy = points.Max(z => z.Y) + gap;
      var miny = points.Min(z => z.Y) - gap;

      var w = box.Width;
      var h = box.Height;

      var dx = maxx - minx;
      var kx = w / dx;
      var dy = maxy - miny;
      var ky = h / dy;

      var oz = zoom;
      var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
      var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
      zoom = kx;
      if (sz1.Width > w || sz1.Height > h) zoom = ky;

      var x = (dx / 2) + minx;
      var y = (dy / 2) + miny;

      sx = (((w / 2f) / zoom) - x);
      sy = -(((h / 2f) / zoom) + y);

      var test = Transform(new PointF(x, y));
    }

    public void RenderSheetToClipboard(Sheet sh, ICollection<INfp> polygons, ICollection<INfp> sheets)
    {
      this.sx = (double)sh.X;
      this.sy = (double)sh.Y;
      this.invertY = false;
      var bb = new Bitmap(1 + (int)sh.Width, 1 + (int)sh.Height);
      var gr = Graphics.FromImage(bb);
      var tmpbmp = this.bmp;
      this.gr = gr;
      this.bmp = bb;
      RenderSheet(polygons, sheets);
      this.gr = gr;
      this.bmp = tmpbmp;
      this.invertY = true;
      Clipboard.SetImage(bb);
    }

    private void RenderSheet(ICollection<INfp> polygons, ICollection<INfp> sheets)
    {
      this.gr.SmoothingMode = SmoothingMode.AntiAlias;
      this.Clear(Color.White);
      this.Reset();
      foreach (var item in polygons.Union(sheets))
      {
        if (!(item is Sheet))
        {
          if (!item.Fitted) continue;
        }

        GraphicsPath path = new GraphicsPath();
        if (item.Points != null && item.Points.Any())
        {
          //rotate first;
          var m = new Matrix();
          m.Translate((float)item.X, (float)item.Y);
          m.Rotate((float)item.Rotation);

          var pnts = item.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
          m.TransformPoints(pnts);

          path.AddPolygon(pnts.Select(z => this.Transform(z)).ToArray());

          if (item.Children != null)
          {
            foreach (var citem in item.Children)
            {
              var pnts2 = citem.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
              m.TransformPoints(pnts2);
              path.AddPolygon(pnts2.Select(z => this.Transform(z)).ToArray());
            }
          }

          this.gr.ResetTransform();

          if (!sheets.Contains(item))
          {
            this.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
          }

          this.gr.DrawPath(Pens.Black, path);
        }
      }
    }

    public void RenderNestResult(Font font, bool isInfoShow, NestingContext context)
    {
      var pos1 = this.GetPos();
      var posx = pos1.X;
      var posy = pos1.Y;
      this.Update();

      this.gr.SmoothingMode = SmoothingMode.AntiAlias;
      this.Clear(Color.White);

      this.Reset();

      this.gr.DrawLine(Pens.Red, this.Transform(new PointF(0, 0)), this.Transform(new PointF(1000, 0)));
      this.gr.DrawLine(Pens.Blue, this.Transform(new PointF(0, 0)), this.Transform(new PointF(0, 1000)));
      int yy = 0;
      int gap = (int)font.Size;
      if (isInfoShow)
      {
        this.gr.DrawString("X:" + posx.ToString("0.00") + " Y: " + posy.ToString("0.00"), font, Brushes.Blue, 0, yy);
        yy += (int)font.Size + gap;
        if (context.Nest != null && context.State.TopNestResults != null && context.State.TopNestResults.Top != null)
        {
          this.gr.DrawString($"Material Utilization: {Math.Round((context.Current?.MaterialUtilization ?? 0) * 100.0f, 2)}%   Iterations: {context.State.Iterations}    Parts placed: {context.Current?.TotalPlacedCount ?? 0}/{context.Polygons.Count} ({100 * context.Current?.PartsPlacedPercent ?? 0:N2}%)", font, Brushes.DarkBlue, 0, yy);
          yy += (int)font.Size + gap;
          if (SvgNest.Config.UseParallel)
          {
            this.gr.DrawString($"Generations: {context.State.Generations}    Population: {context.State.Population}    Threads: {context.State.Threads}", font, Brushes.DarkBlue, 0, yy);
          }
          else
          {
            this.gr.DrawString($"Generations: {context.State.Generations}    Population: {context.State.Population}", font, Brushes.DarkBlue, 0, yy);
          }

          yy += (int)font.Size + gap;
          this.gr.DrawString($"Sheets: {context.Sheets.Count}   Parts:{context.Polygons.Count}    parts types: {context.Polygons.GroupBy(z => z.Source).Count()}", font, Brushes.DarkBlue, 0, yy);
          yy += (int)font.Size + gap;
          this.gr.DrawString($"Nests: {context.State.TopNestResults.Count} Fitness: {context.State.TopNestResults.Top.Fitness}   Area:{context.State.TopNestResults.Top.TotalSheetsArea}  ", font, Brushes.DarkBlue, 0, yy);
          yy += (int)font.Size + gap;
          this.gr.DrawString($"Minkowski Calls: {context.State.CallCounter};  Last placing time: {context.State.LastPlacementTime}ms;  Average nest time: {context.State.AverageNestTime}ms", font, Brushes.DarkBlue, 0, yy);
          yy += (int)font.Size + gap;
        }
      }
      else
      {
        if (context.Nest != null && context.State.TopNestResults != null && context.State.TopNestResults.Top != null)
        {
          this.gr.DrawString($"Iterations: {context.State.Iterations}    Parts placed: {context.Current?.TotalPlacedCount ?? 0}/{context.Polygons.Count} ({100 * context.Current?.PartsPlacedPercent:N2}%)", font, Brushes.DarkBlue, 0, yy);
          yy += (int)font.Size + gap;
        }

        this.gr.DrawString($"Generations: {context.State.Generations}    Population: {context.State.Population}", font, Brushes.DarkBlue, 0, yy);
        yy += (int)font.Size + gap;
        this.gr.DrawString($"Sheets: {context.Sheets.Count}   Parts:{context.Polygons.Count}    Parts types: {context.Polygons.GroupBy(z => z.Source).Count()}", font, Brushes.DarkBlue, 0, yy);
        yy += (int)font.Size + gap;
      }

      int i = 0;
      foreach (var item in context.Polygons.Union(context.Sheets))
      {
        if (!(item is Sheet))
        {
          if (!item.Fitted) continue;
        }

        GraphicsPath path = new GraphicsPath();
        if (item.Points != null && item.Points.Any())
        {
          // rotate first;
          var m = new Matrix();
          m.Translate((float)item.X, (float)item.Y);
          m.Rotate((float)item.Rotation);

          var pnts = item.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
          m.TransformPoints(pnts);

          path.AddPolygon(pnts.Select(z => this.Transform(z)).ToArray());

          if (!(item is Sheet) && isInfoShow && SvgNest.Config.ShowPartPositions)
          {
            var label = $"{item.PlacementOrder} ({item.X:N0},{item.Y:N0})@{item.Rotation}";
            var midPnt = new PointF(pnts.Average(o => o.X), pnts.Average(o => o.Y));
            this.gr.DrawString(label, font, Brushes.Black, this.Transform(midPnt));
          }

          if (item.Children != null)
          {
            foreach (var citem in item.Children)
            {
              var pnts2 = citem.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
              m.TransformPoints(pnts2);
              path.AddPolygon(pnts2.Select(z => this.Transform(z)).ToArray());
            }
          }

          this.gr.ResetTransform();

          /*if (selected == item)
          {
              this.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.Orange)), path);
              this.gr.DrawPath(Pens.DarkBlue, path);

          }
          else*/
          {
            if (!context.Sheets.Contains(item))
            {
              this.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
            }

            this.gr.DrawPath(Pens.Black, path);
          }

          if (item is Sheet)
          {
            if (context.Current != null)
            {
              var trans1 = this.Transform(new PointF(pnts[0].X, pnts[0].Y - 30));
              var sheetPlacement = context.Current.UsedSheets.FirstOrDefault(s => s.SheetId == item.Id);
              if (sheetPlacement != null)
              {
                this.gr.DrawString($"util: {100 * sheetPlacement.MaterialUtilization:N2}% {sheetPlacement.ToString()}", font, Brushes.Black, trans1);

                if (isInfoShow)
                {
                  var hullPoints = sheetPlacement.Hull.Points.Select(z => new PointF((float)z.X, (float)z.Y)).ToArray();
                  m.TransformPoints(hullPoints);

                  path.AddPolygon(hullPoints.Select(z => this.Transform(z)).ToArray());
                  this.gr.DrawPath(Pens.Red, path);

                  //var simplifyPoints = sheetPlacement.Simplify.Points.Select(z => new PointF((double)z.x, (double)z.y)).ToArray();
                  //m.TransformPoints(simplifyPoints);

                  //path.AddPolygon(simplifyPoints.Select(z => this.Transform(z)).ToArray());
                  //this.gr.DrawPath(Pens.Green, path);
                  if (sheetPlacement == context.Current.UsedSheets.First())
                  {
                    var trans2 = this.Transform(new PointF(pnts[0].X, pnts[0].Y - 30 + (int)font.Size + gap));
                    this.gr.DrawString($"util: {100 * context.Current.MaterialUtilization:N2}% {context.Current.ToString()}", font, Brushes.Black, trans2);
                  }
                }
              }
            }
          }
        }
      }

      this.Setup();
    }

    public void RenderPreview(object previewObject)
    {
      this.Update();
      this.Clear(Color.White);

      //this.gr.DrawLine(Pens.Blue, this.Transform(new PointF(0, 0)), this.Transform(100, 0));
      //this.gr.DrawLine(Pens.Red, this.Transform(new PointF(0, 0)), this.Transform(0, 100));
      this.Reset();

      if (previewObject != null)
      {
        RectangleF bnd;
        if (previewObject is RawDetail || previewObject is NFP)
        {
          if (previewObject is RawDetail raw)
          {
            this.Draw(raw, Pens.Black, Brushes.LightBlue);
            if (SvgNest.Config.DrawSimplification)
            {
              AddApproximation(raw);
            }

            bnd = raw.BoundingBox();
          }
          else
          {
            var g = this.Draw(previewObject as NFP, Pens.Black, Brushes.LightBlue);
            bnd = g.GetBounds();
          }

          var cap = $"{bnd.Width:N2} x {bnd.Height:N2}";
          this.DrawLabel(cap, Brushes.Black, Color.LightGreen, 5, 5);
        }
      }

      this.Setup();
    }

    /// <summary>
    /// Display the bounds that will be used by the nesting algorithym.
    /// </summary>
    /// <param name="raw">The part to approximate.</param>
    private void AddApproximation(RawDetail raw)
    {
      var part = raw.ToNfp();
      var simplification = SvgNest.simplifyFunction(part, false, SvgNest.Config);
      this.Draw(simplification, Pens.Red);
      var pointsChange = $"{part.Points.Length} => {simplification.Points.Length} points";
      this.DrawLabel(pointsChange, Brushes.Black, Color.Orange, 5, (int)(10 + this.GetLabelHeight()));
    }
  }
}
