namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;

  public class RawDetail
  {
    public List<LocalContour> Outers = new List<LocalContour>();
    public List<LocalContour> Holes = new List<LocalContour>();

    public RectangleF BoundingBox()
    {
      GraphicsPath gp = new GraphicsPath();
      foreach (var item in Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      return gp.GetBounds();
    }

    public string Name { get; set; }

    public bool TryImportFromRawDetail(int src, out NFP loadedNfp)
    {
      return this.TryGetNfp(src, out loadedNfp);
    }

    public bool TryGetNfp(int src, out NFP loadedNfp)
    {
      if (this == null)
      {
        loadedNfp = null;
        return false;
      }

      loadedNfp = this.ToNfp();
      if (loadedNfp == null)
      {
        return false;
      }

      loadedNfp.Source = src;
      return true;
    }

    public NFP ToNfp()
    {
      NFP po = null;
      List<NFP> nfps = new List<NFP>();
      foreach (var item in this.Outers)
      {
        var nn = new NFP();
        nfps.Add(nn);
        foreach (var pitem in item.Points)
        {
          nn.AddPoint(new SvgPoint(pitem.X, pitem.Y));
        }
      }

      if (nfps.Any())
      {
        var tt = nfps.OrderByDescending(z => z.Area).First();
        po = tt; // Reference caution needed here; should be cloning not messing with the original object?
        po.Name = Name;

        foreach (var r in nfps)
        {
          if (r == tt)
          {
            continue;
          }

          if (po.Children == null)
          {
            po.Children = new List<NFP>();
          }

          po.Children.Add(r);
        }
      }

      return po;
    }
  }
}