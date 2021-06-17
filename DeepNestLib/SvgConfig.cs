namespace DeepNestLib
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    public class SvgConfig
    {
        public float tolerance = 2f; // max bound for bezier->line segment conversion, in native SVG units
        public float toleranceSvg = 0.005f; // fudge factor for browser inaccuracy in SVG unit handling
    }
}
