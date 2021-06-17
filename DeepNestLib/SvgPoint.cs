namespace DeepNestLib
{
    public class SvgPoint
    {
        public bool exact = true;

        public override string ToString()
        {
            return "x: " + this.x + "; y: " + this.y;
        }

        public int id;

        public SvgPoint(double _x, double _y)
        {
            this.x = _x;
            this.y = _y;
        }

        public bool marked;
        public double x;
        public double y;
    }
}
