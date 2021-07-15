namespace DeepNestSharp
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
  using DeepNestLib.Ui;
  using DeepNestPort;

  public partial class Form1 : Form
  {
    public static IDeepNestState DeepNestState;

    private static volatile object contextSyncLock = new object();

    private MessageBoxService messageBoxService = MessageBoxService.Default;

    private NestingContext context;
    private MessageFilter mf = null;
    private bool isInfoShow = false;
    private PictureBoxProgressBar progressBar1;
    private object selected = null;
    private Thread dth;
    private object preview;

    public Form1()
    {
      InitializeComponent();

      this.ProgressDisplayerInstance = new ProgressDisplayer(this);
      this.ContextualiseRunStopButtons(false);

      LoadSettings();
      
      ctx = new DrawingContext(nestPreview);

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
      this.parallelNestsNud.Minimum = SvgNestConfig.ParallelNestsMin;
      this.parallelNestsNud.Maximum = SvgNestConfig.ParallelNestsMax;
      this.parallelNestsNud.Value = SvgNest.Config.ParallelNests;

      this.placementTypeCombo.SelectedItem = SvgNest.Config.PlacementType.ToString();
      this.textBox1.Text = SvgNest.Config.Spacing.ToString();
      this.textBox6.Text = SvgNest.Config.CurveTolerance.ToString();

      this.checkBox5.Checked = SvgNest.Config.DrawSimplification;
      this.checkBox6.Checked = SvgNest.Config.ClipByHull;

      this.showPartPositions.Checked = SvgNest.Config.ShowPartPositions;

      this.strictAnglesCombo.Items.AddRange(Enum.GetNames(typeof(AnglesEnum)));
      this.strictAnglesCombo.SelectedItem = SvgNest.Config.StrictAngles.ToString();

      Form1.DeepNestState = new DeepNestState(new List<DetailLoadInfo>(), new List<ISheetLoadInfo>(), Context);

      Load += Form1_Load;
    }

    private ProgressDisplayer ProgressDisplayerInstance { get; }

    public NestingContext Context
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

      //set
      //{
      //  lock (contextSyncLock)
      //  {
      //    this.context = value;
      //  }
      //}
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      mf = new MessageFilter();
      Application.AddMessageFilter(mf);
    }

    private void LoadSettings()
    {
      if (!System.IO.File.Exists("settings.xml"))
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

    private void Redraw()
    {
      try
      {
        var pos = nestPreview.PointToClient(Cursor.Position);
        var pos1 = ctx.GetPos();
        var posx = pos1.X;
        var posy = pos1.Y;
        ctx.Update();

        partsList1.RedrawPreview();

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
            if (SvgNest.Config.UseParallel)
            {
              ctx.gr.DrawString($"Generations: {SvgNest.Generations}    Population: {SvgNest.Population}    Threads: {SvgNest.Threads}", Font, Brushes.DarkBlue, 0, yy);
            }
            else
            {
              ctx.gr.DrawString($"Generations: {SvgNest.Generations}    Population: {SvgNest.Population}", Font, Brushes.DarkBlue, 0, yy);
            }
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

        int i = 0;
        foreach (var item in Polygons.Union(sheets))
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
            m.Translate((float)item.x, (float)item.y);
            m.Rotate(item.Rotation);

            var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
            m.TransformPoints(pnts);

            path.AddPolygon(pnts.Select(z => ctx.Transform(z)).ToArray());

            if (!(item is Sheet) && isInfoShow && SvgNest.Config.ShowPartPositions)
            {
              var label = $"{item.PlacementOrder} ({item.x:N0},{item.y:N0})@{item.Rotation}";
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
        messageBoxService.ShowMessage(ex);
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

    private void pictureBox1_MouseEnter(object sender, EventArgs e)
    {
      //pictureBox1.Focus();
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
      partsList1.clearToolStripMenuItem_Click(sender, e);
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

    bool stop = false;

    
    Random r = new Random();

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
            nestResults1.UpdateNestsList();

            while (!this.stop)
            {
              Stopwatch sw = new Stopwatch();
              sw.Start();
              Cursor.Current = Cursors.Default;
              this.Context.NestIterate(SvgNest.Config);
              nestResults1.UpdateNestsList();
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
      if (IsHandleCreated)
      {
        if (InvokeRequired)
        {
          _ = this.Invoke((MethodInvoker)(() => ContextualiseRunStopButtons(isRunning)));
        }
        else
        {
          runButton.Enabled = !isRunning;
          this.startNestToolStripMenuItem.Enabled = !isRunning;
          this.stopNestToolStripMenuItem.Enabled = isRunning;
          this.stopButton.Enabled = isRunning;
          Application.DoEvents();
        }
      }
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

    private int GetCountFromDialog()
    {
      QntDialog q = new DeepNestSharp.QntDialog();
      if (q.ShowDialog() == DialogResult.OK)
      {
        return q.Qnt;
      }
      return 0;
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
        messageBoxService.ShowMessage(ex);
      }
    }

    internal ICollection<INfp> Polygons { get { return Context.Polygons; } }

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

          var fr = DeepNestState.PartInfos.FirstOrDefault(z => z.Path == ofd.FileNames[i]);
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
            else if (new FileInfo(ofd.FileNames[i]).Name.Contains("HalfCrossConnector"))
            {
              det.Quantity = 18;
              det.IsMultiplied = false;
            }
            else if (new FileInfo(ofd.FileNames[i]).Name.Contains("LowerSectionR"))
            {
              det.Quantity = 11;
              det.IsMultiplied = false;
            }
            else if (new FileInfo(ofd.FileNames[i]).Name.Contains("CrossConnector"))
            {
              det.Quantity = 4;
              det.IsMultiplied = false;
            }

            DeepNestState.PartInfos.Add(det);
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show($"{ofd.FileNames[i]}: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }

      partsList1.UpdateInfos();
      this.ProgressDisplayerInstance.DisplayToolStripMessage(string.Empty);
      Cursor.Current = Cursors.Default;
    }


    private void runNestingButton_Click(object sender, EventArgs e)
    {
      Cursor.Current = Cursors.WaitCursor;
      Context.Reset();
      UpdateNestsList();
      Application.DoEvents();

      int src = 0;
      foreach (var item in DeepNestState.SheetInfos)
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
      foreach (var item in DeepNestState.PartInfos.Where(o => o.IsIncluded))
      {
        this.ProgressDisplayerInstance.DisplayToolStripMessage($"Preload {item.Path}. . .");
        var det = FileService.Default.LoadRawDetail(new FileInfo(item.Path));

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

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {

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
        messageBoxService.ShowMessage("Defaulted to AnglesEnum.None", MessageBoxIcon.Information);
      }
    }

    private void parallelNestsNud_ValueChanged(object sender, EventArgs e)
    {
      SvgNest.Config.ParallelNests = (int)parallelNestsNud.Value;
    }

    private void deepNestToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetActiveView(UiTab.About);
    }

    private void deepNestPortToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetActiveView(UiTab.About);
    }

    private void SetActiveView(UiTab uiTab)
    {
      tabControl1.Visible = uiTab == UiTab.Settings || uiTab == UiTab.Debug || uiTab == UiTab.Nest;
      partsList1.Visible = uiTab == UiTab.Input;
      about.Visible = uiTab == UiTab.About;
    }

    private void deepNestToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      SetActiveView(UiTab.About);
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Context?.StopNest();
      Application.Exit();
    }

    private void nestToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
    {
      SetActiveView(UiTab.Nest);
    }

    private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      SetActiveView(UiTab.Settings);
    }

    private void savePartsListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var deepNestState = new DeepNestLib.Ui.DeepNestState()
      {
        Polygons = this.Polygons.ToList(),
        Sheets = this.sheets.ToList(),
        NestResults = this.Context.Nest.TopNestResults,
        PartInfos = DeepNestState.PartInfos,
        SheetInfos = DeepNestState.SheetInfos.Select(o => new SheetLoadInfoDto() { Height = o.Height, Quantity = o.Quantity, Width = o.Width }).ToList<ISheetLoadInfo>(),
      };

      SaveFileDialog sfd = new SaveFileDialog();
      sfd.Filter = "DeepNest files (*.deepnest)|*.deepnest";
      if (sfd.ShowDialog() == DialogResult.OK)
      {
        using (StreamWriter outputFile = new StreamWriter(sfd.FileName))
        {
          outputFile.WriteLine(deepNestState.ToJson());
        }
      }
    }

    private void loadPartsListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OpenFileDialog dialog = new OpenFileDialog();
      dialog.Filter = "DeepNest files (*.deepnest)|*.deepnest";
      if (dialog.ShowDialog() == DialogResult.OK)
      {
        using (StreamReader inputFile = new StreamReader(dialog.FileName))
        {
          var state = DeepNestLib.Ui.DeepNestState.FromJson(inputFile.ReadToEnd());

          this.Polygons.Clear();
          state.Polygons.ToList().ForEach(o => this.Polygons.Add(o));
          this.sheets.Clear();
          state.Sheets.ForEach(o => this.sheets.Add(o));
          DeepNestState.SheetInfos.Clear();
          state.SheetInfos.ForEach(o => DeepNestState.SheetInfos.Add(new SheetLoadInfo() { Height = o.Height, Quantity = o.Quantity, Width = o.Width }));
          DeepNestState.PartInfos.Clear();
          state.PartInfos.ForEach(o => DeepNestState.PartInfos.Add(o));
        }
      }
    }

    private void partsToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
    {
      SetActiveView(UiTab.Input);
    }
  }
}