namespace DeepNestPort
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Windows.Forms;

    public class DrawingContext
    {
        public DrawingContext(PictureBox pb)
        {
            this.box = pb;

            this.bmp = new Bitmap(pb.Width, pb.Height);
            pb.SizeChanged += this.Pb_SizeChanged;
            this.gr = Graphics.FromImage(this.bmp);
            this.gr.SmoothingMode = SmoothingMode.AntiAlias;
            this.box.Image = this.bmp;

            pb.MouseDown += this.PictureBox1_MouseDown;
            pb.MouseUp += this.PictureBox1_MouseUp;
            pb.MouseMove += this.Pb_MouseMove;
            this.sx = this.box.Width / 2;
            this.sy = -this.box.Height / 2;
            pb.MouseWheel += this.Pb_MouseWheel;
        }

        private void Pb_MouseWheel(object sender, MouseEventArgs e)
        {
            float zold = this.zoom;
            if (e.Delta > 0)
            {
                this.zoom *= 1.5f;
            }
            else
            {
                this.zoom *= 0.5f;
            }

            if (this.zoom < 0.08)
            {
                this.zoom = 0.08f;
            }

            if (this.zoom > 1000)
            {
                this.zoom = 1000f;
            }

            var pos = this.box.PointToClient(Cursor.Position);

            this.sx = -((pos.X / zold) - this.sx - (pos.X / this.zoom));
            this.sy = (pos.Y / zold) + this.sy - (pos.Y / this.zoom);
        }

        public bool FocusOnMove = true;
        private void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            if (!FocusOnMove) return;
            box.Focus();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.isDrag = false;

            var p = this.box.PointToClient(Cursor.Position);
            var pos = this.box.PointToClient(Cursor.Position);
            var posx = (pos.X / this.zoom) - this.sx;
            var posy = (-pos.Y / this.zoom) - this.sy;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = this.box.PointToClient(Cursor.Position);
            var p = this.Transform(pos);

            if (e.Button == MouseButtons.Right)
            {
                this.isDrag = true;
                this.startx = pos.X;
                this.starty = pos.Y;
                this.origsx = this.sx;
                this.origsy = this.sy;
            }
        }

        private float startx;
        private float starty;
        private float origsx;
        private float origsy;
        private bool isDrag = false;

        private PictureBox box;
        public float sx;
        public float sy;
        public float zoom = 1;
        public Graphics gr;
        public Bitmap bmp;
        public bool InvertY = true;

        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + this.sx) * this.zoom, (this.InvertY ? (-1) : 1) * (p1.Y + this.sy) * this.zoom);
        }

        public virtual PointF Transform(double x, double y)
        {
            return new PointF(((float)x + this.sx) * this.zoom, (this.InvertY ? (-1) : 1) * ((float)y + this.sy) * this.zoom);
        }


        private void Pb_SizeChanged(object sender, EventArgs e)
        {
            this.bmp = new Bitmap(this.box.Width, this.box.Height);
            this.gr = Graphics.FromImage(this.bmp);

            this.box.Image = this.bmp;
        }

        public PointF GetPos()
        {
            var pos = this.box.PointToClient(Cursor.Position);
            var posx = (pos.X / this.zoom) - this.sx;
            var posy = (-pos.Y / this.zoom) - this.sy;

            return new PointF(posx, posy);
        }

        public void Update()
        {
            if (this.isDrag)
            {
                var p = this.box.PointToClient(Cursor.Position);

                this.sx = this.origsx + ((p.X - this.startx) / this.zoom);
                this.sy = this.origsy + (-(p.Y - this.starty) / this.zoom);
            }
        }

        public void Setup()
        {
            this.box.Invalidate();
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

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;

            sx = ((w / 2f) / zoom - x);
            sy = -((h / 2f) / zoom + y);

            var test = Transform(new PointF(x, y));

        }
    }
}
