namespace DeepNestLib
{
    public class RectangleSheet : Sheet
    {
        public void Rebuild()
        {
            this.Points = new SvgPoint[] { };
            this.AddPoint(new SvgPoint(this.x, this.y));
            this.AddPoint(new SvgPoint(this.x + this.Width, this.y));
            this.AddPoint(new SvgPoint(this.x + this.Width, this.y + this.Height));
            this.AddPoint(new SvgPoint(this.x, this.y + this.Height));
        }
    }
}
