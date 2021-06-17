namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class PolygonBounds
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public PolygonBounds(double _x, double _y, double _w, double _h)
        {
            this.x = _x;
            this.y = _y;
            this.width = _w;
            this.height = _h;
        }
    }
}
