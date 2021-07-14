namespace DeepNestPort
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Data;
  using System.Diagnostics;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Threading;
  using System.Windows.Forms;
  using System.Xml.Linq;
  using DeepNestLib;
  using DeepNestLib.Placement;

  public partial class Form1 : Form
  {
    private static volatile object contextSyncLock = new object();

    private NestingContext context;
    private MessageFilter mf = null;
    private bool isInfoShow = false;
    private PictureBoxProgressBar progressBar1;
    private object selected = null;
    private Thread dth;
    private object preview;
    private int errorMessageCount = 0;

    public Form1()
    {
      InitializeComponent();

      this.ProgressDisplayerInstance = new ProgressDisplayer(this);
      this.ContextualiseRunStopButtons(false);

      LoadSettings();
      sheetsInfos.Add(new SheetLoadInfo() { Width = SvgNest.Config.SheetWidth, Height = SvgNest.Config.SheetHeight, Quantity = SvgNest.Config.SheetQuantity });

      //hack
      toolStripButton9.BackgroundImageLayout = ImageLayout.None;
      toolStripButton9.BackgroundImage = new Bitmap(1, 1);
      toolStripButton9.BackColor = Color.LightGreen;

      sheetsList.SetObjects(sheetsInfos);

      ctx = new DrawingContext(nestPreview);
      ctx2 = new DrawingContext(debugPreview);
      ctx3 = new DrawingContext(partsPreview);
      ctx3.FocusOnMove = false;
      ctx2.FocusOnMove = false;

      listView1.DoubleBuffered(true);
      listView2.DoubleBuffered(true);
      listView3.DoubleBuffered(true);
      listViewTopNests.DoubleBuffered(true);
      progressBar1 = new PictureBoxProgressBar();
      progressBar1.Dock = DockStyle.Fill;
      panel1.Controls.Add(progressBar1);

      checkBox2.Checked = SvgNest.Config.Simplify;
      checkBox3.Checked = SvgNest.Config.OffsetTreePhase;
      checkBox4.Checked = SvgNest.Config.UseParallel;
      this.numericUpDown1.Minimum = SvgNestConfig.PopulationMin;
      this.numericUpDown1.Maximum = SvgNestConfig.PopulationMax;
      this.numericUpDown1.Value = SvgNest.Config.PopulationSize;
      this.numericUpDown2.Minimum = SvgNestConfig.MutationRateMin;
      this.numericUpDown2.Maximum = SvgNestConfig.MutationRateMax;
      this.numericUpDown2.Value = SvgNest.Config.MutationRate;

      this.placementTypeCombo.SelectedItem = SvgNest.Config.PlacementType.ToString();
      this.textBox1.Text = SvgNest.Config.Spacing.ToString();
      this.textBox6.Text = SvgNest.Config.CurveTolerance.ToString();

      this.checkBox5.Checked = SvgNest.Config.DrawSimplification;
      this.checkBox6.Checked = SvgNest.Config.ClipByHull;

      this.showPartPositions.Checked = SvgNest.Config.ShowPartPositions;

      this.strictAnglesCombo.Items.AddRange(Enum.GetNames(typeof(AnglesEnum)));
      this.strictAnglesCombo.SelectedItem = SvgNest.Config.StrictAngles.ToString();

      this.multiplierUpDown.Value = SvgNest.Config.Multiplier;

      UpdateFilesList(@"dxfs");
      Load += Form1_Load;
    }

    private ProgressDisplayer ProgressDisplayerInstance { get; }

    private NestingContext Context
    {
      get
      {
        lock (contextSyncLock)
        {
          if (this.context == null)
          {
            this.context = new NestingContext(new MessageBoxService(), new ProgressDisplayer(this));
          }
        }

        return this.context;
      }

      set
      {
        lock (contextSyncLock)
        {
          this.context = value;
        }
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      mf = new MessageFilter();
      Application.AddMessageFilter(mf);
    }

    private void LoadSettings()
    {
      if (!File.Exists("settings.xml"))
      {
        return;
      }

      var doc = XDocument.Load("settings.xml");
      foreach (var item in doc.Descendants("setting"))
      {
        switch (item.Attribute("name").Value)
        {
          case "closingThreshold":
            {
              DxfParser.ClosingThreshold = double.Parse(item.Attribute("value").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            }

            break;
          case "removeThreshold":
            {
              DxfParser.RemoveThreshold = double.Parse(item.Attribute("value").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            }

            break;
        }
      }
    }

    public void UpdateList()
    {
      listView1.Items.Clear();
      foreach (var item in Polygons)
      {
        listView1.Items.Add(new ListViewItem(new string[] { item.Id.ToString(), item.Source.ToString(), item.Name, item.Points.Count().ToString() }) { Tag = item });
      }
      listView2.Items.Clear();
      foreach (var item in sheets)
      {
        listView2.Items.Add(new ListViewItem(new string[] { item.Id.ToString(), item.Source.ToString(), item.Name, item.Points.Count().ToString() }) { Tag = item });
      }

      groupBox5.Text = "Parts: " + Polygons.Count();
      groupBox6.Text = "Sheets: " + sheets.Count;
    }

    private void RedrawPreview(DrawingContext ctx2, object previewObject)
    {
      ctx2.Update();
      ctx2.Clear(Color.White);

      //ctx2.gr.DrawLine(Pens.Blue, ctx2.Transform(new PointF(0, 0)), ctx2.Transform(100, 0));
      //ctx2.gr.DrawLine(Pens.Red, ctx2.Transform(new PointF(0, 0)), ctx2.Transform(0, 100));
      ctx2.Reset();

      if (previewObject != null)
      {
        RectangleF bnd;
        if (previewObject is RawDetail || previewObject is NFP)
        {
          if (previewObject is RawDetail raw)
          {
            ctx2.Draw(raw, Pens.Black, Brushes.LightBlue);
            if (SvgNest.Config.DrawSimplification)
            {
              AddApproximation(ctx2, raw);
            }

            bnd = raw.BoundingBox();
          }
          else
          {
            var g = ctx2.Draw(previewObject as NFP, Pens.Black, Brushes.LightBlue);
            bnd = g.GetBounds();
          }

          var cap = $"{bnd.Width:N2} x {bnd.Height:N2}";
          ctx2.DrawLabel(cap, Brushes.Black, Color.LightGreen, 5, 5);
        }
      }

      ctx2.Setup();
    }

    /// <summary>
    /// Display the bounds that will be used by the nesting algorithym.
    /// </summary>
    /// <param name="ctx">Drawing context upon which to draw.</param>
    /// <param name="raw">The part to approximate.</param>
    private void AddApproximation(DrawingContext ctx, RawDetail raw)
    {
      NFP part = raw.ToNfp();
      var simplification = SvgNest.simplifyFunction(part, false);
      ctx.Draw(simplification, Pens.Red);
      var pointsChange = $"{part.Points.Length} => {simplification.Points.Length} points";
      ctx.DrawLabel(pointsChange, Brushes.Black, Color.Orange, 5, (int)(10 + ctx.GetLabelHeight()));
    }

    private void Redraw()
    {
      try
      {
        var pos = nestPreview.PointToClient(Cursor.Position);
        var pos1 = ctx.GetPos();
        var posx = pos1.X;
        var posy = pos1.Y;
        ctx.Update();

        RedrawPreview(ctx2, preview);
        RedrawPreview(ctx3, preview);

        ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
        ctx.Clear(Color.White);

        ctx.Reset();

        ctx.gr.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
        ctx.gr.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));
        int yy = 0;
        int gap = (int)Font.Size;
        if (isInfoShow)
        {
          ctx.gr.DrawString("X:" + posx.ToString("0.00") + " Y: " + posy.ToString("0.00"), Font, Brushes.Blue, 0, yy);
          yy += (int)Font.Size + gap;
          if (this.Context.Nest != null && this.Context.Nest.TopNestResults != null && this.Context.Nest.TopNestResults.Top != null)
          {
            ctx.gr.DrawString($"Material Utilization: {Math.Round(Context.Nest.TopNestResults.Top.MaterialUtilization * 100.0f, 2)}%   Iterations: {Context.Iterations}    Parts placed: {Context.PlacedPartsCount}/{Polygons.Count} ({100 * Context.Nest.TopNestResults.Top.PartsPlacedPercent:N2}%)", Font, Brushes.DarkBlue, 0, yy);
            yy += (int)Font.Size + gap;
            ctx.gr.DrawString($"Generations: {SvgNest.Generations}    Population: {SvgNest.Population}", Font, Brushes.DarkBlue, 0, yy);
            yy += (int)Font.Size + gap;
            ctx.gr.DrawString($"Sheets: {sheets.Count}   Parts:{Polygons.Count}    parts types: {Polygons.GroupBy(z => z.Source).Count()}", Font, Brushes.DarkBlue, 0, yy);
            yy += (int)Font.Size + gap;
            ctx.gr.DrawString($"Nests: {this.Context.Nest.TopNestResults.Count} Fitness: {this.Context.Nest.TopNestResults.Top.Fitness}   Area:{this.Context.Nest.TopNestResults.Top.TotalSheetsArea}  ", Font, Brushes.DarkBlue, 0, yy);
            yy += (int)Font.Size + gap;
            ctx.gr.DrawString($"Minkowski Calls: {Background.CallCounter};  Last placing time: {Context.Nest.LastPlacementTime}ms;  Average nest time: {Context.Nest.AverageNestTime}ms", Font, Brushes.DarkBlue, 0, yy);
            yy += (int)Font.Size + gap;
          }
        }
        else
        {
          if (this.Context.Nest != null && this.Context.Nest.TopNestResults != null && this.Context.Nest.TopNestResults.Top != null)
          {
            ctx.gr.DrawString($"Iterations: {Context.Iterations}    Parts placed: {Context.PlacedPartsCount}/{Polygons.Count} ({100 * Context.Nest.TopNestResults.Top.PartsPlacedPercent:N2}%)", Font, Brushes.DarkBlue, 0, yy);
            yy += (int)Font.Size + gap;
          }

          ctx.gr.DrawString($"Generations: {SvgNest.Generations}    Population: {SvgNest.Population}", Font, Brushes.DarkBlue, 0, yy);
          yy += (int)Font.Size + gap;
          ctx.gr.DrawString($"Sheets: {sheets.Count}   Parts:{Polygons.Count}    Parts types: {Polygons.GroupBy(z => z.Source).Count()}", Font, Brushes.DarkBlue, 0, yy);
          yy += (int)Font.Size + gap;
        }

        if (!checkBox1.Checked)
        {
          if (bb != null)
          {
            //ctx.gr.TranslateTransform((float)sheets[0].x, (float)sheets[0].y);
            var pp = ctx.Transform((float)sheets[0].x, (float)sheets[0].y);
            ctx.gr.DrawImage(bb, new RectangleF(pp.X, pp.Y, bb.Width * ctx.zoom, bb.Height * ctx.zoom), new Rectangle(0, 0, bb.Width, bb.Height), GraphicsUnit.Pixel);
          }
        }

        int i = 0;
        foreach (var item in Polygons.Union(sheets))
        {
          if (!checkBox1.Checked)
          {
            continue;
          }

          if (!(item is Sheet))
          {
            if (!item.Fitted) continue;
          }

          GraphicsPath path = new GraphicsPath();
          if (item.Points != null && item.Points.Any())
          {
            // rotate first;
            var m = new Matrix();
            m.Translate((float)item.x, (float)item.y);
            m.Rotate(item.Rotation);

            var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
            m.TransformPoints(pnts);

            path.AddPolygon(pnts.Select(z => ctx.Transform(z)).ToArray());

            if (!(item is Sheet) && isInfoShow && SvgNest.Config.ShowPartPositions)
            {
              var label = $"{item.PlacementOrder} ({item.x:N0},{item.y:N0})@{item.Rotation}";
              var ms = ctx2.gr.MeasureString(label, SystemFonts.DefaultFont);
              var midPnt = new PointF(pnts.Average(o => o.X), pnts.Average(o => o.Y));
              ctx.gr.DrawString(label, Font, Brushes.Black, ctx.Transform(midPnt));
            }

            if (item.Children != null)
            {
              foreach (var citem in item.Children)
              {
                var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                m.TransformPoints(pnts2);
                path.AddPolygon(pnts2.Select(z => ctx.Transform(z)).ToArray());
              }
            }

            ctx.gr.ResetTransform();

            /*if (selected == item)
            {
                ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.Orange)), path);
                ctx.gr.DrawPath(Pens.DarkBlue, path);

            }
            else*/
            {
              if (!sheets.Contains(item))
              {
                ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
              }

              ctx.gr.DrawPath(Pens.Black, path);
            }

            if (item is Sheet)
            {
              if (this.Context.Current != null)
              {
                var trans1 = ctx.Transform(new PointF((float)pnts[0].X, (float)pnts[0].Y - 30));
                var sheetPlacement = this.Context.Current.UsedSheets.FirstOrDefault(s => s.SheetId == item.Id);
                if (sheetPlacement != null)
                {
                  ctx.gr.DrawString($"util: {100 * sheetPlacement.MaterialUtilization:N2}% {sheetPlacement.ToString()}", Font, Brushes.Black, trans1);

                  if (isInfoShow)
                  {
                    var hullPoints = sheetPlacement.Hull.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                    m.TransformPoints(hullPoints);

                    path.AddPolygon(hullPoints.Select(z => ctx.Transform(z)).ToArray());
                    ctx.gr.DrawPath(Pens.Red, path);

                    //var simplifyPoints = sheetPlacement.Simplify.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                    //m.TransformPoints(simplifyPoints);

                    //path.AddPolygon(simplifyPoints.Select(z => ctx.Transform(z)).ToArray());
                    //ctx.gr.DrawPath(Pens.Green, path);
                    if (sheetPlacement == this.Context.Current.UsedSheets.First())
                    {
                      var trans2 = ctx.Transform(new PointF((float)pnts[0].X, (float)pnts[0].Y - 30 + (int)Font.Size + gap));
                      ctx.gr.DrawString($"util: {100 * this.Context.Current.MaterialUtilization:N2}% {this.Context.Current.ToString()}", Font, Brushes.Black, trans2);
                    }
                  }
                }
              }
            }
          }
        }

        ctx.Setup();
      }
      catch (Exception ex)
      {
        // NOP - the code iterates collections that could change during the Redraw; so just swallow and let it recover next tick.
        this.ShowMessage(ex);
      }
    }

    private void RenderSheet()
    {
      ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
      ctx.Clear(Color.White);
      ctx.Reset();

      foreach (var item in Polygons.Union(sheets))
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
          m.Translate((float)item.x, (float)item.y);
          m.Rotate(item.Rotation);

          var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
          m.TransformPoints(pnts);

          path.AddPolygon(pnts.Select(z => ctx.Transform(z)).ToArray());

          if (item.Children != null)
          {
            foreach (var citem in item.Children)
            {
              var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
              m.TransformPoints(pnts2);
              path.AddPolygon(pnts2.Select(z => ctx.Transform(z)).ToArray());
            }
          }

          ctx.gr.ResetTransform();

          if (!sheets.Contains(item))
          {
            ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
          }

          ctx.gr.DrawPath(Pens.Black, path);
        }
      }
    }

    private void RedrawAsync()
    {
      if (dth != null) return;
      dth = new Thread(() =>
       {
         Redraw();
         dth = null;
       });
      dth.IsBackground = true;
      dth.Start();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      try
      {
        progressBar1.UpdateImg();
        progressBar1.Value = (int)Math.Round(progressVal * 100f);
        Redraw();
      }
      catch
      {
        //NOP
      }
    }

    public DrawingContext ctx;
    public DrawingContext ctx2;
    public DrawingContext ctx3;

    public void UpdateNestsList()
    {
      try
      {
        if (this.Context.Nest != null)
        {
          listViewTopNests.Invoke((Action)(() =>
          {
            listViewTopNests.BeginUpdate();
            int selectedIndex = listViewTopNests.FocusedItem?.Index ?? 0;
            listViewTopNests.Items.Clear();
            int i = 0;
            if (this.Context?.Nest != null)
            {
              foreach (var item in this.Context.Nest.TopNestResults)
              {
                var listItem = new ListViewItem(new string[] { item.Fitness.ToString("N0"), item.CreatedAt.ToString("HH:mm:ss.fff") }) { Tag = item };
                listViewTopNests.Items.Add(listItem);
                if (i == selectedIndex)
                {
                  listItem.Selected = true;
                  listItem.Focused = true;
                }

                i++;
              }
            }

            listViewTopNests.EndUpdate();
          }));
        }
      }
      catch (InvalidOperationException)
      {
        //NOP
      }
      catch (InvalidAsynchronousStateException)
      {
        //NOP
      }
      catch (Exception ex)
      {
        this.ShowMessage(ex);
      }
    }

    Thread th;

    internal void DisplayProgress(float progressVal)
    {
      this.progressVal = progressVal;
    }

    public string ToolStripMessage
    {
      set
      {
        try
        {
          _ = this.Invoke((MethodInvoker)(() => { this.toolStripStatusLabel1.Text = value; }));
          Application.DoEvents();
        }
        catch (Exception ex)
        {
          System.Diagnostics.Debug.Print(ex.Message);
        }
      }
    }

    public float progressVal = 0;

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        selected = listView1.SelectedItems[0].Tag;
        preview = selected;
      }
    }

    private void pictureBox1_MouseEnter(object sender, EventArgs e)
    {
      //pictureBox1.Focus();
    }


    private void UpdateFilesList(string path)
    {
      var di = new DirectoryInfo(path);
      groupBox3.Text = "Files: " + di.FullName;
      listView3.Items.Clear();
      listView3.Items.Add(new ListViewItem(new string[] { ".." }) { Tag = di.Parent, BackColor = Color.LightBlue });
      foreach (var item in di.GetDirectories())
      {
        listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item, BackColor = Color.LightBlue });
      }

      foreach (var item in di.GetFiles())
      {
        if (!(item.Extension.Contains("svg") || item.Extension.Contains("dxf"))) continue;
        listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
      }
    }

    private Sheet NewSheet(int w = 3000, int h = 1500)
    {
      var tt = new RectangleSheet();
      tt.Name = "rectSheet" + (sheets.Count + 1);
      tt.Height = h;
      tt.Width = w;
      tt.Rebuild();

      return tt;
    }

    private Sheet NewRhombusSheet(int w = 3000, int h = 1500)
    {
      var tt = new Sheet();
      tt.Name = "rhombSheet" + (sheets.Count + 1);

      tt.Height = h;
      tt.Width = w;
      int x = 0;
      int y = 0;
      int _width = w;
      int _height = h;

      tt.AddPoint(new SvgPoint(x + _width / 2, y));
      tt.AddPoint(new SvgPoint(x, y + _height / 2));
      tt.AddPoint(new SvgPoint(x + _width / 2, y + _height));
      tt.AddPoint(new SvgPoint(x + _width, y + _height / 2));

      return tt;
    }

    private Sheet NewCircleSheet(int w = 3000)
    {
      var tt = new Sheet();
      tt.Name = "circleSheet" + (sheets.Count + 1);

      tt.Height = w;
      tt.Width = w;
      int x = 0;
      int y = 0;

      for (int i = 0; i < 360; i += 5)
      {
        var xx = w / 2 * Math.Cos(i * Math.PI / 180.0f);
        var yy = w / 2 * Math.Sin(i * Math.PI / 180.0f);
        tt.AddPoint(new SvgPoint(xx + w / 2, yy + w / 2));
      }

      return tt;
    }

    private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Polygons.Clear();
      UpdateList();
    }

    private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        for (int i = 0; i < listView1.SelectedItems.Count; i++)
        {
          Polygons.Remove(listView1.SelectedItems[i].Tag as NFP);
        }
        UpdateList();
      }
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      try
      {
        SvgNest.Config.Spacing = float.Parse(textBox1.Text, CultureInfo.InvariantCulture);
        textBox1.BackColor = Color.White;
        textBox1.ForeColor = Color.Black;
      }
      catch (Exception ex)
      {
        textBox1.BackColor = Color.Red;
        textBox1.ForeColor = Color.White;
      }
    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
      try
      {
        SvgNest.Config.SheetSpacing = float.Parse(textBox2.Text, CultureInfo.InvariantCulture);
        textBox2.BackColor = Color.White;
        textBox2.ForeColor = Color.Black;
      }
      catch (Exception ex)
      {
        textBox2.BackColor = Color.Red;
        textBox2.ForeColor = Color.White;
      }
    }

    private void textBox3_TextChanged(object sender, EventArgs e)
    {
      try
      {
        SvgNest.Config.Rotations = int.Parse(textBox3.Text, CultureInfo.InvariantCulture);
        textBox3.BackColor = Color.White;
        textBox3.ForeColor = Color.Black;
      }
      catch (Exception ex)
      {
        textBox3.BackColor = Color.Red;
        textBox3.ForeColor = Color.White;
      }
    }

    private void moveToSheetsToolStripMenuItem_Click(object sender, EventArgs e)
    {

      if (listView1.SelectedItems.Count > 0)
      {
        var pol = listView1.SelectedItems[0].Tag as NFP;
        Polygons.Remove(pol);
        var b = GeometryUtil.getPolygonBounds(pol);
        Sheet sheet = new Sheet();
        foreach (var item in pol.Points)
        {
          sheet.AddPoint(new SvgPoint(item.x - b.x, item.y - b.y));
        }
        /*if (pol.children != null)
        {
            sheet.children = new List<NFP>();
            for (int i = 0; i < pol.children.Count; i++)
            {
                var child = pol.children[i];
                NFP newchild = new NFP();
                for (var j = 0; j < child.length; j++)
                {
                    newchild.AddPoint(new SvgPoint(child[j].x - b.x, child[j].y - b.y));
                }
                sheet.children.Add(newchild);
            }
        }*/

        sheet.Width = (float)b.width;
        sheet.Height = (float)b.height;

        sheet.Source = Context.GetNextSheetSource();
        sheets.Add(sheet);
        Context.ReorderSheets();
        UpdateList();
      }
    }

    private void clearAllToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      sheets.Clear();
      UpdateList();
    }

    private void moveToPolygonsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (listView2.SelectedItems.Count > 0)
      {
        var pol = listView2.SelectedItems[0].Tag as NFP;
        sheets.Remove(pol);
        Polygons.Add(pol);
        UpdateList();
      }
    }

    private void listView2_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listView2.SelectedItems.Count > 0)
      {
        selected = listView2.SelectedItems[0].Tag;
        preview = selected;
      }
    }

    private void listView3_MouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (listView3.SelectedItems.Count <= 0) return;

      var si = listView3.SelectedItems[0].Tag;
      if (si is DirectoryInfo)
      {
        UpdateFilesList((si as DirectoryInfo).FullName);

      }
      if (si is FileInfo)
      {
        var f = (si as FileInfo);
        QntDialog q = new QntDialog();
        if (q.ShowDialog() == DialogResult.OK)
        {
          RawDetail det = LoadRawDetail(f);

          int src = 0;
          if (Polygons.Any())
          {
            src = Polygons.Max(z => z.Source) + 1;
          }

          AddToPolygons(src, det, q.Qnt);
          UpdateList();
        }
      }
    }

    private static RawDetail LoadRawDetail(FileInfo f)
    {
      RawDetail det = null;
      if (f.Extension == ".svg")
      {
        det = SvgParser.LoadSvg(f.FullName);
      }
      if (f.Extension == ".dxf")
      {
        det = DxfParser.LoadDxfFile(f.FullName);
      }

      return det;
    }

    private void importSelectedToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (listView3.SelectedItems.Count <= 0) return;

      QntDialog q = new QntDialog();
      if (q.ShowDialog() == DialogResult.OK)
      {
        foreach (var item in listView3.SelectedItems)
        {
          var t = (item as ListViewItem).Tag as FileInfo;
          var det = LoadRawDetail(t);

          int src = 0;
          if (Polygons.Any())
          {
            src = Polygons.Max(z => z.Source) + 1;
          }

          AddToPolygons(src, det, q.Qnt);
        }

        UpdateList();
      }
    }

    bool stop = false;

    private void RunDeepnest()
    {
      try
      {
        if (this.th == null)
        {
          this.th = new Thread(() =>
          {
            _ = this.Invoke((MethodInvoker)(() => { this.progressBar1.Visible = true; }));
            this.Context.StartNest();
            UpdateNestsList();

            while (!this.stop)
            {
              Stopwatch sw = new Stopwatch();
              sw.Start();
              Cursor.Current = Cursors.Default;
              this.Context.NestIterate(SvgNest.Config);
              UpdateNestsList();
              sw.Stop();
              if (SvgNest.Config.UseParallel)
              {
                this.ProgressDisplayerInstance.DisplayToolStripMessage($"Iteration time: {sw.ElapsedMilliseconds}ms ({this.context.Nest.AverageNestTime}ms average)");
              }
              else
              {
                this.ProgressDisplayerInstance.DisplayToolStripMessage($"Nesting time: {sw.ElapsedMilliseconds}ms");
              }

              if (this.Context.IsErrored)
              {
                break;
              }
            }

            ContextualiseRunStopButtons(false);
            this.th = null;
          });

          this.th.IsBackground = true;
          this.th.Start();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error occurred during Nest. . .", ex.Message);
      }
    }

    Random r = new Random();

    private void cloneQntToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        QntDialog qd = new QntDialog();
        if (qd.ShowDialog() == DialogResult.OK)
        {
          var nfp = (listView1.SelectedItems[0].Tag as NFP);
          for (int i = 0; i < qd.Qnt; i++)
          {
            var r = nfp.Clone();
            Polygons.Add(r);
          }

          UpdateList();
        }
      }
    }

    void run()
    {
      if (sheets.Count == 0 || Polygons.Count == 0)
      {
        Cursor.Current = Cursors.Default;
        MessageBox.Show("There are no sheets or parts", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      ContextualiseRunStopButtons(true);

      Cursor.Current = Cursors.WaitCursor;
      stop = false;
      progressBar1.Value = 0;
      tabControl1.SelectedTab = tabPage4;
      Context.ReorderSheets();
      RunDeepnest();
    }

    private void ContextualiseRunStopButtons(bool isRunning)
    {
      runButton.Enabled = !isRunning;
      this.runNestingButton.Enabled = !isRunning;
      this.stopButton.Enabled = isRunning;
      Application.DoEvents();
    }

    private void button10_Click(object sender, EventArgs e)
    {
      run();
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.Simplify = checkBox2.Checked;
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.OffsetTreePhase = checkBox3.Checked;
    }

    private void checkBox4_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.UseParallel = checkBox4.Checked;
    }

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      var t = placementTypeCombo.SelectedItem as string;
      if (t.ToLower().Contains("gravi"))
      {
        SvgNest.Config.PlacementType = PlacementTypeEnum.Gravity;
      }
      if (t.ToLower().Contains("box"))
      {
        SvgNest.Config.PlacementType = PlacementTypeEnum.BoundingBox;
      }
      if (t.ToLower().Contains("squ"))
      {
        SvgNest.Config.PlacementType = PlacementTypeEnum.Squeeze;
      }
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
      SvgNest.Config.PopulationSize = (int)numericUpDown1.Value;
    }

    private void listViewTopNests_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (listViewTopNests.SelectedItems.Count > 0)
      {
        var nestResult = listViewTopNests.SelectedItems[0].Tag as NestResult;
        Context.AssignPlacement(nestResult);
      }
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
      SvgNest.Config.MutationRate = (int)numericUpDown2.Value;
    }


    private void button1_Click(object sender, EventArgs e)
    {
      var cnt = (int)numericUpDown3.Value;
      int? ww = null;
      int? hh = null;

      try
      {
        ww = int.Parse(textBox4.Text);
        textBox4.BackColor = Color.White;
        textBox4.ForeColor = Color.Black;
      }
      catch (Exception ex)
      {
        textBox4.BackColor = Color.Red;
        textBox4.ForeColor = Color.White;
      }

      try
      {
        hh = int.Parse(textBox5.Text);
        textBox5.BackColor = Color.White;
        textBox5.ForeColor = Color.Black;
      }
      catch (Exception ex)
      {
        textBox5.BackColor = Color.Red;
        textBox5.ForeColor = Color.White;
      }

      if (ww == null || hh == null)
      {
        MessageBox.Show("Wrong sizes", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }


      if (comboBox2.SelectedItem == null)
      {
        label11.BackColor = Color.Red;
        label11.ForeColor = Color.White;
        return;
      }
      label11.BackColor = label11.Parent.BackColor;
      label11.ForeColor = label11.Parent.ForeColor;
      List<Sheet> sh = new List<Sheet>();
      var src = Context.GetNextSheetSource();
      for (int i = 0; i < cnt; i++)
      {
        switch (comboBox2.SelectedItem.ToString())
        {
          case "Rectangle":
            sh.Add(NewSheet(ww.Value, hh.Value));
            break;
          case "Rhombus":
            sh.Add(NewRhombusSheet(ww.Value, hh.Value));
            break;
          case "Circle":
            sh.Add(NewCircleSheet(ww.Value));
            break;
        }
      }
      foreach (var item in sh)
      {
        item.Source = src;
        Context.Sheets.Add(item);
      }
      UpdateList();
      Context.ReorderSheets();
    }

    private int GetCountFromDialog()
    {
      QntDialog q = new DeepNestPort.QntDialog();
      if (q.ShowDialog() == DialogResult.OK)
      {
        return q.Qnt;
      }
      return 0;
    }

    private void button13_Click(object sender, EventArgs e)
    {
      var cnt = GetCountFromDialog();
      Random r = new Random();
      for (int i = 0; i < cnt; i++)
      {
        var xx = r.Next(2000) + 100;
        var yy = r.Next(2000);
        var ww = r.Next(60) + 10;
        var hh = r.Next(60) + 5;
        NFP pl = new NFP();
        int src = 0;
        if (Polygons.Any())
        {
          src = Polygons.Max(z => z.Source) + 1;
        }
        Polygons.Add(pl);
        pl.Source = src;
        pl.x = xx;
        pl.y = yy;
        pl.AddPoint(new SvgPoint(0, 0));
        pl.AddPoint(new SvgPoint(ww, 0));
        pl.AddPoint(new SvgPoint(ww, hh));
        pl.AddPoint(new SvgPoint(0, hh));
      }
      UpdateList();
    }

    private void button14_Click(object sender, EventArgs e)
    {
      Random r = new Random();
      for (int i = 0; i < 10; i++)
      {
        var xx = r.Next(2000) + 100;
        var yy = r.Next(2000);
        var rad = r.Next(60) + 10;

        NFP pl = new NFP();
        int src = 0;
        if (Polygons.Any())
        {
          src = Polygons.Max(z => z.Source) + 1;
        }

        pl.Source = src;
        Polygons.Add(pl);
        pl.x = xx;
        pl.y = yy;
        for (int ang = 0; ang < 360; ang += 15)
        {
          var xx1 = (float)(rad * Math.Cos(ang * Math.PI / 180.0f));
          var yy1 = (float)(rad * Math.Sin(ang * Math.PI / 180.0f));
          pl.AddPoint(new SvgPoint(xx1, yy1));
        }
      }

      UpdateList();
    }

    private void button15_Click(object sender, EventArgs e)
    {

      Random r = new Random();
      for (int i = 0; i < 10; i++)
      {
        var xx = r.Next(2000) + 100;
        var yy = r.Next(2000);
        var ww = r.Next(60) + 10;
        var hh = r.Next(60) + 5;
        NFP pl = new NFP();
        int src = 0;
        if (Polygons.Any())
        {
          src = Polygons.Max(z => z.Source) + 1;
        }
        pl.Source = src;
        Polygons.Add(pl);
        pl.x = xx;
        pl.y = yy;
        pl.AddPoint(new SvgPoint(-ww, 0));
        pl.AddPoint(new SvgPoint(+ww, 0));
        pl.AddPoint(new SvgPoint(0, +hh));

      }
      UpdateList();
    }

    private void button16_Click(object sender, EventArgs e)
    {

      Random r = new Random();
      for (int i = 0; i < 10; i++)
      {
        var xx = r.Next(2000) + 100;
        var yy = r.Next(2000);
        var ww = r.Next(400) + 10;
        var hh = r.Next(400) + 5;
        NFP pl = new NFP();
        int src = 0;
        if (Polygons.Any())
        {
          src = Polygons.Max(z => z.Source) + 1;
        }

        pl.Source = src;
        Polygons.Add(pl);
        pl.AddPoint(new SvgPoint(xx, yy));
        pl.AddPoint(new SvgPoint(xx + ww, yy));
        pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
        pl.AddPoint(new SvgPoint(xx, yy + hh));
      }

      UpdateList();
    }

    private void button17_Click(object sender, EventArgs e)
    {
      var ww = r.Next(400) + 10;
      var hh = r.Next(400) + 5;
      QntDialog q = new QntDialog();
      int src = 0;
      if (Polygons.Any())
      {
        src = Polygons.Max(z => z.Source) + 1;
      }
      if (q.ShowDialog() == DialogResult.OK)
      {
        for (int i = 0; i < q.Qnt; i++)
        {
          var xx = r.Next(2000) + 100;
          var yy = r.Next(2000);

          NFP pl = new NFP();

          pl.Source = src;
          Polygons.Add(pl);
          pl.x = xx;
          pl.y = yy;
          pl.AddPoint(new SvgPoint(0, 0));
          pl.AddPoint(new SvgPoint(0 + ww, 0));
          pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
          pl.AddPoint(new SvgPoint(0, 0 + hh));
        }

        UpdateList();
      }
    }

    private void stopButton_Click(object sender, EventArgs e)
    {
      try
      {
        stop = true;
        this.Context.StopNest();
        ContextualiseRunStopButtons(!stop);

        _ = this.Invoke((MethodInvoker)(() => { this.progressBar1.Visible = false; }));
        Application.DoEvents();
      }
      catch (Exception ex)
      {
        ShowMessage(ex);
      }
    }

    internal ICollection<INfp> Polygons { get { return Context.Polygons; } }

    private void button6_Click(object sender, EventArgs e)
    {
      var cnt = GetCountFromDialog();
      Random r = new Random();
      for (int i = 0; i < cnt; i++)
      {
        var xx = r.Next(2000) + 100;
        var yy = r.Next(2000);
        var ww = r.Next(250) + 150;
        var hh = r.Next(250) + 120;
        NFP pl = new NFP();
        int src = 0;
        if (Polygons.Any())
        {
          src = Polygons.Max(z => z.Source) + 1;
        }

        Polygons.Add(pl);
        pl.Source = src;
        pl.AddPoint(new SvgPoint(0, 0));
        pl.AddPoint(new SvgPoint(0 + ww, 0));
        pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
        pl.AddPoint(new SvgPoint(0, 0 + hh));
        pl.x = xx;
        pl.y = yy;
        var hole = new NFP();

        pl.Children = new List<INfp>();
        pl.Children.Add(hole);
        int gap = 10;
        hole.AddPoint(new SvgPoint(0 + gap, 0 + gap));
        hole.AddPoint(new SvgPoint(0 + ww - gap, 0 + gap));
        hole.AddPoint(new SvgPoint(0 + ww - gap, 0 + hh - gap));
        hole.AddPoint(new SvgPoint(0 + gap, 0 + hh - gap));
        hole.x = xx;
        hole.y = yy;
      }

      UpdateList();
    }

    private void button5_Click(object sender, EventArgs e)
    {
      for (int i = 0; i < 10; i++)
      {
        var xx = r.Next(2000) + 100;
        var yy = r.Next(2000);
        var rad = r.Next(60) + 10;
        int rad2 = rad - 8;

        NFP pl = new NFP();
        int src = 0;
        if (Polygons.Any())
        {
          src = Polygons.Max(z => z.Source) + 1;
        }

        pl.Source = src;
        Polygons.Add(pl);

        NFP hole = new NFP();
        for (int ang = 0; ang < 360; ang += 15)
        {
          var xx1 = (float)(rad * Math.Cos(ang * Math.PI / 180.0f));
          var yy1 = (float)(rad * Math.Sin(ang * Math.PI / 180.0f));
          pl.AddPoint(new SvgPoint(xx1, yy1));
          var xx2 = (float)(rad2 * Math.Cos(ang * Math.PI / 180.0f));
          var yy2 = (float)(rad2 * Math.Sin(ang * Math.PI / 180.0f));
          hole.AddPoint(new SvgPoint(xx2, yy2));
        }

        pl.Children = new List<INfp>();
        pl.Children.Add(hole);
        pl.x = xx;
        pl.y = yy;
      }

      UpdateList();
    }

    private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      if (listView2.SelectedItems.Count > 0)
      {
        var f = listView2.SelectedItems[0].Tag as NFP;
        sheets.Remove(f);
        UpdateList();
        Context.ReorderSheets();
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {

      Random r = new Random();


      var xx = r.Next(2000) + 100;
      var yy = r.Next(2000);
      var ww = 20;
      var hh = 20;
      NFP pl = new NFP();
      int src = 0;
      if (Polygons.Any())
      {
        src = Polygons.Max(z => z.Source) + 1;
      }
      Polygons.Add(pl);
      pl.Source = src;
      pl.x = xx;
      pl.y = yy;
      pl.AddPoint(new SvgPoint(0, 0));
      pl.AddPoint(new SvgPoint(ww, 0));
      pl.AddPoint(new SvgPoint(ww, hh));
      pl.AddPoint(new SvgPoint(0, hh));

      UpdateList();
    }

    private void showHideButton_Click(object sender, EventArgs e)
    {
      isInfoShow = !isInfoShow;
      this.Redraw();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      Font = new Font(Font.FontFamily.Name, Font.Size + 1);
    }

    private void button4_Click(object sender, EventArgs e)
    {
      Font = new Font(Font.FontFamily.Name, Font.Size - 1);
    }

    private void listView3_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (!(listView3.SelectedItems.Count > 0 && listView3.SelectedItems[0].Tag is FileInfo)) return;
      try
      {
        var path = (FileInfo)listView3.SelectedItems[0].Tag;
        RawDetail det = null;
        if (path.Extension == ".svg")
        {
          det = SvgParser.LoadSvg(path.FullName);
        }
        if (path.Extension == ".dxf")
        {
          det = DxfParser.LoadDxfFile(path.FullName);
        }

        preview = det;
      }
      catch (Exception ex)
      {
        preview = null;
      }
    }

    private List<INfp> sheets { get { return Context.Sheets; } }

    private int lastSaveFilterIndex = 1;

    private void exportButton_Click_1(object sender, EventArgs e)
    {
      SaveFileDialog sfd = new SaveFileDialog();
      if (Polygons.ContainsDxfs() && Polygons.ContainsSvgs())
      {
        MessageBox.Show("It's not possible to export when your parts were a mix of Svg's and Dxf's.", "DeepNestPort: Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      else
      {
        IExport exporter = ExporterFactory.GetExporter(Polygons, SvgNest.Config);
        sfd.Filter = exporter.SaveFileDialogFilter;
        if (sfd.ShowDialog() == DialogResult.OK)
        {
          exporter.Export(sfd.FileName, Polygons.ToArray(), sheets.ToArray());
        }
      }
    }

    Bitmap bb;

    private void button7_Click(object sender, EventArgs e)
    {
      var sh = sheets[0] as Sheet;
      ctx.sx = (float)sh.x;
      ctx.sy = (float)sh.y;
      ctx.InvertY = false;
      bb = new Bitmap(1 + (int)sh.Width, 1 + (int)sh.Height);
      var gr = Graphics.FromImage(bb);
      var tmpbmp = ctx.bmp;
      ctx.gr = gr;
      ctx.bmp = bb;

      RenderSheet();
      ctx.gr = gr;
      ctx.bmp = tmpbmp;
      ctx.InvertY = true;
      Clipboard.SetImage(bb);
    }

    private void button8_Click(object sender, EventArgs e)
    {
      var xx = r.Next(2000) + 100;
      var yy = r.Next(2000);
      var ww = r.Next(250) + 150;
      var hh = r.Next(250) + 120;
      NFP pl = new NFP();
      int src = 0;
      if (Polygons.Any())
      {
        src = Polygons.Max(z => z.Source) + 1;
      }
      Polygons.Add(pl);
      pl.Source = src;
      pl.AddPoint(new SvgPoint(0, 0));
      pl.AddPoint(new SvgPoint(0 + ww, 0));
      pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
      pl.AddPoint(new SvgPoint(0, 0 + hh));
      pl.x = xx;
      pl.y = yy;
      pl.Children = new List<INfp>();
      int gap = 10;
      int szx = ww / 4;
      int szy = hh / 3;
      for (int i = 0; i < 3; i++)
      {
        for (int j = 0; j < 2; j++)
        {
          var hole = new NFP();

          pl.Children.Add(hole);

          int hx = (i * ww / 4) + gap * (i + 1);
          int hy = (j * hh / 3) + gap * (j + 1);

          hole.AddPoint(new SvgPoint(hx + szx, hy + szy));
          hole.AddPoint(new SvgPoint(hx, hy + szy));
          hole.AddPoint(new SvgPoint(hx, hy));
          hole.AddPoint(new SvgPoint(hx + szx, hy));
          hole.x = xx;
          hole.y = yy;
        }
      }

      UpdateList();
    }

    int lastOpenFilterIndex = 1;

    private void loadDetailButton_Click(object sender, EventArgs e)
    {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
      ofd.FilterIndex = lastOpenFilterIndex;
      ofd.Multiselect = true;
      if (ofd.ShowDialog() != DialogResult.OK) return;
      Cursor.Current = Cursors.WaitCursor;
      for (int i = 0; i < ofd.FileNames.Length; i++)
      {
        lastOpenFilterIndex = ofd.FilterIndex;
        this.ProgressDisplayerInstance.DisplayToolStripMessage($"Load {ofd.FileNames[i]}");
        try
        {
          if (ofd.FileNames[i].ToLower().EndsWith("dxf"))
          {
            DxfParser.LoadDxfFile(ofd.FileNames[i]);
          }

          if (ofd.FileNames[i].ToLower().EndsWith("svg"))
          {
            SvgParser.LoadSvg(ofd.FileNames[i]);
          }

          var fr = Infos.FirstOrDefault(z => z.Path == ofd.FileNames[i]);
          if (fr != null)
          {
            fr.Quantity++;
          }
          else
          {
            var det = new DetailLoadInfo()
            {
              Quantity = 1,
              Name = new FileInfo(ofd.FileNames[i]).Name,
              Path = ofd.FileNames[i],
              IsIncluded = true,
              IsPriority = false,
              IsMultiplied = true,
              StrictAngle = AnglesEnum.None,
            };

            if (new FileInfo(ofd.FileNames[i]).Name.Contains("FrontLowerSectionL") ||
                new FileInfo(ofd.FileNames[i]).Name.Contains("SideConnection"))
            {
              det.Quantity = 2;
            }
            else if (new FileInfo(ofd.FileNames[i]).Name.Contains("SwitchBack"))
            {
              det.Quantity = 3;
              det.StrictAngle = AnglesEnum.Vertical;
              det.IsPriority = true;
            }

            Infos.Add(det);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show($"{ofd.FileNames[i]}: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }

      UpdateInfos();
      this.ProgressDisplayerInstance.DisplayToolStripMessage(string.Empty);
      Cursor.Current = Cursors.Default;
    }

    private void UpdateInfos()
    {
      partsList.SetObjects(Infos);
    }

    public List<DetailLoadInfo> Infos = new List<DetailLoadInfo>();

    private void runNestingButton_Click(object sender, EventArgs e)
    {
      Cursor.Current = Cursors.WaitCursor;
      Context.Reset();
      UpdateNestsList();
      Application.DoEvents();

      int src = 0;
      foreach (var item in sheetsInfos)
      {
        src = Context.GetNextSheetSource();
        for (int i = 0; i < item.Quantity; i++)
        {
          var ns = NewSheet(item.Width, item.Height);
          sheets.Add(ns);
          ns.Source = src;
        }
      }

      Context.ReorderSheets();

      src = 0;
      foreach (var item in Infos.Where(o => o.IsIncluded))
      {
        this.ProgressDisplayerInstance.DisplayToolStripMessage($"Preload {item.Path}. . .");
        var det = LoadRawDetail(new FileInfo(item.Path));

        AddToPolygons(src, det, item.Quantity, isPriority: item.IsPriority, isMultiplied: item.IsMultiplied, strictAngles: item.StrictAngle);

        src++;
      }

      this.ProgressDisplayerInstance.DisplayToolStripMessage(string.Empty);
      if (src == 0)
      {
        MessageBox.Show("No parts to nest.", "DeepNest", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      else
      {
        run();
      }
    }

    private void AddToPolygons(int src, RawDetail det, int quantity, bool isIncluded = true, bool isPriority = false, bool isMultiplied = false, AnglesEnum strictAngles = AnglesEnum.Vertical)
    {
      var item = new DetailLoadInfo() { Quantity = quantity, IsIncluded = isIncluded, IsPriority = isPriority, IsMultiplied = isMultiplied, StrictAngle = strictAngles };
      AddToPolygons(src, det, item);
    }

    private void AddToPolygons(int src, RawDetail det, DetailLoadInfo item)
    {
      INfp loadedNfp;
      if (det.TryGetNfp(src, out loadedNfp))
      {
        loadedNfp.IsPriority = item.IsPriority;
        loadedNfp.StrictAngle = item.StrictAngle;
        var quantity = item.Quantity * (item.IsMultiplied ? SvgNest.Config.Multiplier : 1);
        for (int i = 0; i < quantity; i++)
        {
          Context.Polygons.Add(loadedNfp.Clone());
        }
      }
      else
      {
        MessageBox.Show($"Failed to import {det.Name}.");
      }
    }

    private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (partsList.SelectedObject == null) return;
      Cursor.Current = Cursors.WaitCursor;
      preview = LoadRawDetail(new FileInfo((partsList.SelectedObject as DetailLoadInfo).Path));
      if (autoFit) fitAll();
      Cursor.Current = Cursors.Default;
    }

    private void ShowMessage(Exception ex)
    {
      errorMessageCount++;
      string message = ex.Message + "/r" + ex.GetType().Name + "/r" + ex.StackTrace;

      if (errorMessageCount <= 3)
      {
        this.ShowMessage(message, MessageBoxIcon.Error);
      }
      else if (errorMessageCount > 10)
      {
        this.ShowMessage(message, MessageBoxIcon.Stop);
        Application.Exit();
      }
    }

    private void ShowMessage(string text, MessageBoxIcon type)
    {
      MessageBox.Show(text, Text, MessageBoxButtons.OK, type);
    }

    private DialogResult ShowQuestion(string text)
    {
      return MessageBox.Show(text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
    }

    private void clearToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (Infos.Count == 0) { ShowMessage("There are no parts.", MessageBoxIcon.Warning); return; }
      if (ShowQuestion("Are you to sure to delete all items?") == DialogResult.No) return;
      Infos.Clear();
      partsList.SetObjects(Infos);
      preview = null;
    }

    List<SheetLoadInfo> sheetsInfos = new List<SheetLoadInfo>();

    private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
    {
      if (partsList.SelectedObjects.Count == 0) return;
      if (ShowQuestion($"Are you to sure to delete {partsList.SelectedObjects.Count} items?") == DialogResult.No) return;
      foreach (var item in partsList.SelectedObjects)
      {
        if (preview != null && (item as DetailLoadInfo).Path == (preview as RawDetail).Name)
        {
          preview = null;
        }

        Infos.Remove(item as DetailLoadInfo);
      }

      partsList.SetObjects(Infos);
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      Process.Start(linkLabel1.Text);
    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void toolStripButton8_Click(object sender, EventArgs e)
    {
      fitAll();
    }

    void fitAll()
    {
      if (preview == null) return;
      if (!(preview is RawDetail raw)) return;

      GraphicsPath gp = new GraphicsPath();
      foreach (var item in raw.Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      if (raw.Outers.Count > 0)
      {
        ctx3.FitToPoints(gp.PathPoints, 5);
      }
    }

    bool autoFit = true;

    private void toolStripButton9_CheckedChanged(object sender, EventArgs e)
    {
      autoFit = toolStripButton9.Checked;
      if (autoFit)
      {
        fitAll();
        toolStripButton9.BackColor = Color.LightGreen;
      }
      else
      {
        toolStripButton9.BackColor = Color.Transparent;
      }
    }

    private void textBox6_TextChanged(object sender, EventArgs e)
    {
      try
      {
        SvgNest.Config.CurveTolerance = double.Parse(textBox6.Text.Replace(",", "."), CultureInfo.InvariantCulture);
        textBox6.BackColor = Color.White;
        textBox6.ForeColor = Color.Black;
      }
      catch
      {
        textBox6.BackColor = Color.Red;
        textBox6.ForeColor = Color.White;
      }
    }

    private void setToToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (partsList.SelectedObjects.Count == 0) return;
      QntDialog q = new QntDialog();
      q.ShowDialog();

      foreach (var item in partsList.SelectedObjects)
      {
        (item as DetailLoadInfo).Quantity = q.Qnt;
      }

      partsList.RefreshObjects(partsList.SelectedObjects);
    }

    private void multiplyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (partsList.SelectedObjects.Count == 0) return;
      QntDialog q = new QntDialog();
      q.ShowDialog();

      foreach (var item in partsList.SelectedObjects)
      {
        (item as DetailLoadInfo).Quantity *= q.Qnt;
      }
      partsList.RefreshObjects(partsList.SelectedObjects);
    }

    private void divideToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (partsList.SelectedObjects.Count == 0) return;
      QntDialog q = new QntDialog();
      q.ShowDialog();
      if (q.Qnt == 0) return;
      foreach (var item in partsList.SelectedObjects)
      {
        (item as DetailLoadInfo).Quantity /= q.Qnt;
      }

      partsList.RefreshObjects(partsList.SelectedObjects);
    }

    private void checkBox5_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.DrawSimplification = checkBox5.Checked;
    }

    private void checkBox6_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.ClipByHull = checkBox6.Checked;
    }

    private void strictAnglesCheckbox_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.StrictAngles = (AnglesEnum)strictAnglesCombo.SelectedValue;
    }

    private void multiplierUpDown_ValueChanged(object sender, EventArgs e)
    {
      SvgNest.Config.Multiplier = (int)multiplierUpDown.Value;
    }

    private void showPartPositions_CheckedChanged(object sender, EventArgs e)
    {
      SvgNest.Config.ShowPartPositions = showPartPositions.Checked;
    }

    private void listView4_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == 'e')
      {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Filter = "Json files (*.json)|*.json";
        if (sfd.ShowDialog() == DialogResult.OK)
        {
          using (StreamWriter outputFile = new StreamWriter(sfd.FileName))
          {
            outputFile.WriteLine(this.context.Current.UsedSheets.First().ToJson());
          }
        }
      }
    }

    private void strictAnglesCombo_SelectedValueChanged(object sender, EventArgs e)
    {
      AnglesEnum strictAngle;
      if (Enum.TryParse<AnglesEnum>(strictAnglesCombo.SelectedItem as string, out strictAngle))
      {
        SvgNest.Config.StrictAngles = strictAngle;
      }
      else
      {
        SvgNest.Config.StrictAngles = AnglesEnum.None;
        ShowMessage("Defaulted to AnglesEnum.None", MessageBoxIcon.Information);
      }
    }
  }
}