namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NFP : IStringify
    {
        public bool fitted
        {
            get { return this.sheet != null; }
        }

        public NFP sheet;

        public override string ToString()
        {
            var str1 = (this.Points != null) ? this.Points.Count() + string.Empty : "null";
            return $"nfp: id: {this.Id}; source: {this.Source}; rotation: {this.rotation}; points: {str1}";
        }

        public NFP()
        {
            this.Points = new SvgPoint[] { };
        }

        public string Name { get; set; }

        public void AddPoint(SvgPoint point)
        {
            var list = this.Points.ToList();
            list.Add(point);
            this.Points = list.ToArray();
        }

        public bool isBin;

        public void reverse()
        {
            this.Points = this.Points.Reverse().ToArray();
        }

        public double x { get; set; }

        public double y { get; set; }

        public double WidthCalculated
        {
            get
            {
                var maxx = this.Points.Max(z => z.x);
                var minx = this.Points.Min(z => z.x);

                return maxx - minx;
            }
        }

        public double HeightCalculated
        {
            get
            {
                var maxy = this.Points.Max(z => z.y);
                var miny = this.Points.Min(z => z.y);
                return maxy - miny;
            }
        }

        public SvgPoint this[int ind]
        {
            get
            {
                return this.Points[ind];
            }
        }

        public List<NFP> children;

        public int Length
        {
            get
            {
                return this.Points.Length;
            }
        }

        public int Id { get; set; }

        public double? Offsetx;
        public double? Offsety;
        public int? Source = null;
        private float rotation;

        public float Rotation
        {
            get
            {
                return this.rotation;
            }

            set
            {
                this.rotation = value;
            }
        }

        public SvgPoint[] Points;

        public float Area
        {
            get
            {
                float ret = 0;
                if (this.Points.Length < 3)
                {
                    return 0;
                }

                List<SvgPoint> pp = new List<SvgPoint>();
                pp.AddRange(this.Points);
                pp.Add(this.Points[0]);
                for (int i = 1; i < pp.Count; i++)
                {
                    var s0 = pp[i - 1];
                    var s1 = pp[i];
                    ret += (float)((s0.x * s1.y) - (s0.y * s1.x));
                }

                return (float)Math.Abs(ret / 2);
            }
        }

        internal void Push(SvgPoint svgPoint)
        {
            List<SvgPoint> points = new List<SvgPoint>();
            if (this.Points == null)
            {
                this.Points = new SvgPoint[] { };
            }

            points.AddRange(this.Points);
            points.Add(svgPoint);
            this.Points = points.ToArray();
        }

        public NFP slice(int v)
        {
            var ret = new NFP();
            List<SvgPoint> pp = new List<SvgPoint>();
            for (int i = v; i < this.Length; i++)
            {
                pp.Add(new SvgPoint(this[i].x, this[i].y));
            }

            ret.Points = pp.ToArray();
            return ret;
        }

        public string stringify()
        {
            throw new NotImplementedException();
        }
    }
}
