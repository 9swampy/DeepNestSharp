namespace DeepNestPort
{
    using System;
    using System.Collections.Generic;
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

    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();

            this.LoadSettings();
            this.sheetsInfos.Add(new SheetLoadInfo() { Width = 3000, Height = 1500, Quantity = 10 });

            //hack
            toolStripButton9.BackgroundImageLayout = ImageLayout.None;
            toolStripButton9.BackgroundImage = new Bitmap(1, 1);
            toolStripButton9.BackColor = Color.LightGreen;

            objectListView2.SetObjects(sheetsInfos);

            this.ctx = new DrawingContext(this.pictureBox1);
            this.ctx2 = new DrawingContext(this.pictureBox2);
            this.ctx3 = new DrawingContext(this.pictureBox3);

            this.listView1.DoubleBuffered(true);
            this.listView2.DoubleBuffered(true);
            this.listView3.DoubleBuffered(true);
            this.listView4.DoubleBuffered(true);
            this.progressBar1 = new PictureBoxProgressBar();
            this.progressBar1.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(this.progressBar1);
            ctx3.FocusOnMove = false;
            ctx2.FocusOnMove = false;

            this.checkBox2.Checked = SvgNest.Config.Simplify;
            this.checkBox4.Checked = Background.UseParallel;
            this.numericUpDown1.Value = SvgNest.Config.PopulationSize;
            this.numericUpDown2.Value = SvgNest.Config.MutationRate;
            this.comboBox1.SelectedItem = SvgNest.Config.PlacementType.ToString();
            this.textBox1.Text = SvgNest.Config.Spacing.ToString();

            this.UpdateFilesList(@"dxfs");
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        MessageFilter mf = null;

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

        private PictureBoxProgressBar progressBar1;

        public void UpdateList()
        {
            this.listView1.Items.Clear();
            foreach (var item in this.polygons)
            {
                this.listView1.Items.Add(new ListViewItem(new string[] { item.Id.ToString(), item.Source.ToString(), item.Name, item.Points.Count().ToString() }) { Tag = item });
            }

            this.listView2.Items.Clear();
            foreach (var item in this.sheets)
            {
                this.listView2.Items.Add(new ListViewItem(new string[] { item.Id.ToString(), item.Source.ToString(), item.Name, item.Points.Count().ToString() }) { Tag = item });
            }

            this.groupBox5.Text = "Parts: " + this.polygons.Count();
            this.groupBox6.Text = "Sheets: " + this.sheets.Count;
        }

        public NestingContext Context = new NestingContext();

        public SvgNest nest
        {
            get { return this.context.Nest; }
        }

        public object selected = null;

        private Thread dth;

        private object Preview;

        public void RedrawPreview(DrawingContext ctx2, object previewObject)
        {
            ctx2.Update();

            ctx2.gr.Clear(Color.White);

            //ctx2.gr.DrawLine(Pens.Blue, ctx2.Transform(new PointF(0, 0)), ctx2.Transform(100, 0));
            //ctx2.gr.DrawLine(Pens.Red, ctx2.Transform(new PointF(0, 0)), ctx2.Transform(0, 100));

            if (previewObject != null)
            {
                ctx2.gr.ResetTransform();
                GraphicsPath gp = new GraphicsPath();
                GraphicsPath gp2 = new GraphicsPath();

                if (previewObject is RawDetail raw)
                {
                    foreach (var item in raw.Outers)
                    {
                        gp.AddPolygon(item.Points.Select(z => ctx2.Transform(z)).ToArray());
                        gp2.AddPolygon(item.Points.ToArray());
                    }
                }

                if (previewObject is NFP nfp)
                {
                    gp.AddPolygon(nfp.Points.Select(z => ctx2.Transform(z.x, z.y)).ToArray());
                    if (nfp.children != null)
                    {
                        foreach (var item in nfp.children)
                        {
                            gp.AddPolygon(item.Points.Select(z => ctx2.Transform(z.x, z.y)).ToArray());
                        }
                    }
                }
                var bnd = gp2.GetBounds();

                ctx2.gr.FillPath(Brushes.LightBlue, gp);
                ctx2.gr.DrawPath(Pens.Black, gp);
                
                ctx2.gr.ResetTransform();
                var cap = $"{bnd.Width:N2} x {bnd.Height:N2}";
                var ms = ctx2.gr.MeasureString(cap, SystemFonts.DefaultFont);
                ctx2.gr.FillRectangle(new SolidBrush(Color.FromArgb(128, Color.LightGreen)), 5, 5, ms.Width, ms.Height);

                ctx2.gr.DrawString(cap, SystemFonts.DefaultFont, Brushes.Black, 5, 5);
            }

            ctx2.Setup();
        }


        public void Redraw()
        {
            var pos = this.pictureBox1.PointToClient(Cursor.Position);
            var pos1 = this.ctx.GetPos();
            var posx = pos1.X;
            var posy = pos1.Y;
            this.ctx.Update();

            // ctx2.Update();
            this.RedrawPreview(this.ctx2, this.Preview);
            this.RedrawPreview(this.ctx3, this.Preview);
            this.ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
            this.ctx.gr.Clear(Color.White);

            this.ctx.gr.ResetTransform();

            this.ctx.gr.DrawLine(Pens.Red, this.ctx.Transform(new PointF(0, 0)), this.ctx.Transform(new PointF(1000, 0)));
            this.ctx.gr.DrawLine(Pens.Blue, this.ctx.Transform(new PointF(0, 0)), this.ctx.Transform(new PointF(0, 1000)));
            int yy = 0;
            int gap = (int)this.Font.Size;
            if (this.isInfoShow)
            {
                this.ctx.gr.DrawString("X:" + posx.ToString("0.00") + " Y: " + posy.ToString("0.00"), this.Font, Brushes.Blue, 0, yy);
                yy += (int)this.Font.Size + gap;
                this.ctx.gr.DrawString($"Material Utilization: {Math.Round(this.context.MaterialUtilization * 100.0f, 2)}%   Iterations: {this.context.Iterations}    Parts placed: {this.context.PlacedPartsCount}/{this.polygons.Count}", this.Font, Brushes.DarkBlue, 0, yy);
                yy += (int)this.Font.Size + gap;
                this.ctx.gr.DrawString($"Sheets: {this.sheets.Count}   Parts:{this.polygons.Count}    parts types: {this.polygons.GroupBy(z => z.Source).Count()}", this.Font, Brushes.DarkBlue, 0, yy);
                yy += (int)this.Font.Size + gap;

                if (this.nest != null && this.nest.nests.Any())
                {
                    this.ctx.gr.DrawString($"Nests: {this.nest.nests.Count} Fitness: {this.nest.nests.First().fitness}   Area:{this.nest.nests.First().area}  ", this.Font, Brushes.DarkBlue, 0, yy);
                    yy += (int)this.Font.Size + gap;
                }

                this.ctx.gr.DrawString($"Call counter: {Background.callCounter};  Last placing time: {Background.LastPlacePartTime}ms", this.Font, Brushes.DarkBlue, 0, yy);
                yy += (int)this.Font.Size + gap;
            }
            else
            {
                this.ctx.gr.DrawString($"Iterations: {this.context.Iterations}    Parts placed: {this.context.PlacedPartsCount}/{this.polygons.Count}", this.Font, Brushes.DarkBlue, 0, yy);
                yy += (int)this.Font.Size + gap;
                this.ctx.gr.DrawString($"Sheets: {this.sheets.Count}   Parts:{this.polygons.Count}    Parts types: {this.polygons.GroupBy(z => z.Source).Count()}", this.Font, Brushes.DarkBlue, 0, yy);
                yy += (int)this.Font.Size + gap;
            }

            if (!this.checkBox1.Checked)
            {
                if (this.bb != null)
                {
                    // ctx.gr.TranslateTransform((float)sheets[0].x, (float)sheets[0].y);
                    var pp = this.ctx.Transform((float)this.sheets[0].x, (float)this.sheets[0].y);
                    this.ctx.gr.DrawImage(this.bb, new RectangleF(pp.X, pp.Y, this.bb.Width * this.ctx.zoom, this.bb.Height * this.ctx.zoom), new Rectangle(0, 0, this.bb.Width, this.bb.Height), GraphicsUnit.Pixel);
                }
            }

            foreach (var item in this.polygons.Union(this.sheets))
            {
                if (!this.checkBox1.Checked)
                {
                    continue;
                }

                if (!(item is Sheet))
                {
                    if (!item.fitted)
                    {
                        continue;
                    }
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

                    path.AddPolygon(pnts.Select(z => this.ctx.Transform(z)).ToArray());
                    if (item.children != null)
                    {
                        foreach (var citem in item.children)
                        {
                            var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                            m.TransformPoints(pnts2);
                            path.AddPolygon(pnts2.Select(z => this.ctx.Transform(z)).ToArray());
                        }
                    }

                    this.ctx.gr.ResetTransform();

                    /*if (selected == item)
                    {
                        ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.Orange)), path);
                        ctx.gr.DrawPath(Pens.DarkBlue, path);

                    }
                    else*/
                    {
                        if (!this.sheets.Contains(item))
                        {
                            this.ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
                        }

                        this.ctx.gr.DrawPath(Pens.Black, path);
                    }

                    if (item is Sheet)
                    {
                        if (this.nest != null && this.nest.nests.Any())
                        {
                            var fr = this.nest.nests.First();
                            double tot1 = 0;
                            double tot2 = 0;
                            bool was = false;
                            foreach (var zitem in fr.placements.First())
                            {
                                var sheetid = zitem.sheetId;
                                if (sheetid != item.Id)
                                {
                                    continue;
                                }

                                var sheet = this.sheets.FirstOrDefault(z => z.Id == sheetid);
                                if (sheet != null)
                                {
                                    tot1 += Math.Abs(GeometryUtil.polygonArea(sheet));
                                    was = true;
                                    foreach (var ssitem in zitem.sheetplacements)
                                    {
                                        var poly = this.polygons.FirstOrDefault(z => z.Id == ssitem.id);
                                        if (poly != null)
                                        {
                                            tot2 += Math.Abs(GeometryUtil.polygonArea(poly));
                                        }
                                    }
                                }
                            }

                            var res = Math.Abs(Math.Round(100.0 * (tot2 / tot1), 2));
                            var trans1 = this.ctx.Transform(new PointF((float)pnts[0].X, (float)pnts[0].Y - 30));
                            if (was && this.isInfoShow)
                            {
                                this.ctx.gr.DrawString("util: " + res + "%", this.Font, Brushes.Black, trans1);
                            }
                        }
                    }
                }
            }

            this.ctx.Setup();
        }

        public void RenderSheet()
        {
            this.ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
            this.ctx.gr.Clear(Color.White);

            this.ctx.gr.ResetTransform();

            foreach (var item in this.polygons.Union(this.sheets))
            {
                if (!(item is Sheet))
                {
                    if (!item.fitted)
                    {
                        continue;
                    }
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

                    path.AddPolygon(pnts.Select(z => this.ctx.Transform(z)).ToArray());
                    if (item.children != null)
                    {
                        foreach (var citem in item.children)
                        {
                            var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                            m.TransformPoints(pnts2);
                            path.AddPolygon(pnts2.Select(z => this.ctx.Transform(z)).ToArray());
                        }
                    }

                    this.ctx.gr.ResetTransform();

                    if (!this.sheets.Contains(item))
                    {
                        this.ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
                    }

                    this.ctx.gr.DrawPath(Pens.Black, path);
                }
            }
        }

        public void RedrawAsync()
        {
            if (this.dth != null)
            {
                return;
            }

            this.dth = new Thread(() =>
             {
                 this.Redraw();
                 this.dth = null;
             });
            this.dth.IsBackground = true;
            this.dth.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.progressBar1.UpdateImg();
            this.progressBar1.Value = (int)Math.Round(this.progressVal * 100f);
            this.Redraw();
        }

        public DrawingContext ctx;
        public DrawingContext ctx2;
        public DrawingContext ctx3;

        public void UpdateNestsList()
        {
            if (this.nest != null)
            {
                this.listView4.Invoke((Action)(() =>
                {
                    this.listView4.BeginUpdate();
                    this.listView4.Items.Clear();
                    foreach (var item in this.nest.nests)
                    {
                        this.listView4.Items.Add(new ListViewItem(new string[] { item.fitness + string.Empty }) { Tag = item });
                    }

                    this.listView4.EndUpdate();
                }));
            }
        }

        private Thread th;

        internal void displayProgress(float progress)
        {
            this.progressVal = progress;
        }

        public float progressVal = 0;

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                this.selected = this.listView1.SelectedItems[0].Tag;
                this.Preview = this.selected;
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            // this.pictureBox1.Focus();
        }

        public void UpdateFilesList(string path)
        {
            var di = new DirectoryInfo(path);
            this.groupBox3.Text = "Files: " + di.FullName;
            this.listView3.Items.Clear();
            this.listView3.Items.Add(new ListViewItem(new string[] { ".." }) { Tag = di.Parent, BackColor = Color.LightBlue });
            foreach (var item in di.GetDirectories())
            {
                this.listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item, BackColor = Color.LightBlue });
            }

            foreach (var item in di.GetFiles())
            {
                if (!(item.Extension.Contains("svg") || item.Extension.Contains("dxf")))
                {
                    continue;
                }

                this.listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
            }
        }

        public Sheet NewSheet(int w = 3000, int h = 1500)
        {
            var tt = new RectangleSheet();
            tt.Name = "rectSheet" + (this.sheets.Count + 1);
            tt.Height = h;
            tt.Width = w;
            tt.Rebuild();

            return tt;
        }

        public Sheet NewRhombusSheet(int w = 3000, int h = 1500)
        {
            var tt = new Sheet();
            tt.Name = "rhombSheet" + (this.sheets.Count + 1);

            tt.Height = h;
            tt.Width = w;
            tt.Points = new SvgPoint[] { };
            int x = 0;
            int y = 0;
            int _width = w;
            int _height = h;

            tt.AddPoint(new SvgPoint(x + (_width / 2), y));
            tt.AddPoint(new SvgPoint(x, y + (_height / 2)));
            tt.AddPoint(new SvgPoint(x + (_width / 2), y + _height));
            tt.AddPoint(new SvgPoint(x + _width, y + (_height / 2)));

            return tt;
        }

        public Sheet NewCircleSheet(int w = 3000)
        {
            var tt = new Sheet();
            tt.Name = "circleSheet" + (this.sheets.Count + 1);

            tt.Height = w;
            tt.Width = w;
            tt.Points = new SvgPoint[] { };
            int x = 0;
            int y = 0;

            for (int i = 0; i < 360; i += 5)
            {
                var xx = w / 2 * Math.Cos(i * Math.PI / 180.0f);
                var yy = w / 2 * Math.Sin(i * Math.PI / 180.0f);
                tt.AddPoint(new SvgPoint(xx + (w / 2), yy + (w / 2)));
            }

            return tt;
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.polygons.Clear();
            this.UpdateList();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                for (int i = 0; i < this.listView1.SelectedItems.Count; i++)
                {
                    this.polygons.Remove(this.listView1.SelectedItems[i].Tag as NFP);
                }

                this.UpdateList();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SvgNest.Config.Spacing = float.Parse(this.textBox1.Text, CultureInfo.InvariantCulture);
                this.textBox1.BackColor = Color.White;
                this.textBox1.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                this.textBox1.BackColor = Color.Red;
                this.textBox1.ForeColor = Color.White;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SvgNest.Config.SheetSpacing = float.Parse(this.textBox2.Text, CultureInfo.InvariantCulture);
                this.textBox2.BackColor = Color.White;
                this.textBox2.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                this.textBox2.BackColor = Color.Red;
                this.textBox2.ForeColor = Color.White;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SvgNest.Config.Rotations = int.Parse(this.textBox3.Text, CultureInfo.InvariantCulture);
                this.textBox3.BackColor = Color.White;
                this.textBox3.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                this.textBox3.BackColor = Color.Red;
                this.textBox3.ForeColor = Color.White;
            }
        }

        private void moveToSheetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                var pol = this.listView1.SelectedItems[0].Tag as NFP;
                this.polygons.Remove(pol);
                var b = GeometryUtil.getPolygonBounds(pol);
                Sheet sheet = new Sheet();
                sheet.Points = new SvgPoint[] { };
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

                sheet.Source = this.context.GetNextSheetSource();
                this.sheets.Add(sheet);
                this.context.ReorderSheets();
                this.UpdateList();
            }
        }

        private void clearAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.sheets.Clear();
            this.UpdateList();
        }

        private void moveToPolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView2.SelectedItems.Count > 0)
            {
                var pol = this.listView2.SelectedItems[0].Tag as NFP;
                this.sheets.Remove(pol);
                this.polygons.Add(pol);
                this.UpdateList();
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView2.SelectedItems.Count > 0)
            {
                this.selected = this.listView2.SelectedItems[0].Tag;
                this.Preview = this.selected;
            }
        }

        private void listView3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.listView3.SelectedItems.Count <= 0)
            {
                return;
            }

            var si = this.listView3.SelectedItems[0].Tag;
            if (si is DirectoryInfo)
            {
                this.UpdateFilesList((si as DirectoryInfo).FullName);
            }

            if (si is FileInfo)
            {
                var f = si as FileInfo;
                QntDialog q = new QntDialog();
                if (q.ShowDialog() == DialogResult.OK)
                {
                    RawDetail det = null;
                    if (f.Extension == ".svg")
                    {
                        det = SvgParser.LoadSvg(f.FullName);
                    }

                    if (f.Extension == ".dxf")
                    {
                        det = DxfParser.LoadDxf(f.FullName);
                    }

                    int src = 0;
                    if (this.polygons.Any())
                    {
                        src = this.polygons.Max(z => z.Source.Value) + 1;
                    }

                    for (int i = 0; i < q.Qnt; i++)
                    {
                        this.context.ImportFromRawDetail(det, src);
                    }

                    this.UpdateList();
                }
            }
        }

        private void importSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView3.SelectedItems.Count <= 0)
            {
                return;
            }

            QntDialog q = new QntDialog();
            if (q.ShowDialog() == DialogResult.OK)
            {
                foreach (var item in this.listView3.SelectedItems)
                {
                    var t = (item as ListViewItem).Tag as FileInfo;
                    RawDetail det = null;
                    if (t.Extension == ".svg")
                    {
                        det = SvgParser.LoadSvg(t.FullName);
                    }

                    if (t.Extension == ".dxf")
                    {
                        det = DxfParser.LoadDxf(t.FullName);
                    }

                    int src = 0;
                    if (this.polygons.Any())
                    {
                        src = this.polygons.Max(z => z.Source.Value) + 1;
                    }

                    for (int i = 0; i < q.Qnt; i++)
                    {
                        this.context.ImportFromRawDetail(det, src);
                    }
                }

                this.UpdateList();
            }
        }

        private bool stop = false;

        public void RunDeepnest()
        {
            if (this.th != null)
            {
                return;
            }

            this.th = new Thread(() =>
            {
                this.context.StartNest();
                this.UpdateNestsList();
                Background.displayProgress = this.displayProgress;

                while (true)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    this.context.NestIterate();
                    this.UpdateNestsList();
                    this.displayProgress(1.0f);
                    sw.Stop();
                    this.toolStripStatusLabel1.Text = "Nesting time: " + sw.ElapsedMilliseconds + "ms";
                    if (this.stop)
                    {
                        break;
                    }
                }

                this.th = null;
            });
            this.th.IsBackground = true;
            this.th.Start();
        }

        private Random r = new Random();

        private void cloneQntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                QntDialog qd = new QntDialog();
                if (qd.ShowDialog() == DialogResult.OK)
                {
                    var nfp = this.listView1.SelectedItems[0].Tag as NFP;
                    for (int i = 0; i < qd.Qnt; i++)
                    {
                        var r = Background.clone(nfp);
                        this.polygons.Add(r);
                    }

                    this.UpdateList();
                }
            }
        }

        private void run()
        {
            if (this.sheets.Count == 0 || this.polygons.Count == 0)
            {
                MessageBox.Show("There are no sheets or parts", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.stop = false;
            this.progressBar1.Value = 0;
            this.tabControl1.SelectedTab = this.tabPage4;
            this.context.ReorderSheets();
            this.RunDeepnest();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.run();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SvgNest.Config.Simplify = this.checkBox2.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Background.UseParallel = this.checkBox4.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = this.comboBox1.SelectedItem as string;
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
            SvgNest.Config.PopulationSize = (int)this.numericUpDown1.Value;
        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView4.SelectedItems.Count > 0)
            {
                var shp = this.listView4.SelectedItems[0].Tag as SheetPlacement;
                this.context.AssignPlacement(shp);
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            SvgNest.Config.MutationRate = (int)this.numericUpDown2.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var cnt = (int)this.numericUpDown3.Value;
            int? ww = null;
            int? hh = null;

            try
            {
                ww = int.Parse(this.textBox4.Text);
                this.textBox4.BackColor = Color.White;
                this.textBox4.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                this.textBox4.BackColor = Color.Red;
                this.textBox4.ForeColor = Color.White;
            }

            try
            {
                hh = int.Parse(this.textBox5.Text);
                this.textBox5.BackColor = Color.White;
                this.textBox5.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                this.textBox5.BackColor = Color.Red;
                this.textBox5.ForeColor = Color.White;
            }

            if (ww == null || hh == null)
            {
                MessageBox.Show("Wrong sizes", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.comboBox2.SelectedItem == null)
            {
                this.label11.BackColor = Color.Red;
                this.label11.ForeColor = Color.White;
                return;
            }

            this.label11.BackColor = this.label11.Parent.BackColor;
            this.label11.ForeColor = this.label11.Parent.ForeColor;
            List<Sheet> sh = new List<Sheet>();
            var src = this.context.GetNextSheetSource();
            for (int i = 0; i < cnt; i++)
            {
                switch (this.comboBox2.SelectedItem.ToString())
                {
                    case "Rectangle":
                        sh.Add(this.NewSheet(ww.Value, hh.Value));
                        break;
                    case "Rhombus":
                        sh.Add(this.NewRhombusSheet(ww.Value, hh.Value));
                        break;
                    case "Circle":
                        sh.Add(this.NewCircleSheet(ww.Value));
                        break;
                }
            }

            foreach (var item in sh)
            {
                item.Source = src;
                this.context.Sheets.Add(item);
            }

            this.UpdateList();
            this.context.ReorderSheets();
        }

        public int GetCountFromDialog()
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
            var cnt = this.GetCountFromDialog();
            Random r = new Random();
            for (int i = 0; i < cnt; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var ww = r.Next(60) + 10;
                var hh = r.Next(60) + 5;
                NFP pl = new NFP();
                int src = 0;
                if (this.polygons.Any())
                {
                    src = this.polygons.Max(z => z.Source.Value) + 1;
                }

                this.polygons.Add(pl);
                pl.Source = src;
                pl.x = xx;
                pl.y = yy;
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(0, 0));
                pl.AddPoint(new SvgPoint(ww, 0));
                pl.AddPoint(new SvgPoint(ww, hh));
                pl.AddPoint(new SvgPoint(0, hh));
            }

            this.UpdateList();
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
                if (this.polygons.Any())
                {
                    src = this.polygons.Max(z => z.Source.Value) + 1;
                }

                pl.Source = src;
                this.polygons.Add(pl);
                pl.x = xx;
                pl.y = yy;
                pl.Points = new SvgPoint[] { };
                for (int ang = 0; ang < 360; ang += 15)
                {
                    var xx1 = (float)(rad * Math.Cos(ang * Math.PI / 180.0f));
                    var yy1 = (float)(rad * Math.Sin(ang * Math.PI / 180.0f));
                    pl.AddPoint(new SvgPoint(xx1, yy1));
                }
            }

            this.UpdateList();
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
                if (this.polygons.Any())
                {
                    src = this.polygons.Max(z => z.Source.Value) + 1;
                }

                pl.Source = src;
                this.polygons.Add(pl);
                pl.Points = new SvgPoint[] { };
                pl.x = xx;
                pl.y = yy;
                pl.AddPoint(new SvgPoint(-ww, 0));
                pl.AddPoint(new SvgPoint(+ww, 0));
                pl.AddPoint(new SvgPoint(0, +hh));
            }

            this.UpdateList();
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
                if (this.polygons.Any())
                {
                    src = this.polygons.Max(z => z.Source.Value) + 1;
                }

                pl.Source = src;
                this.polygons.Add(pl);
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(xx, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
                pl.AddPoint(new SvgPoint(xx, yy + hh));
            }

            this.UpdateList();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var ww = this.r.Next(400) + 10;
            var hh = this.r.Next(400) + 5;
            QntDialog q = new QntDialog();
            int src = 0;
            if (this.polygons.Any())
            {
                src = this.polygons.Max(z => z.Source.Value) + 1;
            }

            if (q.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < q.Qnt; i++)
                {
                    var xx = this.r.Next(2000) + 100;
                    var yy = this.r.Next(2000);

                    NFP pl = new NFP();

                    pl.Source = src;
                    this.polygons.Add(pl);
                    pl.Points = new SvgPoint[] { };
                    pl.x = xx;
                    pl.y = yy;
                    pl.AddPoint(new SvgPoint(0, 0));
                    pl.AddPoint(new SvgPoint(0 + ww, 0));
                    pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
                    pl.AddPoint(new SvgPoint(0, 0 + hh));
                }

                this.UpdateList();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.stop = true;
        }

        private NestingContext context = new NestingContext();

        private List<NFP> polygons
        {
            get { return this.context.Polygons; }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var cnt = this.GetCountFromDialog();
            Random r = new Random();
            for (int i = 0; i < cnt; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var ww = r.Next(250) + 150;
                var hh = r.Next(250) + 120;
                NFP pl = new NFP();
                int src = 0;
                if (this.polygons.Any())
                {
                    src = this.polygons.Max(z => z.Source.Value) + 1;
                }

                this.polygons.Add(pl);
                pl.Source = src;
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(0, 0));
                pl.AddPoint(new SvgPoint(0 + ww, 0));
                pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
                pl.AddPoint(new SvgPoint(0, 0 + hh));
                pl.x = xx;
                pl.y = yy;
                var hole = new NFP();

                pl.children = new List<NFP>();
                pl.children.Add(hole);
                hole.Points = new SvgPoint[] { };
                int gap = 10;
                hole.AddPoint(new SvgPoint(0 + gap, 0 + gap));
                hole.AddPoint(new SvgPoint(0 + ww - gap, 0 + gap));
                hole.AddPoint(new SvgPoint(0 + ww - gap, 0 + hh - gap));
                hole.AddPoint(new SvgPoint(0 + gap, 0 + hh - gap));
                hole.x = xx;
                hole.y = yy;
            }

            this.UpdateList();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                var xx = this.r.Next(2000) + 100;
                var yy = this.r.Next(2000);
                var rad = this.r.Next(60) + 10;
                int rad2 = rad - 8;

                NFP pl = new NFP();
                int src = 0;
                if (this.polygons.Any())
                {
                    src = this.polygons.Max(z => z.Source.Value) + 1;
                }

                pl.Source = src;
                this.polygons.Add(pl);
                pl.Points = new SvgPoint[] { };

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

                pl.children = new List<NFP>();
                pl.children.Add(hole);
                pl.x = xx;
                pl.y = yy;
            }

            this.UpdateList();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.listView2.SelectedItems.Count > 0)
            {
                var f = this.listView2.SelectedItems[0].Tag as NFP;
                this.sheets.Remove(f);
                this.UpdateList();
                this.context.ReorderSheets();
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
            if (this.polygons.Any())
            {
                src = this.polygons.Max(z => z.Source.Value) + 1;
            }

            this.polygons.Add(pl);
            pl.Source = src;
            pl.x = xx;
            pl.y = yy;
            pl.Points = new SvgPoint[] { };
            pl.AddPoint(new SvgPoint(0, 0));
            pl.AddPoint(new SvgPoint(ww, 0));
            pl.AddPoint(new SvgPoint(ww, hh));
            pl.AddPoint(new SvgPoint(0, hh));

            this.UpdateList();
        }

        private bool isInfoShow = false;

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.isInfoShow = !this.isInfoShow;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Font = new Font(this.Font.FontFamily.Name, this.Font.Size + 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Font = new Font(this.Font.FontFamily.Name, this.Font.Size - 1);
        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(this.listView3.SelectedItems.Count > 0 && this.listView3.SelectedItems[0].Tag is FileInfo))
            {
                return;
            }

            try
            {
                var path = (FileInfo)this.listView3.SelectedItems[0].Tag;
                RawDetail det = null;
                if (path.Extension == ".svg")
                {
                    det = SvgParser.LoadSvg(path.FullName);
                }

                if (path.Extension == ".dxf")
                {
                    det = DxfParser.LoadDxf(path.FullName);
                }

                this.Preview = det;
            }
            catch (Exception ex)
            {
                this.Preview = null;
            }
        }

        private List<NFP> sheets
        {
            get { return this.context.Sheets; }
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Svgs files (*.svg)|*.svg";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SvgParser.Export(sfd.FileName, this.polygons.ToArray(), this.sheets.ToArray());
            }
        }

        private void listView3_MouseMove(object sender, MouseEventArgs e)
        {
            var ch = this.listView3.GetChildAtPoint(this.listView3.PointToClient(Cursor.Position));

            if (ch != null)
            {
            }
        }

        private Bitmap bb;

        private void button7_Click(object sender, EventArgs e)
        {
            var sh = this.sheets[0] as Sheet;
            this.ctx.sx = (float)sh.x;
            this.ctx.sy = (float)sh.y;
            this.ctx.InvertY = false;
            this.bb = new Bitmap(1 + (int)sh.Width, 1 + (int)sh.Height);
            var gr = Graphics.FromImage(this.bb);
            var tempgr = this.ctx.gr;
            var tmpbmp = this.ctx.bmp;
            this.ctx.gr = gr;
            this.ctx.bmp = this.bb;

            this.RenderSheet();
            this.ctx.gr = gr;
            this.ctx.bmp = tmpbmp;
            this.ctx.InvertY = true;

            // bb = ctx.bmp.Clone(new Rectangle(0, 0, ctx.bmp.Width, ctx.bmp.Height), ctx.bmp.PixelFormat);
            Clipboard.SetImage(this.bb);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var xx = this.r.Next(2000) + 100;
            var yy = this.r.Next(2000);
            var ww = this.r.Next(250) + 150;
            var hh = this.r.Next(250) + 120;
            NFP pl = new NFP();
            int src = 0;
            if (this.polygons.Any())
            {
                src = this.polygons.Max(z => z.Source.Value) + 1;
            }

            this.polygons.Add(pl);
            pl.Source = src;
            pl.Points = new SvgPoint[] { };
            pl.AddPoint(new SvgPoint(0, 0));
            pl.AddPoint(new SvgPoint(0 + ww, 0));
            pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
            pl.AddPoint(new SvgPoint(0, 0 + hh));
            pl.x = xx;
            pl.y = yy;
            pl.children = new List<NFP>();
            int gap = 10;
            int szx = ww / 4;
            int szy = hh / 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var hole = new NFP();

                    pl.children.Add(hole);
                    hole.Points = new SvgPoint[] { };

                    int hx = (i * ww / 4) + (gap * (i + 1));
                    int hy = (j * hh / 3) + (gap * (j + 1));

                    hole.AddPoint(new SvgPoint(hx + szx, hy + szy));
                    hole.AddPoint(new SvgPoint(hx, hy + szy));
                    hole.AddPoint(new SvgPoint(hx, hy));
                    hole.AddPoint(new SvgPoint(hx + szx, hy));
                    hole.x = xx;
                    hole.y = yy;
                }
            }

            this.UpdateList();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.run();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "DXF files (*.dxf)|*.dxf";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                try
                {
                    DxfParser.LoadDxf(ofd.FileNames[i]);
                    var fr = this.Infos.FirstOrDefault(z => z.Path == ofd.FileNames[i]);
                    if (fr != null)
                    {
                        fr.Quantity++;
                    }
                    else
                    {
                        this.Infos.Add(new DetailLoadInfo() { Quantity = 1, Name = new FileInfo(ofd.FileNames[i]).Name, Path = ofd.FileNames[i] });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ofd.FileNames[i]}: {ex.Message}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            this.UpdateInfos();
        }

        public void UpdateInfos()
        {
            this.objectListView1.SetObjects(this.Infos);
        }

        public List<DetailLoadInfo> Infos = new List<DetailLoadInfo>();


        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            this.context = new NestingContext();
            int src = 0;
            foreach (var item in this.sheetsInfos)
            {
                src = this.context.GetNextSheetSource();
                for (int i = 0; i < item.Quantity; i++)
                {
                    var ns = this.NewSheet(item.Width, item.Height);
                    this.sheets.Add(ns);
                    ns.Source = src;
                }
            }

            this.context.ReorderSheets();

            src = 0;
            foreach (var item in this.Infos)
            {
                var det = DxfParser.LoadDxf(item.Path);

                for (int i = 0; i < item.Quantity; i++)
                {
                    this.context.ImportFromRawDetail(det, src);
                }

                src++;
            }

            this.run();
        }

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.objectListView1.SelectedObject == null)
            {
                return;
            }

            this.Preview = DxfParser.loadDxf((this.objectListView1.SelectedObject as DetailLoadInfo).Path);
            if (autoFit) fitAll();
        }

        public void ShowMessage(string text, MessageBoxIcon type)
        {
            MessageBox.Show(text, Text, MessageBoxButtons.OK, type);
        }

        public DialogResult ShowQuestion(string text)
        {
            return MessageBox.Show(text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Infos.Count == 0) { ShowMessage("There are no parts.", MessageBoxIcon.Warning); return; }
            if (ShowQuestion("Are you to sure to delete all items?") == DialogResult.No) return;
            Infos.Clear();
            objectListView1.SetObjects(Infos);
            Preview = null;
        }

        private List<SheetLoadInfo> sheetsInfos = new List<SheetLoadInfo>();

        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObjects.Count == 0) return;
            if (ShowQuestion($"Are you to sure to delete {objectListView1.SelectedObjects.Count} items?") == DialogResult.No) return;
            foreach (var item in objectListView1.SelectedObjects)
            {
                if (Preview != null && (item as DetailLoadInfo).Path == (Preview as RawDetail).Name) Preview = null;
                Infos.Remove(item as DetailLoadInfo);
            }
            objectListView1.SetObjects(Infos);
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(this.linkLabel1.Text);
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
            if (Preview == null) return;
            if (!(Preview is RawDetail raw)) return;

            GraphicsPath gp = new GraphicsPath();
            foreach (var item in raw.Outers)
            {
                gp.AddPolygon(item.Points.ToArray());
            }
            ctx3.FitToPoints(gp.PathPoints, 5);
        }

        bool autoFit = true;
        private void toolStripButton9_Click(object sender, EventArgs e)
        {

        }

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
    }
}