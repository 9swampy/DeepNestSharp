namespace DeepNestLib
{
  public class Sheet : NFP
  {
    public double Width;
    public double Height;

    public static Sheet NewSheet(int nameSuffix, int w = 3000, int h = 1500)
    {
      var tt = new RectangleSheet();
      tt.Name = "rectSheet" + nameSuffix;
      tt.Height = h;
      tt.Width = w;
      tt.Rebuild();

      return tt;
    }
  }
}
